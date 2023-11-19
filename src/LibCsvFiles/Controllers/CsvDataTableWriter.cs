using System.Data;

using Bau.Libraries.LibCsvFiles.Models;

namespace Bau.Libraries.LibCsvFiles.Controllers;

/// <summary>
///		Controlador para escribir una serie de dataTable en un archivo
/// </summary>
public class CsvDataTableWriter : IDisposable
{
	// Variables privadas
	private CsvWriter? _writer = null;
	private bool _writenHeaders;

	public CsvDataTableWriter(FileModel? fileParameters = null)
	{
		FileParameters = fileParameters ?? new FileModel();
	}

	/// <summary>
	///		Abre el archivo para escritura
	/// </summary>
	public void Open(string fileName, System.Text.Encoding? encoding = null)
	{
		// Crea el escritor
		_writer = new CsvWriter(FileParameters);
		// Abre el archivo
		_writer.Open(fileName, encoding);
		// Indica que aún no se ha escrito ninguna cabecera
		_writenHeaders = false;
	}

	/// <summary>
	///		Graba los datos de un <see cref="DataTable"/> en un archivo
	/// </summary>
	public void Save(DataTable table)
	{
		if (_writer is null)
			throw new NotImplementedException("File is closed");
		else
		{
			List<ColumnModel> columns = GetColumns(table);

				// Escribe las cabeceras
				if (FileParameters.WithHeader && !_writenHeaders)
				{
					// Escribe las cabeceras
					_writer.WriteHeaders(GetHeaders(columns));
					// Indica que ya se han escrito las cabeceras
					_writenHeaders = true;
				}
				// Escribe las filas
				foreach (DataRow row in table.Rows)
					_writer.WriteRow(GetRowValues(columns, row));
		}
	}

	/// <summary>
	///		Obtiene la definición de columnas de la tabla
	/// </summary>
	private List<ColumnModel> GetColumns(DataTable table)
	{
		List<ColumnModel> columns = new List<ColumnModel>();

			// Añade las columnas
			foreach (DataColumn column in table.Columns)
				columns.Add(new ColumnModel
									{
										Name = column.ColumnName,
										Type = ColumnModel.GetColumnType(column.DataType)
									}
						   );
			// Devuelve la lista de columnas
			return columns;
	}

	/// <summary>
	///		Obtiene las cabeceras de la tabla
	/// </summary>
	private List<string> GetHeaders(List<ColumnModel> columns)
	{
		List<string> headers = new List<string>();

			// Obtiene las cabeceras
			foreach (ColumnModel column in columns)
				headers.Add(column.Name);
			// Devuelve la colección de cabeceras
			return headers;
	}

	/// <summary>
	///		Obtiene los valores de una fila
	/// </summary>
	private List<(ColumnModel.ColumnType type, object value)> GetRowValues(List<ColumnModel> columns, DataRow row)
	{
		List<(ColumnModel.ColumnType type, object value)> result = new List<(ColumnModel.ColumnType type, object value)>();

			// Obtiene los valores
			for (int index = 0; index < columns.Count; index++)
				result.Add((columns[index].Type, row[index]));
			// Devuelve el resultado
			return result;
	}

	/// <summary>
	///		Libera la memoria
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			// Libera la memoria
			if (disposing && _writer != null)
			{
				_writer.Dispose();
				_writer = null;
			}
			// Indica que se ha liberado la memoria
			IsDisposed = true;
		}
	}

	/// <summary>
	///		Libera la memoria
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///		Parámetros de definición del archivo
	/// </summary>
	private FileModel FileParameters { get; }

	/// <summary>
	///		Indica si se ha liberado la memoria
	/// </summary>
	public bool IsDisposed { get; private set; }
}
