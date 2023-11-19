using System;
using System.Collections.Generic;
using System.Data;

using LibCsvFilesBenchmark.Extensors;
using Bau.Libraries.LibCsvFiles.Models;

namespace Bau.Libraries.LibCsvFiles
{
	/// <summary>
	///		Implementación de <see cref="IDataReader"/> para archivos CSV
	/// </summary>
	public class CsvReaderNotOptimized : IDataReader
	{
		// Eventos públicos
		public event EventHandler<EventArguments.AffectedEvntArgs>? ReadBlock;
		// Variables privadas
		private bool _streamOpenedFromReader;
		private System.IO.StreamReader? _fileReader;
		private List<object?>? _recordsValues;
		private long _row;

		public CsvReaderNotOptimized(FileModel? definition, List<ColumnModel>? columns, int notifyAfter = 10_000)
		{
			FileDefinition = definition ?? new FileModel();
			Columns = columns ?? new List<ColumnModel>();
			NotifyAfter = notifyAfter;
		}

		/// <summary>
		///		Abre el archivo
		/// </summary>
		public void Open(string fileName)
		{
			// Indica que el stream se ha abierto en la librería
			_streamOpenedFromReader = true;
			// Abre el archivo sobre el stream
			Open(new System.IO.StreamReader(fileName, true));
		}

		/// <summary>
		///		Abre el datareader sobre el stream
		/// </summary>
		public void Open(System.IO.StreamReader stream)
		{
			// Guarda el stream
			_fileReader = stream;
			// Lee las cabeceras
			ReadHeader();
			// e indica que aún no se ha leido ninguna línea
			_row = 0;
		}

		/// <summary>
		///		Interpreta la cabecera del archivo utilizando cabeceras de columnas tipadas. 
		///		Las cabeceras de columnas con el tipo, tienen la estructura Nombre|Tipo
		///		Sólo se interpreta la cabecera cuando no hay columnas y el archivo está marcado indicando que contiene columnas tipadas en la cabecera
		/// </summary>
		private void ReadHeader()
		{
			if (_fileReader is not null && !_fileReader.EndOfStream && FileDefinition.WithHeader)
			{
				string? line = _fileReader.ReadLine();

					// Interpreta las columnas (si no se han definido)
					if (Columns.Count == 0 && !string.IsNullOrEmpty(line))
						foreach (string field in ParseLine(line.Trim()))
							if (FileDefinition.TypedHeader)
							{
								string[] parts = field.Split('|');

									if (parts.Length == 2)
										Columns.Add(new ColumnModel
															{
																Name = parts[0],
																Type = parts[1].GetEnum(ColumnModel.ColumnType.String)
															}
													);
									else
										throw new NotImplementedException($"Can't extract the column type from header ({field})");
							}
							else
								Columns.Add(new ColumnModel
													{
														Name = field,
														Type = ColumnModel.ColumnType.String
													}
											);
			}
		}

		/// <summary>
		///		Lee un registro
		/// </summary>
		public bool Read()
		{
			bool readed = false;
			string line = ReadLine();

				// Interpreta los datos
				if (!string.IsNullOrWhiteSpace(line))
				{
					// Interpreta la línea
					_recordsValues = ConvertFields(ParseLine(line));
					// Incrementa el número de línea y lanza el evento
					_row++;
					RaiseEventReadBlock(_row);
					// Indica que se han leido datos
					readed = true;
				}
				else
					_recordsValues = null;
				// Devuelve el valor que indica si se han leído datos
				return readed;
		}

		/// <summary>
		///		Lee la siguiente línea no vacía del archivo
		/// </summary>
		private string ReadLine()
		{
			string line = string.Empty;
			bool mustReadNextLine = false;
			int quotes = 0;

				// Lee la siguiente línea no vacía y se salta las líneas de cabecera
				while (_fileReader != null && !_fileReader.EndOfStream && (string.IsNullOrWhiteSpace(line) || mustReadNextLine))
				{
					string? part = _fileReader.ReadLine();

						// Resetea el valor que indica que la siguiente vez se debe leer la siguiente línea
						mustReadNextLine = false;
						// Lee la línea
						// Cuenta el número de comillas (los acumula porque puede que haya saltos de línea intermedios que tengan también comillas)
						quotes += CountQuotes(part);
						// Se debe leer la siguiente línea si el número de caracteres de comillas no es par, eso quiere decir que ha habido un salto
						// de línea en un campo
						if (quotes % 2 != 0)
						{
							// Añade a la línea el salto de línea que ha borrado FileReader.ReadLine()
							part += Environment.NewLine;
							// Indica que se debe leer una línea más
							mustReadNextLine = true;
						}
						// Añade la sección a la línea
						line += part;
				}
				// Quita los espacios de la línea
				if (!string.IsNullOrWhiteSpace(line))
					line = line.Trim();
				// Devuelve la línea leida
				return line;
		}

