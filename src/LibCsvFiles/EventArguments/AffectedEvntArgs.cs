using System;

namespace Bau.Libraries.LibCsvFiles.EventArguments
{
	/// <summary>
	///		Argumento del evento de lectura / escritura sobre un archivo CSV
	/// </summary>
	public class AffectedEvntArgs : EventArgs
	{
		public AffectedEvntArgs(long records)
		{
			Records = records;
		}

		/// <summary>
		///		Número de registros
		/// </summary>
		public long Records { get; }
	}
}
