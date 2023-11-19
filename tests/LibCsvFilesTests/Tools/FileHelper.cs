using Bau.Libraries.LibCsvFiles;
using Bau.Libraries.LibCsvFiles.Models;
using LibCsvFilesTest.Models;

namespace LibCsvFilesTest.Tools;

/// <summary>
///		Clase de ayuda para tratamiento de archivos
/// </summary>
public static class FileHelper
{
	/// <summary>
	///		Obtiene el nombre completo de un archivo
	/// </summary>
	public static string GetFullFileName(string fileName) => Path.Combine(GetDataPath(), fileName);

	/// <summary>
	///		Obtiene el directorio de archivos de datos del proyecto
	/// </summary>
	public static string GetDataPath() => Path.Combine(GetExecutionPath(), "Data");

	/// <summary>
	///		Obtiene el directorio de ejecución del proyecto
	/// </summary>
	public static string GetExecutionPath() => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? default!;

	/// <summary>
	///		Obtiene la lista de archivos del subdirectorio de Data especificado
	/// </summary>
	public static List<string> GetFiles(string path, string extension)
	{
		List<string> files = new();

			// Combina el subdirectorio
			if (!string.IsNullOrWhiteSpace(path))
				path = Path.Combine(GetDataPath(), path);
			else
				path = GetDataPath();
			// Busca los archivos del directorio
			foreach (string file in Directory.GetFiles(path))
				if (file.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
					files.Add(file);
			// Devuelve los archivos
			return files;
	}

	/// <summary>
	///		Obtiene el nombre del archivo JSON correspondiente a un archivo CSV
	/// </summary>
	public static string GetJsonFile(string file) => ChangeFileExtension(file, ".csv", ".json");

	/// <summary>
	///		Cambia la extensión de un archivo
	/// </summary>
	private static string ChangeFileExtension(string sourceFile, string extension, string targetExtension)
	{
		string file = Path.GetFileName(sourceFile);

			// Obtiene el nombre del archivo de respuesta
			if (file.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
			{
				// Cambia la extensión por la extensión final
				file = file.Replace(extension, targetExtension, StringComparison.CurrentCultureIgnoreCase);
				// Devuelve el nombre completo del archivo
				return Path.Combine(Path.GetDirectoryName(sourceFile) ?? string.Empty, file);
			}
			else
				return string.Empty;
	}

	/// <summary>
	///		Obtiene los datos de un esquema
	/// </summary>
	internal static SchemaModel? GetSchema(string file)
	{
		string schemaFile = file.Replace(".csv", ".schema", StringComparison.CurrentCultureIgnoreCase);

			// Carga el esquema
			if (File.Exists(schemaFile))
				return System.Text.Json.JsonSerializer.Deserialize<SchemaModel>(File.ReadAllText(schemaFile));
			else
				return null;
	}

	/// <summary>
	///		Obtiene un CSV reader de un archivo
	/// </summary>
	public static CsvReader GetCsvReader(string file)
	{
		SchemaModel? schema = GetSchema(file);

			if (schema is null)
				return new CsvReader(new FileModel(), null);
			else
				return new CsvReader(schema.Definition, GetCsvColumns(schema.Columns));
	}

	/// <summary>
	///		Obtiene las columnas para un archivo CSV
	/// </summary>
	private static List<ColumnModel> GetCsvColumns(List<SchemaColumnModel> columns)
	{
		List<ColumnModel> csvColumns = new();

			// Crea las columnas
			foreach (SchemaColumnModel column in columns)
				csvColumns.Add(new ColumnModel
										{
											Name = column.Name,
											Type = Convert(column.Type)
										}
							 );
			// Devuelve la lista de columnas del archivo
			return csvColumns;

			// Convierte el tipo de columna
			ColumnModel.ColumnType Convert(string type)
			{
				// Convierte el tipo
				if (!string.IsNullOrWhiteSpace(type))
				{
					if (type.Equals("int", StringComparison.CurrentCultureIgnoreCase))
						return ColumnModel.ColumnType.Integer;
					else if (type.Equals("decimal", StringComparison.CurrentCultureIgnoreCase))
						return ColumnModel.ColumnType.Decimal;
					else if (type.Equals("date", StringComparison.CurrentCultureIgnoreCase))
						return ColumnModel.ColumnType.DateTime;
					else if (type.Equals("string", StringComparison.CurrentCultureIgnoreCase))
						return ColumnModel.ColumnType.String;
					else if (type.Equals("bool", StringComparison.CurrentCultureIgnoreCase))
						return ColumnModel.ColumnType.Boolean;
				}
				// Si ha llegado hasta aquí es porque no ha encontrado el tipo
				return ColumnModel.ColumnType.Unknown;
			}
	}
}