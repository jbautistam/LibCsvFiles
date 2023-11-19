namespace Bau.Libraries.LibCsvFiles.Controllers;

/// <summary>
///		Filtro para un registro CSV
/// </summary>
public class CsvFilter
{
	// Enumerados públicos
	/// <summary>
	///		Tipo de condición
	/// </summary>
	public enum ConditionType
	{
		/// <summary>Sin condición</summary>
		NoCondition,
		/// <summary>Igual a un valor</summary>
		Equals,
		/// <summary>Distinto de un valor</summary>
		Distinct,
		/// <summary>Mayor que un valor</summary>
		Greater,
		/// <summary>Mayor o igual que un valor</summary>
		GreaterOrEqual,
		/// <summary>Menor que un valor</summary>
		Less,
		/// <summary>Menor o igual que un valor</summary>
		LessOrEqual,
		/// <summary>En una lista de valores</summary>
		In,
		/// <summary>Entre dos valores</summary>
		Between,
		/// <summary>Contiene un valor (cadenas)</summary>
		Contains
	}

	public CsvFilter(string column, ConditionType condition, object? value1, object? value2 = null)
	{
		Column = column;
		Condition = condition;
		Value1 = value1;
		Value2 = value2;
	}

	/// <summary>
	///		Columna a la que se aplica el filtro
	/// </summary>
	public string Column { get; }

	/// <summary>
	///		Condición
	/// </summary>
	public ConditionType Condition { get; }

	/// <summary>
	///		Valor
	/// </summary>
	public object? Value1 { get; }
	
	/// <summary>
	///		Segundo valor
	/// </summary>
	public object? Value2 { get; }
}