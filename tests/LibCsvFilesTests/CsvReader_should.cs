using FluentAssertions;

using LibCsvFilesTest.Tools;
using Bau.Libraries.LibCsvFiles;

namespace LibCsvFilesTests;

/// <summary>
///		Pruebas de <see cref="CsvReader"/>
/// </summary>
public class CsvReader_should
{
	/// <summary>
	///		Comprueba si se puede cargar un directorio con archivos CSV y comprueba los datos
	/// </summary>
	[Theory]
	[InlineData("")]
	public void parse_path_csv(string path)
	{
		string error = string.Empty;

			// Recorre los archivos comprobando los errores
			foreach (string file in FileHelper.GetFiles(path, ".csv"))
				error += CheckFile(file) + Environment.NewLine;
			// Muestra los errores
			error.Should().BeNullOrWhiteSpace();
	}

	/// <summary>
	///		Comprueba si se puede cargar un archivo CSV y comprueba los datos (cuando deseamos comprobar un archivo concreto)
	/// </summary>
	[Theory]
	[InlineData("Test.csv")]
	public void parse_csv(string file)
	{
		// hay que cambiar el json de este archivo para que no dé errores. Me quedé aquí
		string error;

			// Obtiene el nombre del archivo completo
			file = FileHelper.GetFullFileName(file);
			// Recorre los archivos comprobando los errores
			if (!File.Exists(file))
				error = $"Can't find the file {file}";
			else
				error = CheckFile(file);
			// Muestra los errores
			error.Should().BeNullOrWhiteSpace();
	}

	/// <summary>
	///		Comprueba el archivo de respuesta
	/// </summary>
	private List<Dictionary<string, object?>>? LoadJson(string file)
	{
		string json = File.ReadAllText(FileHelper.GetJsonFile(file));

			// Devuevle la lista de registros
			return System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json);
	}

	/// <summary>
	///		Comprueba la lectura del archivo CSV con los registros leidos de un archivo JSON
	/// </summary>
	private string CheckFile(string file)
	{
		string error;
		List<Dictionary<string, object?>>? records = LoadJson(file);

			// Comprueba que realmente haya registros
			records.Should().NotBeNull();
			// Comprueba el contenido del CSV con respecto al JSON
			if (records is not null)
			{
				try
				{
					error = CheckFile(file, records);
				}
				catch (Exception exception)
				{
					error = $"Exception when parse file. {exception.Message}";
				}
			}
			else
				error = $"Can't read the file {file}";
			// Añade el nombre de archivo
			if (!string.IsNullOrWhiteSpace(error))
				error = $"{Path.GetFileName(file)} {error}";
			// Devuelve el resultado
			return error;
	}

	/// <summary>
	///		Comprueba la lectura del archivo CSV con los registros leidos de un archivo JSON
	/// </summary>
	private string CheckFile(string file, List<Dictionary<string, object?>> records)
	{
		string error = string.Empty;
		int index = 0;

			// Lee los datos y lo compara con los registros
			using (CsvReader reader = FileHelper.GetCsvReader(file))
			{
				// Abre el archivo
				reader.Open(file);
				// Compara los registros
				while (reader.Read() && string.IsNullOrWhiteSpace(error))
				{
					// Compara los datos
					foreach (KeyValuePair<string, object?> record in records[index])
						if (reader.GetOrdinal(record.Key) < 0)
							error += $"Can't find the field {record.Key} at CSV. Row: {index.ToString()}";
						else if (reader[record.Key] is null && record.Value is not null)
							error = $"The value of {record.Key} is null at CSV but at json is {record.Value.ToString()}. Row: {index.ToString()}";
						else
						{
							object? csvValue = reader[record.Key];

								if (record.Value is System.Text.Json.JsonElement jsonElement)
								{
									object? jsonValue = ConvertJsonObject(jsonElement, csvValue);

										if (csvValue is null && jsonValue is not null)
											error = $"The value of CSV is NULL, but at json is {jsonValue.ToString()}. Row: {index.ToString()}";
										else if (csvValue is not null && jsonValue is null)
											error = $"The value of CSV is {csvValue.ToString()}, but at json is NULL. Row: {index.ToString()}";
										else if (csvValue is not null && jsonValue is not null)
										{
											if (csvValue.GetType() != jsonValue.GetType())
												error = $"The type of CSV is {csvValue.GetType().ToString()}, but at json in {jsonValue.GetType().ToString()}. Row: {index.ToString()}";
											else if (csvValue.ToString() != jsonValue.ToString())
												error = $"The value of CSV is '{csvValue?.ToString()}', but at json is {jsonValue?.ToString()}. Row: {index.ToString()}";
										}
								}
								else if (csvValue != record.Value)
									error = $"The value of CSV is '{csvValue?.ToString()}', but at json is {record.Value?.ToString()}. Row: {index.ToString()}";
						}
					// Incrementa el índice de registro
					index++;
				}
			}
			// Comprueba el número de registros
			if (index != records.Count)
				error = $"The records number of json {records.Count:#,##0} is greater than csv lines {index:#,##0}";
			// Devuelve el contenido del error
			return error;
	}

	/// <summary>
	///		Convierte un valor de Json en un valor .Net
	/// </summary>
	private object? ConvertJsonObject(System.Text.Json.JsonElement element, object? value)
	{
		switch (element.ValueKind)
		{
			case System.Text.Json.JsonValueKind.Null:
				return null;
			case System.Text.Json.JsonValueKind.String:
				if (value is DateTime)
					return element.GetDateTime();
				else
					return element.GetString();
			case System.Text.Json.JsonValueKind.True:
				return true;
			case System.Text.Json.JsonValueKind.False:
				return false;
			case System.Text.Json.JsonValueKind.Object:
				if (value is DateTime)
					return element.GetDateTime();
				else
					return null;
			case System.Text.Json.JsonValueKind.Number:
				if (value is double)
					return element.GetDouble();
				else
					return element.GetInt32();
			default:
				return null;
		}
	}
}
