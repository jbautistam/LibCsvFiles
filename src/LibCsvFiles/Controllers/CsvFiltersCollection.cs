namespace Bau.Libraries.LibCsvFiles.Controllers;

/// <summary>
///		Colección de filtros
/// </summary>
public class CsvFiltersCollection
{
	/// <summary>
	///		Añade un filtro
	/// </summary>
	public void Add(string column, CsvFilter.ConditionType condition, object? value1, object? value2 = null)
	{
		Add(new CsvFilter(column, condition, value1, value2));
	}

	/// <summary>
	///		Añade un filtro
	/// </summary>
	public void Add(CsvFilter filter)
	{
		Filters.Add(filter.Column, filter);
	}

	/// <summary>
	///		Obtiene el filtro asociado a una columna
	/// </summary>
	internal CsvFilter? GetFilter(string column)
	{
		if (Filters.TryGetValue(column, out CsvFilter? filter) && filter.Condition != CsvFilter.ConditionType.NoCondition)
			return filter;
		else
			return null;
	}

	/// <summary>
	///		Evalúa los filtros sobre una columna
	/// </summary>
	internal bool Evaluate(string column, object? value)
	{
		CsvFilter? filter = GetFilter(column);

			if (filter is null)
				return true;
			else
				return filter.Condition switch
					{
						CsvFilter.ConditionType.Contains => EvaluateContains((value ?? string.Empty).ToString(), (filter.Value1 ?? string.Empty).ToString()),
						CsvFilter.ConditionType.Between => false,
						CsvFilter.ConditionType.In => false,
						_ => Evaluate(filter.Condition, value, filter.Value1)
					};
	}

	/// <summary>
	///		Evalúa la condición
	/// </summary>
	private bool Evaluate(CsvFilter.ConditionType conditionType, object? value, object? filterValue)
	{
		int comparison = EvaluateCompare(value, filterValue);

			return conditionType switch
					{
						CsvFilter.ConditionType.Equals => comparison == 0,
						CsvFilter.ConditionType.Less => comparison < 0,
						CsvFilter.ConditionType.LessOrEqual => comparison <= 0,
						CsvFilter.ConditionType.Greater => comparison > 0,
						CsvFilter.ConditionType.GreaterOrEqual => comparison >= 0,
						CsvFilter.ConditionType.Distinct => comparison != 0,
						_ => false
					};
	}

	/// <summary>
	///		Evalúa la condición de comparación
	///		* Menor que cero => value menor que filterValue
	///		* Cero => value igual a filterValue
	///		* Mayor que cero => value mayor que filterValue
	/// </summary>
	private int EvaluateCompare(object? value, object? filterValue)
	{
		if (value is null && filterValue is null)
			return 0;
		else if (value is null)
			return -1;
		else if (value is double || value is decimal || value is float)
			return ((value as double?) ?? 0).CompareTo((filterValue as double?) ?? 0);
		else if (value is int || value is byte || value is short || value is long)
			return ((value as long?) ?? 0).CompareTo((filterValue as long?) ?? 0);
		else if (value is DateTime)
			return ((value as DateTime?) ?? DateTime.UtcNow).CompareTo((filterValue as DateTime?) ?? DateTime.UtcNow);
		else 
			return (value?.ToString() ?? string.Empty).ToUpperInvariant().CompareTo((filterValue?.ToString() ?? string.Empty).ToUpperInvariant());
	}

	/// <summary>
	///		Evalúa la condición que indica si contiene el valor
	/// </summary>
	private bool EvaluateContains(string value, string filterValue) => value.IndexOf(filterValue, StringComparison.CurrentCultureIgnoreCase) >= 0;

	/// <summary>
	///		Número de filtros
	/// </summary>
	public int Count => Filters.Count;

	/// <summary>
	///		Filtros
	/// </summary>
	private Dictionary<string, CsvFilter> Filters { get; } = new(StringComparer.InvariantCultureIgnoreCase);
}
