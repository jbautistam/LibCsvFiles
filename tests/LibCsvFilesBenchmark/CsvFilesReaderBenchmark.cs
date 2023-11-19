using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

using Bau.Libraries.LibCsvFiles;
using LibCsvTestsHelper.Tools;

namespace LibCsvFilesBenchmark;

/// <summary>
///		Benchmark de lectura de archivos CSV
/// </summary>
[SimpleJob(RuntimeMoniker.Net70, baseline: true, iterationCount: 5)]
//[SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn, StdDevColumn, StdErrorColumn]
public class CsvFilesReaderBenchmark
{
	[Params("Sales.csv", "SalesBig.csv")]
	public string FileName = default!;

	/// <summary>
	///		Configuración
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
	}

	/// <summary>
	///		Lectura de archivos CSV no optimizado
	/// </summary>
	[Benchmark(Baseline = true, Description = "Csv not optimized")]
	public void CsvReadNotOptimized()
	{
		using (CsvReaderNotOptimized reader = FileHelper.GetCsvReaderNotOptimized(FileHelper.GetFullFileName(FileName)))
		{
			object? value;

				// Abre el archivo
				reader.Open(FileHelper.GetFullFileName(FileName));
				// Recorre todos los registros
				while (reader.Read())
					for (int index = 0; index < reader.FieldCount; index++)
						value = reader.GetValue(index);
		}
	}

	/// <summary>
	///		Lectura de archivos CSV optimizada
	/// </summary>
	[Benchmark(Description = "Csv optimized")]
	public void CsvReadOptimized()
	{
		using (CsvReader reader = new(null, null))
		{
			object? value;

				// Abre el archivo
				reader.Open(FileHelper.GetFullFileName(FileName));
				// Recorre todos los registros
				while (reader.Read())
					for (int index = 0; index < reader.FieldCount; index++)
						value = reader.GetValue(index);
		}
	}
}