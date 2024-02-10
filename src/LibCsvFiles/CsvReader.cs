using System.Data;

using Bau.Libraries.LibCsvFiles.Extensors;
using Bau.Libraries.LibCsvFiles.Models;

namespace Bau.Libraries.LibCsvFiles;

/// <summary>
///		Implementación de <see cref="IDataReader"/> para archivos CSV
/// </summary>
public class CsvReader : IDataReader
{
	// Eventos públicos
	public event EventHandler<EventArguments.AffectedEvntArgs>? ReadBlock;
	// Variables privadas
	private bool _streamOpenedFromReader;
	private StreamReader? _fileReader;
	private object?[] _recordsValues = new object?[1];
	private System.Text.StringBuilder _actualLine = new();
	private long _row;

	public CsvReader(FileModel? definition, List<ColumnModel>? columns, int notifyAfter = 10_000)
	{
		FileDefinition = definition ?? new FileModel();
		Columns = columns ?? new List<ColumnModel>();
		NotifyAfter = notifyAfter;
	}

	/// <summary>
	///		Abre el archivo
	/// </summary>
	public void Open(string fileName)
	{
		// Indica que el stream se ha abierto en la librería
		_streamOpenedFromReader = true;
		// Abre el archivo sobre el stream
		Open(new StreamReader(fileName, true));
	}

	/// <summary>
	///		Abre el datareader sobre el stream
	/// </summary>
	public void Open(StreamReader stream)
	{
		// Guarda el stream
		_fileReader = stream;
		// Lee las cabeceras
		ReadHeader();
		// e indica que aún no se ha leido ninguna línea
		_row = 0;
	}

	/// <summary>
	///		Interpreta la cabecera del archivo si es necesario
	///		Sólo se interpreta la cabecera cuando la definición indica que tiene cabeceras
	/// </summary>
	private void ReadHeader()
	{
		if (_fileReader is not null && !_fileReader.EndOfStream && FileDefinition.WithHeader)
		{
			ReadOnlySpan<char> line = _fileReader.ReadLine();

				// Interpreta las columnas (si no se han definido)
				if (Columns.Count == 0 && line.Length > 0)
				{
					int start = 0;

						// Mientras quede algo para leer
						while (start < line.Length)
						{
							int length = GetLengthField(line, start);

								// Añade el nombre de la columna
								Columns.Add(new ColumnModel
													{
														Name = line.Slice(start, length).ToString(),
														Type = ColumnModel.ColumnType.String
													}
											);
								// Incrementa la columna de inicio
								start += length + 1;
						}
				}
				// Genera el array de registros
				if (Columns.Count > 0)
					_recordsValues = new object?[Columns.Count];
		}
	}

	/// <summary>
	///		Lee un registro
	/// </summary>
	public bool Read()
	{
		bool readed = false;

			// Lee la línea
			ReadLine();
			// Interpreta los datos
			if (_actualLine.Length > 0)
			{
				// Interpreta la línea
				ConvertFields(_actualLine.ToString());
				// Incrementa el número de línea y lanza el evento
				_row++;
				RaiseEventReadBlock(_row);
				// Indica que se han leido datos
				readed = true;
			}
			// Limpia la línea leida
			_actualLine.Clear();
			// Devuelve el valor que indica si se han leído datos
			return readed;
	}

	/// <summary>
	///		Convierte las cadenas leidas
	/// </summary>
	private void ConvertFields(ReadOnlySpan<char> line)
	{
		int start = 0, length;

			// Convierte los campos
			for (int column = 0; column < _recordsValues.Length; column++)
			{
				// Vacía el valor
				_recordsValues[column] = null;
				// Obtiene el siguiente campo
				length = GetLengthField(line, start);
				// Si hay algo...
				if (length > 0)
				{
					// Obtiene el valor (normaliza la línea para quitar las comillas de inicio a fin)
					_recordsValues[column] = ConvertField(Columns[column], Normalize(line.Slice(start, length)));
					// Asigna el índice de inicio
					start += length + 1;
				}
				else// es un campo nulo, pasamos al siguiente
					start++;
			}
	}

