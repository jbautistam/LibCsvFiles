# LibCsvFiles

Clases de ayuda para leer / escribir archivos CSV

[![Publish package](https://github.com/jbautistam/LibCsvFiles/actions/workflows/dotnet.yml/badge.svg)](https://github.com/jbautistam/LibCsvFiles/actions/workflows/dotnet.yml)

# Tiempo de lectura de archivos tras optimización

En las últimas versiones, se ha optimizado la lectura de archivos como se muestra en la siguiente tabla obtenida mediante BenchMarkDotNet:

| Method            | FileName     | Mean       | Error     | StdDev   | StdErr   | Min        | Max        | Median     | Ratio | Gen0        | Gen1      | Allocated   | Alloc Ratio |
|-------------------|------------- |-----------:|----------:|---------:|---------:|-----------:|-----------:|-----------:|------:|------------:|----------:|------------:|------------:|
| Csv optimized     | Sales.csv    |   419.4 ms |  10.46 ms |  2.72 ms |  1.21 ms |   416.4 ms |   422.4 ms |   418.6 ms |  0.23 |  42000.0000 |         - |   682.51 MB |        0.17 |
| Csv not optimized | Sales.csv    | 1,845.6 ms |  53.50 ms | 13.89 ms |  6.21 ms | 1,828.0 ms | 1,864.5 ms | 1,845.5 ms |  1.00 | 249000.0000 |         - |  3986.98 MB |        1.00 |
|                   |              |            |           |          |          |            |            |            |       |             |           |             |             |
| Csv optimized     | SalesBig.csv | 1,796.2 ms |  41.83 ms | 10.86 ms |  4.86 ms | 1,783.3 ms | 1,809.3 ms | 1,793.0 ms |  0.25 | 171000.0000 |         - |  2730.02 MB |        0.17 |
| Csv not optimized | SalesBig.csv | 7,282.0 ms | 346.95 ms | 90.10 ms | 40.30 ms | 7,172.9 ms | 7,397.0 ms | 7,275.4 ms |  1.00 | 999000.0000 | 3000.0000 | 15947.87 MB |        1.00 |

El método `Csv not optimized` muestra el resultado antes de optimizar mientras que el método `Csv optimized` muestra los datos tras las optimizaciones.

El archivo `Sales.csv` contiene 1.523.630 registros mientras que `SalesBig.csv` contiene 6.094.520 registros.

Puede ver las optimizaciones realizadas en mi [web](https://jbautistam.github.io/articles/development/optimising-csv-reader/optimising-csv-reader/).