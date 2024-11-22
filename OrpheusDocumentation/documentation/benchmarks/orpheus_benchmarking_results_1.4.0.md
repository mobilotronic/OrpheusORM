# Benchmark results
 
## Machine details 
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Operating System          |Windows 11  |
 |CPU                       |AMD Ryzen 5 5500U 1 CPU, 12 logical 6 physical|
 |Database engine           |SQL Server  |
 |Database engine location  |Local       |
 |HDD type                  |SSD         |
 |RAM                       |16GB         |

## Benchmark type
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Serialization type        |POCO        |
 |Benchmark framework       |[BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)|
 |Performed on              |November 21st 2024|
 |Orpheus version           |1.4.0|
 |Runtime                   |BenchmarkDotNet v0.13.8, Windows 11 (10.0.22631.4460)|
 |                          |.NET SDK 8.0.202|
 |                          | Job-KCZQOK : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2|

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

| Method         | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Allocated | Alloc Ratio |
|--------------- |----------:|----------:|----------:|------:|--------:|---------:|---------:|----------:|------------:|
| Insert10Rows   |  1.291 ms | 0.0742 ms | 0.0694 ms |  1.00 |    0.00 |  23.4375 |        - |  52.96 KB |        1.00 |
| Insert100Rows  |  9.811 ms | 0.7585 ms | 0.7095 ms |  7.63 |    0.81 | 171.8750 |        - | 365.26 KB |        6.90 |
| Insert1000Rows | 94.304 ms | 4.6471 ms | 4.3469 ms | 73.28 |    5.69 | 666.6667 | 333.3333 | 3574.1 KB |       67.48 |

## Load results
Loading and serializing one model/record per iteration. 

| Method                 | Mean       | Error      | StdDev     | Ratio | RatioSD | Gen0       | Allocated   | Alloc Ratio |
|----------------------- |-----------:|-----------:|-----------:|------:|--------:|-----------:|------------:|------------:|
| Load50RowsOneAtATime   |   4.517 ms |  0.0219 ms |  0.0205 ms |  1.00 |    0.00 |   117.1875 |   254.07 KB |        1.00 |
| Load500RowsOneAtATime  |  42.527 ms |  0.1537 ms |  0.1362 ms |  9.41 |    0.05 |  1230.7692 |  2540.65 KB |       10.00 |
| Load5000RowsOneAtATime | 433.219 ms | 17.0756 ms | 15.9725 ms | 95.90 |    3.47 | 12250.0000 | 25406.77 KB |      100.00 |

## Update results
Updating rows in a batch. One transaction per test. 4 model fields have been updated.

| Method         | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0     | Gen1     | Allocated  | Alloc Ratio |
|--------------- |----------:|---------:|---------:|------:|--------:|---------:|---------:|-----------:|------------:|
| Update10Rows   |  13.14 ms | 0.654 ms | 0.612 ms |  1.00 |    0.00 | 156.2500 |        - |  345.05 KB |        1.00 |
| Update100Rows  |  11.98 ms | 0.343 ms | 0.321 ms |  0.91 |    0.04 | 156.2500 |        - |  345.05 KB |        1.00 |
| Update1000Rows | 119.99 ms | 3.536 ms | 3.307 ms |  9.15 |    0.51 | 500.0000 | 250.0000 | 3446.09 KB |        9.99 |

## Delete results
Deleting rows in a batch. One transaction per test.

| Method         | Mean        | Error     | StdDev    | Ratio | RatioSD | Gen0     | Allocated | Alloc Ratio |
|--------------- |------------:|----------:|----------:|------:|--------:|---------:|----------:|------------:|
| Delete10Rows   |    937.7 μs |  16.89 μs |  15.80 μs |  1.00 |    0.00 |   3.9063 |   8.68 KB |        1.00 |
| Delete100Rows  |  7,720.3 μs | 360.45 μs | 337.17 μs |  8.23 |    0.35 |  31.2500 |  82.53 KB |        9.51 |
| Delete1000Rows | 77,875.9 μs | 338.68 μs | 316.80 μs | 83.07 |    1.34 | 142.8571 | 820.94 KB |       94.56 |