	/// <summary>
	///		Normaliza la línea: el inicio y fin pueden ser comillas y además puede tener un "" en la cadena
	/// </summary>
	private ReadOnlySpan<char> Normalize(ReadOnlySpan<char> field)
	{
		// Si realmente tenemos espacio para tener comillas apreciables
		if (field.Length > 1)
		{
			// Quita las comillas de inicio y fin
			if (field[0] == '"' && field[^1] == '"')
			{
				if (field.Length == 2)
					field = string.Empty;
				else
					field = field[1..^1];
			}
			// Si hay dobles comillas (es decir, al menos hay una comilla dentro) se quita una de ellas
			if (field.IndexOf('"') >= 0)
				field = field.ToString().Replace("\"\"", "\"").AsSpan();
		}
		// Devuelve el campo
		return field;
	}

	/// <summary>
	///		Lee la siguiente línea no vacía del archivo
	/// </summary>
	private void ReadLine()
	{
		if (_fileReader is not null) // Si realmente tenemos archivo (no queremos esta comparación en el while)
		{
			bool mustReadNextLine = true;
			int quotes = 0;

				// Lee la siguiente línea no vacía y se salta las líneas de cabecera
				while (!_fileReader.EndOfStream && mustReadNextLine)
				{
					ReadOnlySpan<char> part = _fileReader.ReadLine();

						// Resetea el valor que indica que la siguiente vez se debe leer la siguiente línea
						mustReadNextLine = false;
						// Cuenta el número de comillas (los acumula porque puede que haya saltos de línea intermedios que tengan también comillas)
						quotes += CountQuotes(part);
						// Se debe leer la siguiente línea si el número de caracteres de comillas no es par, eso quiere decir que ha habido un salto
						// de línea en un campo
						if (quotes % 2 != 0)
							mustReadNextLine = true;
						// Añade la sección a la línea
						_actualLine.Append(part);
						// Si se debe leer una línea más, se añade el salto de línea que se ha quitado en el ReadLine
						if (mustReadNextLine)
							_actualLine.Append(Environment.NewLine);
				}
		}
	}

	/// <summary>
	///		Cuenta el número de comillas que hay en una línea
	/// </summary>
	private int CountQuotes(ReadOnlySpan<char> line)
	{
		int number = 0;

			// Cuenta las comillas de la cadena
			foreach (char chr in line)
				if (chr == '"')
					number++;
			// Devuelve el número contado
			return number;
	}

	/// <summary>
	///		Lanza el evento de lectura de un bloque
	/// </summary>
	private void RaiseEventReadBlock(long row)
	{
		if (NotifyAfter > 0 && row % NotifyAfter == 0)
			ReadBlock?.Invoke(this, new EventArguments.AffectedEvntArgs(row));
	}

	/// <summary>
	///		Obtiene la longitud del siguiente campo
	/// </summary>
	private int GetLengthField(ReadOnlySpan<char> line, int start)
	{
		bool end = false, isInQuotes = false, atScape = false;
		int index = start, length = 0;

			// Calcula la longitud del siguiente campo
			while (index < line.Length && !end)
			{
				char actual = line[index];
				char next = '#';

					// Obtiene el siguiente carácter
					if (index < line.Length - 1)
						next = line[index + 1];
					// Busca el separador
					if (actual == FileDefinition.Separator && !isInQuotes)
						end = true;
					else if (actual == '"')
					{
						// Sea lo que sea añade el carácter a la longitud
						length++;
						// Si es una comilla y lo siguiente es una comilla: se toma como carácter de escape
						if (next != '"')
						{
							// Comprueba si debe cambiar si está entre comillas
							if (atScape)
								atScape = false;
							else
								isInQuotes = !isInQuotes;
						}
						else
							atScape = true;
					}
					else
						length++;
					// Pasa al siguiente carácter
					index++;
			}
			// Devuelve la longitud del campo
			return length;
	}

