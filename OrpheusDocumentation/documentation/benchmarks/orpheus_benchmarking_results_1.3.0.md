# Benchmark results
 
## Machine details 
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Operating System          |Windows 11  |
 |CPU                       |AMD Ryzen 5 5500U|
 |Database engine           |SQL Server  |
 |Database engine location  |Local       |
 |HDD type                  |SSD         |
 |RAM                       |16GB         |

## Benchmark type
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Serialization type        |POCO        |
 |Benchmark framework       |[BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)|
 |Performed on              |September 10th 2023|
 |Orpheus version           |1.3.0|
 |Runtime                   |BenchmarkDotNet v0.13.8, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2)|
 |                          |.NET SDK 7.0.400|
 |                          | Job-DJWJHC : .NET 7.0.10 (7.0.1023.36312), X64 RyuJIT AVX2|

## Model used
```csharp
    public enum TestModelTransactorType
    {
        ttCustomer,
        ttSupplier
    }
    public class TestModelTransactor
    {
        [PrimaryKey]
        public Guid TransactorId { get; set; }

        [Length(30)]
        public string Code { get; set; }

        [Length(120)]
        public string Description { get; set; }

        [Length(120)]
        public string Address { get; set; }

        [Length(250)]
        public string Email { get; set; }

        public TestModelTransactorType Type { get; set; }
    }
```

## Insert results
Inserting rows in a batch. One transaction per test.

| Method         | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Allocated  | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|--------:|---------:|---------:|-----------:|------------:|
| Insert10Rows   |  1.081 ms | 0.0391 ms | 0.0366 ms |  1.00 |    0.00 |  25.3906 |        - |   54.65 KB |        1.00 |
| Insert100Rows  |  9.139 ms | 0.5144 ms | 0.4812 ms |  8.45 |    0.32 | 187.5000 |        - |  399.32 KB |        7.31 |
| Insert1000Rows | 87.707 ms | 2.0864 ms | 1.9516 ms | 81.21 |    2.53 | 666.6667 | 500.0000 | 3844.32 KB |       70.35 |

## Load results
Loading and serializing one model/record per iteration. 

| Method                 | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0       | Allocated   | Alloc Ratio |
|----------------------- |-----------:|----------:|----------:|------:|--------:|-----------:|------------:|------------:|
| Load50RowsOneAtATime   |   4.055 ms | 0.1247 ms | 0.1166 ms |  1.00 |    0.00 |   125.0000 |   256.39 KB |        1.00 |
| Load500RowsOneAtATime  |  41.018 ms | 1.6845 ms | 1.5756 ms | 10.12 |    0.46 |  1230.7692 |  2563.92 KB |       10.00 |
| Load5000RowsOneAtATime | 390.491 ms | 8.6642 ms | 7.6806 ms | 96.05 |    3.17 | 12500.0000 | 25639.05 KB |      100.00 |

## Update results
Updating rows in a batch. One transaction per test. 4 model fields have been updated.

| Method         | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1     | Allocated  | Alloc Ratio |
|--------------- |----------:|---------:|---------:|------:|--------:|---------:|---------:|-----------:|------------:|
| Update10Rows   |  10.98 ms | 0.402 ms | 0.357 ms |  1.00 |    0.00 | 171.8750 |        - |  377.11 KB |        1.00 |
| Update100Rows  |  11.19 ms | 0.647 ms | 0.605 ms |  1.02 |    0.06 | 171.8750 |        - |  377.11 KB |        1.00 |
| Update1000Rows | 110.83 ms | 3.419 ms | 2.855 ms | 10.09 |    0.46 | 600.0000 | 400.0000 | 3766.46 KB |        9.99 |

## Delete results
Deleting rows in a batch. One transaction per test.

| Method         | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0     | Allocated | Alloc Ratio |
|--------------- |------------:|----------:|----------:|------:|--------:|---------:|----------:|------------:|
| Delete10Rows   |    792.8 ?s |   3.70 ?s |   3.46 ?s |  1.00 |    0.00 |   3.9063 |   9.57 KB |        1.00 |
| Delete100Rows  |  7,144.7 ?s | 141.73 ?s | 125.64 ?s |  9.02 |    0.15 |  39.0625 |  91.15 KB |        9.52 |
| Delete1000Rows | 68,439.7 ?s | 978.96 ?s | 867.82 ?s | 86.38 |    1.14 | 142.8571 | 906.95 KB |       94.75 |
