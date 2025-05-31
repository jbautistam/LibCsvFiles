namespace LibCsvFilesBenchmark.Tools;

/// <summary>
///		Clase de ayuda para tratamiento de archivos
/// </summary>
internal static class FileHelper
{
	/// <summary>
	///		Obtiene el nombre completo de un archivo
	/// </summary>
	internal static string GetFullFileName(string fileName) => Path.Combine(GetDataPath(), fileName);

	/// <summary>
	///		Obtiene el directorio de archivos de datos del proyecto
	/// </summary>
	internal static string GetDataPath() => Path.Combine(GetExecutionPath(), "Data");

	/// <summary>
	///		Obtiene el directorio de ejecución del proyecto
	/// </summary>
	private static string GetExecutionPath() => Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? default!;

	/// <summary>
	///		Obtiene la lista de archivos del subdirectorio de Data especificado
	/// </summary>
	internal static List<string> GetFiles(string path, string extension)
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
}