	/// <summary>
	///		Convierte una cadena en el contenido de una columna
	/// </summary>
	private object? ConvertField(ColumnModel column, ReadOnlySpan<char> field)
	{
		// Convierte una cadena en un objeto
		if (field.Length > 0)
			switch (column.Type)
			{
				case ColumnModel.ColumnType.DateTime:
					return field.GetDateTime(FileDefinition.DateFormat);
				case ColumnModel.ColumnType.Boolean:
					return field.Equals(FileDefinition.TrueValue, StringComparison.CurrentCultureIgnoreCase);
				case ColumnModel.ColumnType.Integer:
					return field.GetInt(0);
				case ColumnModel.ColumnType.Decimal:
					if (FileDefinition.DecimalSeparator == '.')
						return field.GetDouble(0);
					else
						return field.ToString().Replace(FileDefinition.DecimalSeparator, '.').GetDouble(0);
				default:
					return field.ToString();
			}
		// Si ha llegado hasta aquí es porque no ha encontrado ningún valor
		return null;
	}

	/// <summary>
	///		Cierra el archivo
	/// </summary>
	public void Close()
	{
		if (_streamOpenedFromReader && _fileReader is not null)
		{
			// Cierra el archivo
			_fileReader.Close();
			// y libera los datos
			_fileReader = null;
		}
	}

	/// <summary>
	///		Obtiene el nombre del campo
	/// </summary>
	public string GetName(int i) => Columns[i].Name;

	/// <summary>
	///		Obtiene el nombre del tipo de datos
	/// </summary>
	public string GetDataTypeName(int i) => GetFieldType(i).Name;

	/// <summary>
	///		Obtiene el tipo de un campo
	/// </summary>
	public Type GetFieldType(int i)
	{
		if (_recordsValues is null)
			return GetColumnType(Columns[i].Type);
		else
			return _recordsValues[i].GetType();
	}

	/// <summary>
	///		Obtiene el tipo de una columna
	/// </summary>
	private Type GetColumnType(ColumnModel.ColumnType type)
	{
		return type switch
					{
						ColumnModel.ColumnType.Boolean => typeof(bool),
						ColumnModel.ColumnType.DateTime => typeof(DateTime),
						ColumnModel.ColumnType.Integer => typeof(int),
						ColumnModel.ColumnType.Decimal => typeof(double),
						_ => typeof(string),
					};
	}

	/// <summary>
	///		Obtiene el valor de un campo
	/// </summary>
	public object GetValue(int i) 
	{
		if (_recordsValues is null || i > _recordsValues.Length - 1)
			return null;
		else
			return _recordsValues[i];
	}

	/// <summary>
	///		Obtiene el esquema
	/// </summary>
	public DataTable GetSchemaTable()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///		Obtiene una serie de valores
	/// </summary>
	public int GetValues(object[] values)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///		Obtiene un valor bool de un campo
	/// </summary>
	public bool GetBoolean(int i) => GetDataValue<bool>(i);

	/// <summary>
	///		Obtiene un valor byte de un campo
	/// </summary>
	public byte GetByte(int i) => GetDataValue<byte>(i);

	/// <summary>
	///		Obtiene una serie de bytes
	/// </summary>
	public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///		Obtiene un valor char de un campo
	/// </summary>
	public char GetChar(int i) => GetDataValue<char>(i);

	/// <summary>
	///		Obtiene una serie de caracteres
	/// </summary>
	public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///		Obtiene un Guid
	/// </summary>
	public Guid GetGuid(int i) => GetDataValue<Guid>(i);

	/// <summary>
	///		Obtiene un entero de 16
	/// </summary>
	public short GetInt16(int i) => GetDataValue<short>(i);

	/// <summary>
	///		Obtiene un entero de 32
	/// </summary>
	public int GetInt32(int i) => GetDataValue<int>(i);

