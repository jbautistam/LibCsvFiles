using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Bau.Libraries.LibCsvFiles.Controllers
{
	/// <summary>
	///		Generador de un archivo CSV a partir de un <see cref="IDataReader"/>
	/// </summary>
	public class CsvDataReaderWriter
	{
		// Eventos públicos
		public event EventHandler<EventArguments.AffectedEvntArgs> Progress;

		public CsvDataReaderWriter(Models.FileModel fileParameters = null, int notifyAfter = 200_000)
		{
			FileParameters = fileParameters ?? new Models.FileModel();
			NotifyAfter = notifyAfter;
		}

		/// <summary>
		///		Graba el archivo
		/// </summary>
		public void Save(IDataReader reader, string targetFileName)
		{
			long rows = 0;

				// Escribe los datos
				using (CsvWriter writer = new CsvWriter(new Models.FileModel()))
				{
					// Abre el archivo destino
					writer.Open(targetFileName);
					// Añade las cabeceras
					writer.WriteHeaders(GetColumns(reader));
					// Escribe las filas
					while (reader.Read())
					{
						List<object> values = new List<object>();

							// Asigna los valores
							for (int index = 0; index < reader.FieldCount; index++)
								values.Add(reader.GetValue(index));
							// Escribe los datos
							writer.WriteRow(values);
							// Lanza el evento
							if (++rows % NotifyAfter == 0)
								Progress?.Invoke(this, new EventArguments.AffectedEvntArgs(rows));
					}
				}
				// Lanza el último registro de progreso
				Progress?.Invoke(this, new EventArguments.AffectedEvntArgs(rows));
		}

		/// <summary>
		///		Graba el archivo
		/// </summary>
		public async Task SaveAsync(DbDataReader reader, string targetFileName, CancellationToken cancellationToken)
		{
			long rows = 0;

				// Escribe los datos
				using (CsvWriter writer = new CsvWriter(new Models.FileModel()))
				{
					// Abre el archivo destino
					writer.Open(targetFileName);
					// Añade las cabeceras
					writer.WriteHeaders(GetColumns(reader));
					// Escribe las filas
					while (!cancellationToken.IsCancellationRequested && await reader.ReadAsync(cancellationToken))
					{
						List<object> values = new List<object>();

							// Asigna los valores
							for (int index = 0; index < reader.FieldCount; index++)
								values.Add(reader.GetValue(index));
							// Escribe los datos
							writer.WriteRow(values);
							// Lanza el evento
							if (++rows % NotifyAfter == 0)
								Progress?.Invoke(this, new EventArguments.AffectedEvntArgs(rows));
					}
				}
		}

		/// <summary>
		///		Obtiene las columnas
		/// </summary>
		private List<string> GetColumns(IDataReader reader)
		{
			List<string> columns = new List<string>();

				// Obtiene las columnas
				for (int index = 0; index < reader.FieldCount; index++)
					columns.Add(reader.GetName(index));
				// Devuelve la colección
				return columns;
		}

		/// <summary>
		///		Parámetros el archivo
		/// </summary>
		public Models.FileModel FileParameters { get; }

		/// <summary>
		///		Número de registros a escribir antes de lanzar un evento de progreso
		/// </summary>
		public int NotifyAfter { get; set; }
	}
}
