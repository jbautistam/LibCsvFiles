namespace Bau.Libraries.LibCsvFiles.Models;

/// <summary>
///		Clase con los datos de una columna de un archivo CSV
/// </summary>
public class ColumnModel
{
	/// <summary>
	///		Tipo de columna
	/// </summary>
	public enum ColumnType
	{
		/// <summary>Desconocido. No se debería utilizar</summary>
		Unknown,
		/// <summary>Valor entero</summary>
		Integer,
		/// <summary>Valor decimal</summary>
		Decimal,
		/// <summary>Fecha / hora</summary>
		DateTime,
		/// <summary>Cadena</summary>
		String,
		/// <summary>Valor lógico</summary>
		Boolean
	}

	/// <summary>
	///		Obtiene el tipo de columna
	/// </summary>
	internal static ColumnType GetColumnType(Type dataType)
	{
		string type = dataType.ToString();

			if (Contains(type, ".int") || Contains(type, ".byte") || Contains(type, ".short"))
				return ColumnType.Integer;
			else if (Contains(type, ".double") || Contains(type, ".float") || Contains(type, ".decimal"))
				return ColumnType.Decimal;
			else if (Contains(type, ".datetime"))
				return ColumnType.DateTime;
			else if (Contains(type, ".bool"))
				return ColumnType.Boolean;
			else if (Contains(type, ".string") || Contains(type, ".char"))
				return ColumnType.String;
			else
				return ColumnType.Unknown;

		// Comprueba si una cadena contiene un valor
		bool Contains(string value, string search)
		{
			return value.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
		}
	}

	/// <summary>
	///		Nombre de la columna
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///		Tipo de la columna
	/// </summary>
	public ColumnType Type { get; set; }
}