	/// <summary>
	///		Obtiene un entero largo
	/// </summary>
	public long GetInt64(int i) => GetDataValue<long>(i);

	/// <summary>
	///		Obtiene un valor flotante
	/// </summary>
	public float GetFloat(int i) => GetDataValue<float>(i);

	/// <summary>
	///		Obtiene un valor doble
	/// </summary>
	public double GetDouble(int i) => GetDataValue<double>(i);

	/// <summary>
	///		Obtiene una cadena
	/// </summary>
	public string GetString(int i)
	{
		object? value = GetValue(i);

			if (value is string resultValue)
				return resultValue;
			else if (value is null)
				return null;
			else
				return value.ToString();
	}

	/// <summary>
	///		Obtiene un valor decimal
	/// </summary>
	public decimal GetDecimal(int i) => GetDataValue<decimal>(i);

	/// <summary>
	///		Obtiene una fecha
	/// </summary>
	public DateTime GetDateTime(int i) => GetDataValue<DateTime>(i);

	public IDataReader GetData(int i)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	///		Obtiene un campo de un tipo determinado
	/// </summary>
	private TypeData? GetDataValue<TypeData>(int column)
	{
		object? value = GetValue(column);

			if (value is TypeData resultValue)
				return resultValue;
			else
				return default;
	}

	/// <summary>
	///		Obtiene el índice de un campo a partir de su nombre
	/// </summary>
	public int GetOrdinal(string name)
	{
		// Obtiene el índice del registro
		if (!string.IsNullOrWhiteSpace(name))
			for (int index = 0; index < Columns.Count; index++)
				if (Columns[index].Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
					return index;
		// Si ha llegado hasta aquí es porque no ha encontrado el campo
		return -1;
	}

	/// <summary>
	///		Indica si el campo es un DbNull
	/// </summary>
	public bool IsDBNull(int index) => index >= _recordsValues.Length || _recordsValues?[index] is null || _recordsValues[index] is DBNull;

	/// <summary>
	///		Los CSV sólo devuelven un Resultset, de todas formas, DbDataAdapter espera este valor
	/// </summary>
	public bool NextResult() => false;

	/// <summary>
	///		Libera la memoria
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			// Libera los datos
			if (disposing)
				Close();
			// Indica que se ha liberado
			IsDisposed = true;
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
	///		Profundidad del recordset
	/// </summary>
	public int Depth => 0; 

	/// <summary>
	///		Indica si está cerrado
	/// </summary>
	public bool IsClosed => _fileReader is null;

	/// <summary>
	///		Registros afectados
	/// </summary>
	public int RecordsAffected => -1;

	/// <summary>
	///		Parámetros de definición del archivo
	/// </summary>
	public FileModel FileDefinition { get; }

	/// <summary>
	///		Columnas
	/// </summary>
	public List<ColumnModel> Columns { get; }

	/// <summary>
	///		Bloque de filas para las que se lanza el evento de grabación
	/// </summary>
	public int NotifyAfter { get; }

	/// <summary>
	///		Indica si se ha liberado el recurso
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <summary>
	///		Número de campos a partir de las columnas
	/// </summary>
	/// <remarks>
	///		Lo primero que hace un BulkCopy es ver el número de campos que tiene, si no se ha leido la cabecera puede
	///	que aún no tengamos ningún número de columnas, por eso se lee por primera vez
	/// </remarks>
	public int FieldCount  => Columns.Count; 

	/// <summary>
	///		Indizador por número de campo
	/// </summary>
	public object this[int i] 
	{ 
		get 
		{ 
			if (_recordsValues is null)
				return null;
			else 
				return _recordsValues[i]; 
		}
	}

	/// <summary>
	///		Indizador por nombre de campo
	/// </summary>
	public object this[string name]
	{ 
		get 
		{
			if (_recordsValues is null)
				return null;
			else
			{
				int index = GetOrdinal(name);

					if (index < 0 || index >= _recordsValues.Length)
						return null;
					else
						return _recordsValues[index]; 
			}
		}
	}
}
