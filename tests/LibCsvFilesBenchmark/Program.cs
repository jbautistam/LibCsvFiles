using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run(typeof(LibCsvFilesBenchmark.CsvFilesReaderBenchmark));

