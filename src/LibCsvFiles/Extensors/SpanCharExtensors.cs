namespace Bau.Libraries.LibCsvFiles.Extensors;

/// <summary>
///		Extensores para <see cref="ReadOnlySpan{Char}"/>
/// </summary>
internal static class SpanCharExtensors
{
	/// <summary>
	///		Obtiene el valor de un enumerado
	/// </summary>
	internal static TypeEnum GetEnum<TypeEnum>(this ReadOnlySpan<char> value, TypeEnum defaultValue) where TypeEnum : struct
	{
		if (Enum.TryParse(value, true, out TypeEnum result))
			return result;
		else
			return defaultValue;
	}

	/// <summary>
	///		Obtiene una fecha para una cadena
	/// </summary>
	public static DateTime? GetDateTime(this ReadOnlySpan<char> value)
	{
		if (DateTime.TryParse(value, out DateTime result))
			return result;
		else
			return null;
	}

	/// <summary>
	///		Obtiene una fecha para una cadena
	/// </summary>
	public static DateTime GetDateTime(this ReadOnlySpan<char> value, DateTime defaultValue) => GetDateTime(value) ?? defaultValue;

	/// <summary>
	///		Convierte la fecha y hora de una cadena utilizando un formato estricto
	/// </summary>
	public static DateTime? GetDateTime(this ReadOnlySpan<char> value, string format, System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.AssumeUniversal)
	{
		if (DateTime.TryParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, style, out DateTime result))
			return result;
		else
			return null;
	}

	/// <summary>
	///		Convierte la fecha y hora de una cadena con un formato estricto
	/// </summary>
	public static DateTime GetDateTime(this ReadOnlySpan<char> value, string format, DateTime defaultValue, 
									   System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.AssumeUniversal)
	{
		return GetDateTime(value, format, style) ?? defaultValue;
	}

	/// <summary>
	///		Obtiene un valor decimal para una cadena
	/// </summary>
	public static double? GetDouble(this ReadOnlySpan<char> value)
	{
		if (double.TryParse(value, 
							System.Globalization.NumberStyles.AllowDecimalPoint | 
								System.Globalization.NumberStyles.AllowLeadingSign, 
							System.Globalization.CultureInfo.InvariantCulture, out double result))
			return result;
		else
			return null;
	}

	/// <summary>
	///		Obtiene un valor decimal para una cadena
	/// </summary>
	public static double GetDouble(this ReadOnlySpan<char> value, double defaultValue) => GetDouble(value) ?? defaultValue;

	/// <summary>
	///		Obtiene un valor entero para una cadena
	/// </summary>
	public static int? GetInt(this ReadOnlySpan<char> value)
	{
		if (int.TryParse(value, out int result))
			return result;
		else
			return null;
	}

	/// <summary>
	///		Obtiene un valor entero para una cadena
	/// </summary>
	public static int GetInt(this ReadOnlySpan<char> value, int defaultValue) => GetInt(value) ?? defaultValue;
}
