using System;
using System.Collections.Generic;
using System.Data;

using Bau.Libraries.LibHelper.Extensors;
using Bau.Libraries.LibCsvFiles.Models;

namespace Bau.Libraries.LibCsvFiles
{
	/// <summary>
	///		Implementación de <see cref="IDataReader"/> para archivos CSV
	/// </summary>
	public class CsvReader : IDataReader
	{
		// Eventos públicos
		public event EventHandler<EventArguments.AffectedEvntArgs> ReadBlock;
		// Variables privadas
		private List<object> _recordsValues;
		private long _row;

		public CsvReader(string fileName, FileModel definition, List<ColumnModel> columns, int notifyAfter = 10_000)
		{
			FileName = fileName;
			FileDefinition = definition ?? new FileModel();
			Columns = columns ?? new List<ColumnModel>();
			NotifyAfter = notifyAfter;
		}

		public CsvReader(System.IO.StreamReader fileReader, FileModel definition, List<ColumnModel> columns, int notifyAfter = 10_000)
		{
			FileReader = fileReader;
			FileDefinition = definition ?? new FileModel();
			Columns = columns ?? new List<ColumnModel>();
			NotifyAfter = notifyAfter;
		}

		/// <summary>
		///		Abre el archivo
		/// </summary>
		private void Open()
		{
			// Abre el stream
			if (!string.IsNullOrWhiteSpace(FileName) && FileReader == null)
			{
				// Abre el archivo
				FileReader = new System.IO.StreamReader(FileName, true);
				// Lee las cabeceras
				ReadHeader();
			}
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
			if (!FileReader.EndOfStream && FileDefinition.WithHeader)
			{
				string line = FileReader.ReadLine();

					// Quita los espacios
					if (!string.IsNullOrWhiteSpace(line))
						line = line.Trim();
					// Interpreta las columnas (si no se han definido)
					if (Columns.Count == 0)
						foreach (string field in ParseLine(line))
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

				// Abre el archivo si estaba cerrado
				if (FileReader == null)
					Open();
				// Lee la siguiente línea no vacía y se salta las líneas de cabecera
				while (!FileReader.EndOfStream && (string.IsNullOrWhiteSpace(line) || mustReadNextLine))
				{
					// Resetea el valor que indica que la siguiente vez se debe leer la siguiente línea
					mustReadNextLine = false;
					// Lee la línea
					line += FileReader.ReadLine();
					// Se debe leer la siguiente línea si el número de caracteres de comillas no es par, eso quiere decir que ha habido un salto
					// de línea en un campo
					if (CountQuotes(line) % 2 != 0)
					{
						// Añade a la línea el salto de línea que ha borrado FileReader.ReadLine()
						line += Environment.NewLine;
						// Indica que se debe leer una línea más
						mustReadNextLine = true;
					}
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
		private int CountQuotes(string line)
		{
			int number = 0;

				// Cuenta las comillas de la cadena
				foreach (char chr in line)
					if (chr == '"')
						number++;
				// Devuelve el número contado
				return number;
		}

		///// <summary>
		/////		Comprueba si se debe interpretar la primera línea
		///// </summary>
		//private bool MustParseFirstLine()
		//{
		//	return Columns.Count == 0 || FileDefinition.TypedHeader || FileDefinition.WithHeader;
		//}

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
			bool isInQuotes = false;

				// Interpreta las partes de la línea
				foreach (char actual in line)
				{
					// Trata el carácter
					if (isInQuotes && actual == '"')
						isInQuotes = false;
					else if (!isInQuotes)
					{
						if (actual == '"')
							isInQuotes = true;
						else if (actual == FileDefinition.Separator[0])
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
		private List<object> ConvertFields(List<string> fields)
		{
			List<object> values = new List<object>();
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
		private object ConvertField(ColumnModel column, string field)
		{
			object value = null;

				// Convierte una cadena en un objeto
				if (!string.IsNullOrEmpty(field))
					switch (column.Type)
					{
						case ColumnModel.ColumnType.Unknown:
								value = null;
							break;
						case ColumnModel.ColumnType.DateTime:
								value = field.GetDateTime(FileDefinition.DateFormat);
							break;
						case ColumnModel.ColumnType.Boolean:
								value = field.EqualsIgnoreCase(FileDefinition.TrueValue);
							break;
						case ColumnModel.ColumnType.Numeric:
								value = field.Replace(FileDefinition.DecimalSeparator, ".").GetDouble(0);
							break;
						default:
								value = field;
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
			if (FileReader != null)
			{
				// Cierra el archivo
				FileReader.Close();
				// y libera los datos
				FileReader = null;
			}
		}

		/// <summary>
		///		Obtiene el nombre del campo
		/// </summary>
		public string GetName(int i)
		{
			return Columns[i].Name;
		}

		/// <summary>
		///		Obtiene el nombre del tipo de datos
		/// </summary>
		public string GetDataTypeName(int i)
		{
			return _recordsValues[i].GetType().Name;
		}

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
				case ColumnModel.ColumnType.Numeric:
					return typeof(double);
				default:
					return typeof(string);
			}
		}

		/// <summary>
		///		Obtiene el valor de un campo
		/// </summary>
		public object GetValue(int i)
		{
			return _recordsValues[i];
		}

		public DataTable GetSchemaTable()
		{
			throw new NotImplementedException();
		}

		public int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public bool GetBoolean(int i)
		{
			throw new NotImplementedException();
		}

		public byte GetByte(int i)
		{
			throw new NotImplementedException();
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public char GetChar(int i)
		{
			throw new NotImplementedException();
		}

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid(int i)
		{
			throw new NotImplementedException();
		}

		public short GetInt16(int i)
		{
			throw new NotImplementedException();
		}

		public int GetInt32(int i)
		{
			throw new NotImplementedException();
		}

		public long GetInt64(int i)
		{
			throw new NotImplementedException();
		}

		public float GetFloat(int i)
		{
			throw new NotImplementedException();
		}

		public double GetDouble(int i)
		{
			throw new NotImplementedException();
		}

		public string GetString(int i)
		{
			throw new NotImplementedException();
		}

		public decimal GetDecimal(int i)
		{
			throw new NotImplementedException();
		}

		public DateTime GetDateTime(int i)
		{
			throw new NotImplementedException();
		}

		public IDataReader GetData(int i)
		{
			throw new NotImplementedException();
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
		public bool IsDBNull(int i)
		{
			return _recordsValues[i] == null || _recordsValues[i] is DBNull;
		}

		/// <summary>
		///		Los CSV sólo devuelven un Resultset, de todas formas, DbDataAdapter espera este valor
		/// </summary>
		public bool NextResult()
		{
			return false;
		}

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
		public int Depth 
		{ 
			get { return 0; }
		}

		/// <summary>
		///		Indica si está cerrado
		/// </summary>
		public bool IsClosed 
		{ 
			get { return FileReader == null; }
		}

		/// <summary>
		///		Registros afectados
		/// </summary>
		public int RecordsAffected 
		{ 
			get { return -1; }
		}

		/// <summary>
		///		Nombre del archivo
		/// </summary>
		public string FileName { get; }

		/// <summary>
		///		Lector del archivo
		/// </summary>
		private System.IO.StreamReader FileReader { get; set; }

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
				// Lee la cabecera para cargar las columnas si es necesario
				if (Columns.Count == 0 && (FileDefinition.WithHeader || FileDefinition.TypedHeader))
					Open();
				// Devuelve el número de columnas
				return Columns.Count; 
			}
		}

		/// <summary>
		///		Indizador por número de campo
		/// </summary>
		public object this[int i] 
		{ 
			get { return _recordsValues[i]; }
		}

		/// <summary>
		///		Indizador por nombre de campo
		/// </summary>
		public object this[string name]
		{ 
			get { return _recordsValues[GetOrdinal(name)]; }
		}
	}
}
