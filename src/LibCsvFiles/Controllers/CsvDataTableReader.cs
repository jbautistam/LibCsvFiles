using System;
using System.Collections.Generic;
using System.Data;

using Bau.Libraries.LibCsvFiles.Models;

namespace Bau.Libraries.LibCsvFiles.Controllers
{
	/// <summary>
	///		Lector de un CSV en un DataTable
	/// </summary>
	public class CsvDataTableReader
	{
		public CsvDataTableReader(FileModel? fileParameters = null)
		{
			FileParameters = fileParameters ?? new FileModel();
		}

		/// <summary>
		///		Carga una página de un archivo en un dataTable
		/// </summary>
		public DataTable Load(string fileName, int page, int recordsPerPage, bool countRecords, CsvFiltersCollection? filters, out long totalRecords)
		{
			long record = 0;
			int offset = (page - 1) * recordsPerPage;
			DataTable table = new DataTable();
			bool end = false;

				// Lee los datos
				using (CsvReader reader = new CsvReader(FileParameters, null))
				{
					// Abre el archivo
					reader.Open(fileName);
					// Lee los registros
					while (reader.Read() && !end)
					{
						// Sólo se tiene en cuenta la fila si estamos en el filtro
						if (IsAtFilter(reader, filters))
						{
							// Añade la fila a la tabla
							if (record >= offset && record < offset + recordsPerPage)
							{
								// Añade las columnas al esquema o la fila de datos
								if (table.Columns.Count == 0)
									AddSchema(table, reader.Columns);
								// Añade los datos de la fila
								AddRow(table, reader);
							}
							// Incrementa el registro (sólo si estamos en el filtro para contabilizar correctamente el número de registros)
							record++;
						}
						// Comprueba si se deben contar todos los registros
						end = !countRecords && record >= offset + recordsPerPage;
					}
				}
				// Asigna el número de registros
				totalRecords = record;
				// Devuelve la tabla de datos
				return table;
		}

		/// <summary>
		///		Comprueba si se debe añadir el registro actual a la salida teniendo en cuentra el filtro
		/// </summary>
		private bool IsAtFilter(CsvReader reader, CsvFiltersCollection? filters)
		{
			if (filters is null || filters.Count == 0)
				return true;
			else
			{
				// Evalúa las condiciones sobre los campos
				for (int index = 0; index < reader.FieldCount; index++)
					if (!filters.Evaluate(reader.GetName(index), reader[index]))
						return false;
				// Si ha llegado hasta aquí es porque cumple con todas las condiciones
				return true;
			}
		}

		/// <summary>
		///		Añade el esquema a la tabla
		/// </summary>
		private void AddSchema(DataTable table, List<ColumnModel> columns)
		{
			foreach (ColumnModel column in columns)
				table.Columns.Add(column.Name, GetType(column));
		}

		/// <summary>
		///		Añade una fila a la tabla
		/// </summary>
		private void AddRow(DataTable table, CsvReader reader)
		{
			DataRow row = table.NewRow();

				// Añade las columnas
				foreach (DataColumn column in row.Table.Columns)
					row[column] = reader[column.ColumnName];
				// Añade la fila a la tabla
				table.Rows.Add(row);
		}

		/// <summary>
		///		Obtiene el tipo de una columna del CSV
		/// </summary>
		private Type GetType(ColumnModel column)
		{
			switch (column.Type)
			{
				case ColumnModel.ColumnType.Boolean:
					return typeof(bool);
				case ColumnModel.ColumnType.DateTime:
					return typeof(DateTime);
				case ColumnModel.ColumnType.Integer:
					return typeof(int);
				case ColumnModel.ColumnType.Decimal:
					return typeof(double);
				default:
					return typeof(string);
			}
		}

		/// <summary>
		///		Parámetros el archivo
		/// </summary>
		public FileModel FileParameters { get; }
	}
}
