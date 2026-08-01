# Benchmark results

## Machine details
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Operating System          |Windows 11 (25H2)|
 |CPU                       |AMD Ryzen 5 2600 3.40GHz, 1 CPU, 12 logical, 6 physical|
 |Database engine           |SQL Server  |
 |Database engine location  |Local       |
 |HDD type                  |SSD         |

## Benchmark type
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Serialization type        |POCO        |
 |Benchmark framework       |[BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)|
 |Compared against          |[Dapper](https://github.com/DapperLib/Dapper) 2.1.66, [Entity Framework Core](https://github.com/dotnet/efcore) 9.0.13|
 |Orpheus version           |2.0.0|
 |Runtime                   |BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8875/25H2/2025Update/HudsonValley2)|
 |                          |.NET SDK 10.0.201|
 |                          |Job-BDGAVX : .NET 9.0.14 (9.0.14, 9.0.1426.11910), X64 RyuJIT x86-64-v3|
 |                          |Jit=RyuJit, Toolchain=.NET 9.0, InvocationCount=1, IterationCount=30, LaunchCount=1, RunStrategy=Monitoring, UnrollFactor=1, WarmupCount=30|

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

## About this comparison

Starting with 2.0.0, benchmarks compare Orpheus directly against [Dapper](https://github.com/DapperLib/Dapper) (a micro-ORM, closest in spirit to Orpheus's explicit CRUD model) and [Entity Framework Core](https://github.com/dotnet/efcore) (the dominant full-scale .NET ORM), across Insert/Load/Update/Delete at 10, 100, and 1000 rows. `Ratio`/`RatioSD` are relative to Dapper's mean at that row count (Dapper = 1.00).

## Insert results
Inserting rows in a batch. One transaction per test.

| Method  | RowCount | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0      | Allocated  | Alloc Ratio |
|-------- |--------- |-----------:|----------:|----------:|------:|--------:|----------:|-----------:|------------:|
| Dapper  | 10       |   2.723 ms | 0.1486 ms | 0.2224 ms |  1.01 |    0.11 |         - |   91.41 KB |        1.00 |
| EFCore  | 10       |   2.475 ms | 0.3401 ms | 0.5090 ms |  0.91 |    0.20 |         - |  217.27 KB |        2.38 |
| Orpheus | 10       |   1.040 ms | 0.1150 ms | 0.1722 ms |  0.38 |    0.07 |         - |   91.27 KB |        1.00 |
| Dapper  | 100      |  16.538 ms | 0.3115 ms | 0.4663 ms |  1.00 |    0.04 |         - |  907.51 KB |        1.00 |
| EFCore  | 100      |  11.679 ms | 0.5401 ms | 0.8083 ms |  0.71 |    0.05 |         - | 1033.85 KB |        1.14 |
| Orpheus | 100      |   9.663 ms | 0.2378 ms | 0.3559 ms |  0.58 |    0.03 |         - |  671.18 KB |        0.74 |
| Dapper  | 1000     | 146.703 ms | 1.0140 ms | 1.5177 ms |  1.00 |    0.01 | 2000.0000 | 9102.04 KB |        1.00 |
| EFCore  | 1000     |  49.575 ms | 3.9156 ms | 5.8607 ms |  0.34 |    0.04 | 1000.0000 | 8793.63 KB |        0.97 |
| Orpheus | 1000     |  71.401 ms | 0.5559 ms | 0.8320 ms |  0.49 |    0.01 | 1000.0000 | 6521.23 KB |        0.72 |

## Load results
Loading and deserializing rows into models.

| Method  | RowCount | Mean       | Error     | StdDev    | Ratio | RatioSD | Gen0      | Allocated   | Alloc Ratio |
|-------- |--------- |-----------:|----------:|----------:|------:|--------:|----------:|------------:|------------:|
| Dapper  | 10       |   2.276 ms | 0.1634 ms | 0.2446 ms |  1.01 |    0.14 |         - |    78.83 KB |        1.00 |
| EFCore  | 10       |   5.317 ms | 0.3125 ms | 0.4678 ms |  2.36 |    0.30 |         - |    273.2 KB |        3.47 |
| Orpheus | 10       |   2.383 ms | 0.6339 ms | 0.9488 ms |  1.06 |    0.43 |         - |    97.95 KB |        1.24 |
| Dapper  | 100      |  15.735 ms | 0.3669 ms | 0.5491 ms |  1.00 |    0.05 |         - |   789.06 KB |        1.00 |
| EFCore  | 100      |  36.910 ms | 1.8727 ms | 2.8029 ms |  2.35 |    0.19 |         - |  1663.52 KB |        2.11 |
| Orpheus | 100      |  15.484 ms | 0.2731 ms | 0.4088 ms |  0.99 |    0.04 |         - |   804.98 KB |        1.02 |
| Dapper  | 1000     | 145.292 ms | 0.5676 ms | 0.8495 ms |  1.00 |    0.01 | 1000.0000 |  7913.28 KB |        1.00 |
| EFCore  | 1000     | 265.809 ms | 1.7313 ms | 2.5914 ms |  1.83 |    0.02 | 3000.0000 | 15500.87 KB |        1.96 |
| Orpheus | 1000     | 146.665 ms | 0.7704 ms | 1.1531 ms |  1.01 |    0.01 | 1000.0000 |  7885.61 KB |        1.00 |

## Update results
Updating rows in a batch. One transaction per test.

| Method  | RowCount | Mean       | Error      | StdDev     | Ratio | RatioSD | Gen0      | Allocated   | Alloc Ratio |
|-------- |--------- |-----------:|-----------:|-----------:|------:|--------:|----------:|------------:|------------:|
| Dapper  | 10       |   2.577 ms |  0.1394 ms |  0.2086 ms |  1.01 |    0.11 |         - |    86.34 KB |        1.00 |
| EFCore  | 10       |   3.036 ms |  0.3757 ms |  0.5624 ms |  1.19 |    0.24 |         - |   234.09 KB |        2.71 |
| Orpheus | 10       |   1.594 ms |  0.1175 ms |  0.1758 ms |  0.62 |    0.08 |         - |   116.78 KB |        1.35 |
| Dapper  | 100      |  16.084 ms |  0.2950 ms |  0.4416 ms |  1.00 |    0.04 |         - |   837.98 KB |        1.00 |
| EFCore  | 100      |   9.615 ms |  1.4803 ms |  2.2156 ms |  0.60 |    0.14 |         - |  1164.77 KB |        1.39 |
| Orpheus | 100      |  14.039 ms |  1.7706 ms |  2.6502 ms |  0.87 |    0.16 |         - |   922.63 KB |        1.10 |
| Dapper  | 1000     | 160.447 ms |  8.6981 ms | 13.0190 ms |  1.01 |    0.11 | 1000.0000 |   8391.1 KB |        1.00 |
| EFCore  | 1000     |  57.765 ms |  3.6808 ms |  5.5092 ms |  0.36 |    0.04 | 1000.0000 | 10341.16 KB |        1.23 |
| Orpheus | 1000     | 134.292 ms | 10.5359 ms | 15.7696 ms |  0.84 |    0.11 | 1000.0000 |  9038.82 KB |        1.08 |

## Delete results
Deleting rows in a batch. One transaction per test.

| Method  | RowCount | Mean         | Error      | StdDev     | Ratio | RatioSD | Gen0      | Allocated  | Alloc Ratio |
|-------- |--------- |-------------:|-----------:|-----------:|------:|--------:|----------:|-----------:|------------:|
| Dapper  | 10       |   2,537.8 μs |   160.8 μs |   240.6 μs |  1.01 |    0.13 |         - |   71.02 KB |        1.00 |
| EFCore  | 10       |   2,552.8 μs |   156.9 μs |   234.9 μs |  1.01 |    0.13 |         - |  185.79 KB |        2.62 |
| Orpheus | 10       |     915.1 μs |   346.4 μs |   518.4 μs |  0.36 |    0.21 |         - |   42.16 KB |        0.59 |
| Dapper  | 100      |  15,918.2 μs |   195.7 μs |   292.9 μs |  1.00 |    0.03 |         - |  680.16 KB |        1.00 |
| EFCore  | 100      |   7,674.1 μs |   728.9 μs | 1,090.9 μs |  0.48 |    0.07 |         - |  703.82 KB |        1.03 |
| Orpheus | 100      |   4,831.4 μs | 3,077.5 μs | 4,606.2 μs |  0.30 |    0.28 |         - |  190.41 KB |        0.28 |
| Dapper  | 1000     | 146,573.5 μs |   987.4 μs | 1,477.9 μs |  1.00 |    0.01 | 1000.0000 | 6812.98 KB |        1.00 |
| EFCore  | 1000     |  27,842.9 μs | 3,690.4 μs | 5,523.7 μs |  0.19 |    0.04 | 1000.0000 | 5644.77 KB |        0.83 |
| Orpheus | 1000     |  35,881.0 μs |   478.1 μs |   715.5 μs |  0.24 |    0.01 |         - |  1714.8 KB |        0.25 |