		/// <summary>
		///		Cuenta el número de comillas que hay en una línea
		/// </summary>
		private int CountQuotes(string? line)
		{
			int number = 0;

				// Si hay algo que contar
				if (!string.IsNullOrWhiteSpace(line))
				{
					// Quita los caracteres de escape (\")
					line = line.Replace("\\\"", "");
					// Cuenta las comillas de la cadena
					foreach (char chr in line)
						if (chr == '"')
							number++;
				}
				// Devuelve el número contado
				return number;
		}

		/// <summary>
		///		Lanza el evento de lectura de un bloque
		/// </summary>
		private void RaiseEventReadBlock(long row)
		{
			if (NotifyAfter > 0 && row % NotifyAfter == 0)
				ReadBlock?.Invoke(this, new EventArguments.AffectedEvntArgs(row));
		}

		/// <summary>
		///		Interpreta la línea separando los campos teniendo en cuenta las comillas
		/// </summary>
		private List<string> ParseLine(string line)
		{
			List<string> fields = new List<string>();
			string field = string.Empty;
			bool isInQuotes = false, scapeChar = false;

				// Interpreta las partes de la línea
				foreach (char actual in line)
				{
					// Trata el carácter
					if (isInQuotes)
					{
						if (!scapeChar && (actual == '\\' || actual == '"'))
							scapeChar = true;
						else if (actual == '"')
						{
							if (scapeChar)
							{
								field += actual;
								scapeChar = false;
							}
							else
							{
								isInQuotes = false;
								scapeChar = false;
							}
						}
						else if (actual == FileDefinition.Separator && scapeChar)
						{
							scapeChar = false;
							isInQuotes = false;
							fields.Add(field);
							field = string.Empty;
						}
						else
							field += actual;
					}
					else if (!isInQuotes)
					{
						if (actual == '"')
							isInQuotes = true;
						else if (actual == FileDefinition.Separator)
						{
							// Convierte la cadena
							fields.Add(field);
							// Vacía la cadena intermedia e incrementa el índice del campo
							field = string.Empty;
						}
						else
							field += actual;
					}
					else
						field += actual;
				}
				// Añade el último campo
				fields.Add(field);
				// Devuelve la lista de campos
				return fields;
		}

		/// <summary>
		///		Convierte las cadenas leidas
		/// </summary>
		private List<object?> ConvertFields(List<string> fields)
		{
			List<object?> values = new List<object?>();
			int index = 0;

				// Convierte cada uno de los valores
				foreach (string field in fields)
					if (Columns.Count > index)
						values.Add(ConvertField(Columns[index++], field));
					else
						values.Add(null);
				// Devuelve la lista de valores
				return values;
		}

		/// <summary>
		///		Convierte una cadena en el contenido de una columna
		/// </summary>
		private object? ConvertField(ColumnModel column, string field)
		{
			object? value = null;

				// Convierte una cadena en un objeto
				if (field.Length > 0)
					switch (column.Type)
					{
						case ColumnModel.ColumnType.Unknown:
								value = null;
							break;
						case ColumnModel.ColumnType.DateTime:
								value = field.GetDateTime(FileDefinition.DateFormat);
							break;
						case ColumnModel.ColumnType.Boolean:
								value = field.Equals(FileDefinition.TrueValue, StringComparison.CurrentCultureIgnoreCase);
							break;
						case ColumnModel.ColumnType.Integer:
								value = field.GetInt(0);
							break;
						case ColumnModel.ColumnType.Decimal:
								value = field.ToString().Replace(FileDefinition.DecimalSeparator, '.').GetDouble(0);
							break;
						default:
								value = field.ToString();
							break;
					}
				// Devuelve el valor convertido
				return value;
		}

		/// <summary>
		///		Cierra el archivo
		/// </summary>
		public void Close()
		{
			if (_streamOpenedFromReader && _fileReader != null)
			{
				// Cierra el archivo
				_fileReader.Close();
				// y libera los datos
				_fileReader = null;
			}
		}

		/// <summary>
		///		Obtiene el nombre del campo
		/// </summary>
		public string GetName(int i) => Columns[i].Name;

		/// <summary>
		///		Obtiene el nombre del tipo de datos
		/// </summary>
		public string GetDataTypeName(int i) => GetFieldType(i).Name;

		/// <summary>
		///		Obtiene el tipo de un campo
		/// </summary>
		public Type GetFieldType(int i)
		{
			if (_recordsValues == null)
				return GetColumnType(Columns[i].Type);
			else
				return _recordsValues[i].GetType();
		}

		/// <summary>
		///		Obtiene el tipo de una columna
		/// </summary>
		private Type GetColumnType(ColumnModel.ColumnType type)
		{
			switch (type)
			{
				case ColumnModel.ColumnType.Boolean:
					return typeof(bool);
				case ColumnModel.ColumnType.DateTime:
					return typeof(DateTime);
				case ColumnModel.ColumnType.Integer:
					return typeof(int);
				case ColumnModel.ColumnType.Decimal:
					return typeof(double);
				default:
					return typeof(string);
			}
		}

		/// <summary>
		///		Obtiene el valor de un campo
		/// </summary>
		public object? GetValue(int i) 
		{
			if (i > _recordsValues.Count - 1)
				return null;
			else
				return _recordsValues?[i];
		}

