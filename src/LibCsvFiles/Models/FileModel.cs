namespace Bau.Libraries.LibCsvFiles.Models;

/// <summary>
///		Parámetros de archivo CSV
/// </summary>
public class FileModel
{
	// Variables privadas
	private string _dateFormat = default!, _thousandsSeparator = default!;
	private string _trueValue = default!, _falseValue = default!;
	private char _separator = ',', _decimalSeparator = default!;

	public FileModel()
	{
		DateFormat = "yyyy-MM-dd";
		DecimalSeparator = '.';
		ThousandsSeparator = "";
		TrueValue = "1";
		FalseValue = "0";
		Separator = ',';
		WithHeader = true;
	}

	/// <summary>
	///		Obtiene un valor predeterminado cuando se lee una cadena vacía
	/// </summary>
	private string GetDefault(string value, string defaultValue)
	{
		if (string.IsNullOrEmpty(value))
			return defaultValue;
		else
			return value;
	}

	/// <summary>
	///		Formato de fecha
	/// </summary>
	public string DateFormat
	{
		get { return _dateFormat; }
		set { _dateFormat = GetDefault(value, "yyyy-MM-dd"); }
	}

	/// <summary>
	///		Separador de decimales
	/// </summary>
	public char DecimalSeparator
	{
		get { return _decimalSeparator; }
		set { _decimalSeparator = value; }
	}

	/// <summary>
	///		Separador de miles
	/// </summary>
	public string ThousandsSeparator
	{
		get { return _thousandsSeparator; }
		set { _thousandsSeparator = GetDefault(value, ","); }
	}

	/// <summary>
	///		Cadena para los valores verdaderos
	/// </summary>
	public string TrueValue
	{
		get { return _trueValue; }
		set { _trueValue = GetDefault(value, "1"); }
	}

	/// <summary>
	///		Cadena para los valores falsos
	/// </summary>
	public string FalseValue
	{
		get { return _falseValue; }
		set { _falseValue = GetDefault(value, "0"); }
	}

	/// <summary>
	///		Separador de campos
	/// </summary>
	public char Separator
	{
		get { return _separator; }
		set { _separator = value; }
	}

	/// <summary>
	///		Indica si la primera línea es la cabecera
	/// </summary>
	public bool WithHeader { get; set; }

	/// <summary>
	///		Indica si se debe leer o escribir una cabecera tipada
	/// </summary>
	public bool TypedHeader { get; set; }
}
