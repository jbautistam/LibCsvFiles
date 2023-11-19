using Bau.Libraries.LibCsvFiles.Models;

namespace Bau.Libraries.LibCsvFiles;

/// <summary>
///		Clase para escritura sobre un archivo CSV
/// </summary>
public class CsvWriter : IDisposable
{
	public CsvWriter(FileModel fileParameters)
	{
		FileParameters = fileParameters ?? new FileModel();
	}

	/// <summary>
	///		Abre el archivo
	/// </summary>
	public void Open(string fileName, System.Text.Encoding? encoding = null)
	{
		FileWriter = File.CreateText(fileName);
	}

	/// <summary>
	///		Abre el archivo
	/// </summary>
	public void Open(StreamWriter stream)
	{
		FileWriter = stream;
	}

	/// <summary>
	///		Envía los datos restantes al stream
	/// </summary>
	public void Flush()
	{
		if (FileWriter != null)
			FileWriter.Flush();
	}

	/// <summary>
	///		Cierra el stream
	/// </summary>
	public void Close()
	{
		if (FileWriter != null)
		{
			// Envía los datos restantes al archivo
			Flush();
			// Cierra el stream
			FileWriter.Close();
			FileWriter = null;
		}
	}

	/// <summary>
	///		Escribe las cabeceras
	/// </summary>
	public void WriteHeaders(List<string> headers)
	{
		WriteLine(headers);
	}

	/// <summary>
	///		Escribe las cabeceras
	/// </summary>
	public void WriteHeaders(List<ColumnModel> columns)
	{
		List<string> headers = new List<string>();

			// Añade las cabeceras
			foreach (ColumnModel column in columns)
				headers.Add(column.Name);
			// Escribe las cabeceras
			WriteLine(headers);
	}

	/// <summary>
	///		Escribe una línea
	/// </summary>
	public void WriteRow(List<(ColumnModel.ColumnType type, object value)> values)
	{
		List<string> rowData = new();

			// Convierte los valores
			foreach ((ColumnModel.ColumnType type, object value) in values)
				rowData.Add(ConvertValue(type, value));
			// Escribe la línea
			WriteLine(rowData);
	}

	/// <summary>
	///		Escribe una línea
	/// </summary>
	public void WriteRow(List<object> values)
	{
		List<string> rowData = new();

			// Convierte los valores
			foreach (object value in values)
				rowData.Add(ConvertValue(GetColumnType(value), value));
			// Escribe la línea
			WriteLine(rowData);
	}

	/// <summary>
	///		Obtiene el tipo de columna de un objeto
	/// </summary>
	private ColumnModel.ColumnType GetColumnType(object value)
	{
		switch (value)
		{
			case bool _:
				return ColumnModel.ColumnType.Boolean;
			case DateTime _:
				return ColumnModel.ColumnType.DateTime;
			case int _:
			case long _:
				return ColumnModel.ColumnType.Integer;
			case float _:
			case double _:
			case decimal _:
				return ColumnModel.ColumnType.Decimal;
			default:
				return ColumnModel.ColumnType.String;
		}
	}

	/// <summary>
	///		Convierte un valor a cadena
	/// </summary>
	private string ConvertValue(ColumnModel.ColumnType type, object value)
	{
		if (value is null)
			return string.Empty;
		else
			switch (type)
			{
				case ColumnModel.ColumnType.Unknown:
					return string.Empty;
				case ColumnModel.ColumnType.String:
					return value.ToString() ?? string.Empty;
				case ColumnModel.ColumnType.Boolean:
					if ((bool) value)
						return FileParameters.TrueValue;
					else
						return FileParameters.FalseValue;
				case ColumnModel.ColumnType.Integer:
				case ColumnModel.ColumnType.Decimal:
					return ConvertNumeric(value, FileParameters.DecimalSeparator);
				case ColumnModel.ColumnType.DateTime:
					return ConvertDateTime(value as DateTime?, FileParameters.DateFormat);
				default:
					return string.Empty;
			}
	}

	/// <summary>
	///		Convierte un valor numérico a cadena
	/// </summary>
	private string ConvertNumeric(object value, char decimalSeparator)
	{
		//TODO --> Le falta la conversión de puntos de miles, el número de decimales y el signo a izquierda / derecha
		switch (value)
		{
			case long converted:
				return converted.ToString();
			case int converted:
				return converted.ToString();
			case decimal converted:
				return converted.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace('.', decimalSeparator);
			case double converted:
				return converted.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace('.', decimalSeparator);
			default:
				return value.ToString() ?? string.Empty;
		}
	}

	/// <summary>
	///		Convierte una fecha
	/// </summary>
	private string ConvertDateTime(DateTime? value, string format)
	{
		if (value == null)
			return string.Empty;
		else
			return (value ?? DateTime.Now).ToString(format);
	}

	/// <summary>
	///		Escribe una línea
	/// </summary>
	private void WriteLine(List<string> columns)
	{
		System.Text.StringBuilder builder = new System.Text.StringBuilder();

			// Añade los datos de la fila
			foreach (string column in columns)
			{
				// Añade el separador de celdas
				if (builder.Length > 0)
					builder.Append(FileParameters.Separator);
				// Añade el campo
				builder.Append(Normalize(column, FileParameters.Separator));
			}
			// Escribe la cadena
			FileWriter.WriteLine(builder);
	}

	/// <summary>
	///		Normaliza el valor a escribir teniendo en cuenta el separador de campos
	/// </summary>
	private string Normalize(string value, char separator)
	{
		// Si en la cadena que se va a escribir, aparece el separador o hay un salto de línea, hay que rodear la cadena por comillas pero
		// si la cadena ya tiene comillas hay que duplicar las comillas internas
		if (!string.IsNullOrEmpty(value) && (value.Contains(separator.ToString(), StringComparison.CurrentCultureIgnoreCase) ||
											 value.Contains('\r') || value.Contains('\n')))
		{ 
			// Duplica las "comillas" internas del campo
			value = value.Replace("\"", "\"\"");
			// Añade las "comillas" iniciales y finales
			value = $"\"{value}\"";
		}
		// Devuelve el valor normalizado
		return value;
	}

	/// <summary>
	///		Libera la memoria
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			// Cierra el archivo
			if (disposing)
				Close();
			// Indica que se ha liberado la memoria
			Disposed = true;
		}
	}

	/// <summary>
	///		Libera la memoria
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
	}

	/// <summary>
	///		Parámetros del archivo
	/// </summary>
	public FileModel FileParameters { get; }

	/// <summary>
	///		Columnas
	/// </summary>
	public List<ColumnModel> Columns { get; } = new();

	/// <summary>
	///		Handle de archivo
	/// </summary>
	private StreamWriter? FileWriter { get; set; }

	/// <summary>
	///		Indica si se ha liberado el archivo
	/// </summary>
	public bool Disposed { get; private set; }
}