		/// <summary>
		///		Obtiene el esquema
		/// </summary>
		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///		Obtiene una serie de valores
		/// </summary>
		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///		Obtiene un valor bool de un campo
		/// </summary>
		public bool GetBoolean(int i) => GetDataValue<bool>(i);

		/// <summary>
		///		Obtiene un valor byte de un campo
		/// </summary>
		public byte GetByte(int i) => GetDataValue<byte>(i);

		/// <summary>
		///		Obtiene una serie de bytes
		/// </summary>
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///		Obtiene un valor char de un campo
		/// </summary>
		public char GetChar(int i) => GetDataValue<char>(i);

		/// <summary>
		///		Obtiene una serie de caracteres
		/// </summary>
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///		Obtiene un Guid
		/// </summary>
		public Guid GetGuid(int i) => GetDataValue<Guid>(i);

		/// <summary>
		///		Obtiene un entero de 16
		/// </summary>
		public short GetInt16(int i) => GetDataValue<short>(i);

		/// <summary>
		///		Obtiene un entero de 32
		/// </summary>
		public int GetInt32(int i) => GetDataValue<int>(i);

		/// <summary>
		///		Obtiene un entero largo
		/// </summary>
		public long GetInt64(int i) => GetDataValue<long>(i);

		/// <summary>
		///		Obtiene un valor flotante
		/// </summary>
		public float GetFloat(int i) => GetDataValue<float>(i);

		/// <summary>
		///		Obtiene un valor doble
		/// </summary>
		public double GetDouble(int i) => GetDataValue<double>(i);

		/// <summary>
		///		Obtiene una cadena
		/// </summary>
		public string? GetString(int i)
		{
			object value = GetValue(i);

				if (value is string resultValue)
					return resultValue;
				else
					return value?.ToString();
		}

		/// <summary>
		///		Obtiene un valor decimal
		/// </summary>
		public decimal GetDecimal(int i) => GetDataValue<decimal>(i);

		/// <summary>
		///		Obtiene una fecha
		/// </summary>
		public DateTime GetDateTime(int i) => GetDataValue<DateTime>(i);

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///		Obtiene un campo de un tipo determinado
		/// </summary>
		private TypeData GetDataValue<TypeData>(int i)
		{
			object? value = GetValue(i);

				if (value is TypeData resultValue)
					return resultValue;
				else
					return default;
		}

		/// <summary>
		///		Obtiene el índice de un campo a partir de su nombre
		/// </summary>
		public int GetOrdinal(string name)
		{
			// Obtiene el índice del registro
			if (!string.IsNullOrWhiteSpace(name))
				foreach (ColumnModel column in Columns)
					if (column.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
						return Columns.IndexOf(column);
			// Si ha llegado hasta aquí es porque no ha encontrado el campo
			return -1;
		}

		/// <summary>
		///		Indica si el campo es un DbNull
		/// </summary>
		public bool IsDBNull(int index) => index >= _recordsValues?.Count || _recordsValues?[index] is null || _recordsValues[index] is DBNull;

		/// <summary>
		///		Los CSV sólo devuelven un Resultset, de todas formas, DbDataAdapter espera este valor
		/// </summary>
		public bool NextResult() => false;

		/// <summary>
		///		Libera la memoria
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				// Libera los datos
				if (disposing)
					Close();
				// Indica que se ha liberado
				IsDisposed = true;
			}
		}

		/// <summary>
		///		Libera la memoria
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		///		Profundidad del recordset
		/// </summary>
		public int Depth => 0; 

		/// <summary>
		///		Indica si está cerrado
		/// </summary>
		public bool IsClosed => _fileReader == null;

		/// <summary>
		///		Registros afectados
		/// </summary>
		public int RecordsAffected => -1;

		/// <summary>
		///		Parámetros de definición del archivo
		/// </summary>
		public FileModel FileDefinition { get; }

		/// <summary>
		///		Columnas
		/// </summary>
		public List<ColumnModel> Columns { get; }

		/// <summary>
		///		Bloque de filas para las que se lanza el evento de grabación
		/// </summary>
		public int NotifyAfter { get; }

		/// <summary>
		///		Indica si se ha liberado el recurso
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		///		Número de campos a partir de las columnas
		/// </summary>
		/// <remarks>
		///		Lo primero que hace un BulkCopy es ver el número de campos que tiene, si no se ha leido la cabecera puede
		///	que aún no tengamos ningún número de columnas, por eso se lee por primera vez
		/// </remarks>
		public int FieldCount 
		{ 
			get 
			{ 
				return Columns.Count; 
			}
		}

		/// <summary>
		///		Indizador por número de campo
		/// </summary>
		public object? this[int i] 
		{ 
			get 
			{ 
				if (_recordsValues is null)
					return null;
				else 
					return _recordsValues[i]; 
			}
		}

		/// <summary>
		///		Indizador por nombre de campo
		/// </summary>
		public object? this[string name]
		{ 
			get 
			{
				if (_recordsValues is null)
					return null;
				else
				{
					int index = GetOrdinal(name);

						if (index >= _recordsValues.Count)
							return null;
						else
							return _recordsValues[GetOrdinal(name)]; 
				}
			}
		}
	}
}
