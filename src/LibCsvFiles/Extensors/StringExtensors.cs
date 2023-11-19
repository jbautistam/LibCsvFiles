using System;

namespace Bau.Libraries.LibCsvFiles.Extensors
{
	/// <summary>
	///		Extensores para cadenas
	/// </summary>
	internal static class StringExtensors
	{
		/// <summary>
		///		Obtiene el valor de un enumerado
		/// </summary>
		internal static TypeEnum GetEnum<TypeEnum>(this string value, TypeEnum defaultValue) where TypeEnum : struct
		{
			if (Enum.TryParse(value, true, out TypeEnum result))
				return result;
			else
				return defaultValue;
		}

		/// <summary>
		///		Obtiene una fecha para una cadena
		/// </summary>
		public static DateTime? GetDateTime(this string value)
		{
			if (string.IsNullOrEmpty(value) || !DateTime.TryParse(value, out DateTime result))
				return null;
			else
				return result;
		}

		/// <summary>
		///		Obtiene una fecha para una cadena
		/// </summary>
		public static DateTime GetDateTime(this string value, DateTime defaultValue)
		{
			return GetDateTime(value) ?? defaultValue;
		}

		/// <summary>
		///		Convierte la fecha y hora de una cadena utilizando un formato estricto
		/// </summary>
		public static DateTime? GetDateTime(this string value, string format, System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.AssumeUniversal)
		{
			if (DateTime.TryParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, style, out DateTime result))
				return result;
			else
				return null;
		}

		/// <summary>
		///		Convierte la fecha y hora de una cadena con un formato estricto
		/// </summary>
		public static DateTime GetDateTime(this string value, string format, DateTime defaultValue, 
										   System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.AssumeUniversal)
		{
			return GetDateTime(value, format, style) ?? defaultValue;
		}

		/// <summary>
		///		Obtiene un valor decimal para una cadena
		/// </summary>
		public static double? GetDouble(this string value)
		{
			if (string.IsNullOrEmpty(value) || !double.TryParse(value.Replace(',', '.'), 
																System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, 
																System.Globalization.CultureInfo.InvariantCulture, out double result))
				return null;
			else
				return result;
		}

		/// <summary>
		///		Obtiene un valor decimal para una cadena
		/// </summary>
		public static double GetDouble(this string value, double defaultValue)
		{
			return GetDouble(value) ?? defaultValue;
		}

		/// <summary>
		///		Obtiene un valor entero para una cadena
		/// </summary>
		public static int? GetInt(this string value)
		{
			if (string.IsNullOrEmpty(value) || !int.TryParse(value, out int result))
				return null;
			else
				return result;
		}

		/// <summary>
		///		Obtiene un valor entero para una cadena
		/// </summary>
		public static int GetInt(this string value, int defaultValue)
		{
			return GetInt(value) ?? defaultValue;
		}
	}
}
