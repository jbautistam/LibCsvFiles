namespace LibCsvFilesTest.Models;

/// <summary>
///		Clase con los datos de una columna del esquema
/// </summary>
public class SchemaColumnModel
{
	/// <summary>
	///		Nombre de la columna
	/// </summary>
	public string Name { get; set; } = default!;

	/// <summary>
	///		Tipo de la columna
	/// </summary>
	public string Type { get; set; } = default!;
}
