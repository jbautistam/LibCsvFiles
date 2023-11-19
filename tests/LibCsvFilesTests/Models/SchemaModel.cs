using Bau.Libraries.LibCsvFiles.Models;

namespace LibCsvFilesTest.Models;

/// <summary>
///		Clase con los datos de un esquema
/// </summary>
public class SchemaModel
{
	/// <summary>
	///		Columnas
	/// </summary>
	public List<SchemaColumnModel> Columns { get; set; } = default!;

	/// <summary>
	///		Definición del archivo
	/// </summary>
	public FileModel Definition { get; set; } = default!;
}
