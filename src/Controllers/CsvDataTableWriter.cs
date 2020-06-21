using System;
using System.Collections.Generic;
using System.Data;

using Bau.Libraries.LibCsvFiles.Models;

namespace Bau.Libraries.LibCsvFiles.Controllers
{
	/// <summary>
	///		Controlador para escribir un dataTable en un archivo
	/// </summary>
	public class CsvDataTableWriter
	{
		public CsvDataTableWriter(FileModel fileParameters = null)
		{
			FileParameters = fileParameters ?? new FileModel();
		}

		/// <summary>
		///		Graba los datos de un <see cref="DataTable"/> en un archivo
		/// </summary>
		public void Save(DataTable table, string fileName, System.Text.Encoding encoding = null)
		{
			using (CsvWriter writer = new CsvWriter(FileParameters))
			{
				List<ColumnModel> columns = GetColumns(table);

					// Abre el archivo
					writer.Open(fileName, encoding);
					// Escribe las cabeceras
					if (FileParameters.WithHeader)
						writer.WriteHeaders(GetHeaders(columns));
					// Escribe las filas
					foreach (DataRow row in table.Rows)
						writer.WriteRow(GetRowValues(columns, row));
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
		///		Parámetros de definición del archivo
		/// </summary>
		private FileModel FileParameters { get; }
	}
}
