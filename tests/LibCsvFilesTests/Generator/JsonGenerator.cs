#if DEBUG
using FluentAssertions;
using Bau.Libraries.LibCsvFiles;
using LibCsvFilesTest.Tools;

namespace LibCsvFilesTests.Generator;

/// <summary>
///		Generador de archivos JSON a partir de archivos CSV
/// </summary>
public class JsonGenerator
{
	/// <summary>
	///		Genera los archivos de respuesta de todos los archivos de solicitud
	/// </summary>
	[Fact(Skip = "Sólo cuando sea necesario regenerar los archivos JSON")]
	public void generate_all_response_files()
	{
		List<string> files = FileHelper.GetFiles(string.Empty, ".csv");

			// Recorre los esquemas e informes procesando las solicitudes / respuestas
			foreach (string file in files)
			{
				List<Dictionary<string, object?>> records = new();

					// Lee los datos del archivo
					using (CsvReader reader = FileHelper.GetCsvReader(file))
					{
						// Abre el archivo
						reader.Open(file);
						// Obtiene los datos
						while (reader.Read())
						{
							Dictionary<string, object?> record = new();

								// Añade los campos
								for (int index = 0; index < reader.FieldCount; index++)
									record.Add(reader.GetName(index), reader.GetValue(index));
								// Añade el registro a la lista
								records.Add(record);
						}
					}
					// Graba el archivo
					SaveFile(FileHelper.GetJsonFile(file), records);
			}
			// Una aserción para que realmente sea una prueba
			true.Should().BeTrue();
	}

	/// <summary>
	///		Graba el texto de un archivo de respuesta
	/// </summary>
	private void SaveFile(string file, List<Dictionary<string, object?>> records)
	{
		string json = System.Text.Json.JsonSerializer.Serialize(records);

			// Quita el directorio de depuración para que se graben en el archivo de proyectos
			file = file.Replace("\\bin\\Debug\\net7.0", string.Empty);
			file = file.Replace("/bin/Debug/net7.0", string.Empty);
			// Escribe el archivo
			File.WriteAllText(file, json);
	}
}
#endif