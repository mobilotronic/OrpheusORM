# Benchmark results
 
## Machine details 
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Operating System          |Windows 10  |
 |CPU                       |I7          |
 |Database engine           |SQL Server  |
 |Database engine location  |Local       |
 |HDD type                  |SSD         |
 |RAM                       |16GB         |

## Benchmark type
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Serialization type        |POCO        |
 |Benchmark framework       |[BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)|
 |Performed on              |September 24th 2018|
 |Orpheus version           |1.1.3|
 |Runtime                   |.NET Core 2.0.0 (CoreCLR 4.6.00001.0, CoreFX 4.6.25519.03), 64bit RyuJIT|

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

|         Method |      Mean |     Error |    StdDev | Scaled | ScaledSD |     Gen 0 |    Gen 1 |  Allocated |
|--------------- |----------:|----------:|----------:|-------:|---------:|----------:|---------:|-----------:|
|   Insert10Rows |  1.056 ms | 0.0789 ms | 0.0659 ms |   1.00 |     0.00 |   31.2500 |        - |   65.78 KB |
|  Insert100Rows |  7.785 ms | 0.5106 ms | 0.4776 ms |   7.40 |     0.59 |  234.3750 |        - |  491.71 KB |
| Insert1000Rows | 74.869 ms | 3.0477 ms | 2.8508 ms |  71.16 |     4.65 | 1000.0000 | 285.7143 | 4750.32 KB |

## Load results
Loading and serializing one model/record per iteration. 

|                 Method |       Mean |      Error |    StdDev | Scaled | ScaledSD |      Gen 0 |   Allocated |
|----------------------- |-----------:|-----------:|----------:|-------:|---------:|-----------:|------------:|
|   Load50RowsOneAtATime |   4.160 ms |  0.1161 ms | 0.1029 ms |   1.00 |     0.00 |   171.8750 |   360.12 KB |
|  Load500RowsOneAtATime |  41.372 ms |  1.2365 ms | 1.1566 ms |   9.95 |     0.36 |  1692.3077 |  3601.18 KB |
| Load5000RowsOneAtATime | 418.364 ms | 10.0790 ms | 8.9347 ms | 100.62 |     3.18 | 17500.0000 | 36009.47 KB |

## Update results
Updating rows in a batch. One transaction per test. 4 model fields have been updated.

|         Method |      Mean |     Error |    StdDev | Scaled | ScaledSD |     Gen 0 |    Gen 1 |  Allocated |
|--------------- |----------:|----------:|----------:|-------:|---------:|----------:|---------:|-----------:|
|   Update10Rows |  11.83 ms | 1.2164 ms | 1.0783 ms |   1.00 |     0.00 |  218.7500 |        - |  469.54 KB |
|  Update100Rows |  11.57 ms | 0.7881 ms | 0.7372 ms |   0.98 |     0.10 |  218.7500 |        - |  469.54 KB |
| Update1000Rows | 112.65 ms | 4.0897 ms | 3.4151 ms |   9.59 |     0.84 | 1000.0000 | 400.0000 | 4695.44 KB |

## Delete results
Deleting rows in a batch. One transaction per test.

|         Method |        Mean |      Error |     StdDev | Scaled | ScaledSD |    Gen 0 |    Gen 1 |  Allocated |
|--------------- |------------:|-----------:|-----------:|-------:|---------:|---------:|---------:|-----------:|
|   Delete10Rows |    690.5 us |   7.412 us |   5.787 us |   1.00 |     0.00 |   7.8125 |        - |   17.01 KB |
|  Delete100Rows |  6,057.5 us | 133.745 us | 111.683 us |   8.77 |     0.17 |  78.1250 |        - |  163.27 KB |
| Delete1000Rows | 59,989.5 us | 874.615 us | 730.343 us |  86.88 |     1.23 | 444.4444 | 111.1111 | 1625.84 KB |

## Legend
* **Mean**     : Arithmetic mean of all measurements
* **Error**    : Half of 99.9% confidence interval
* **StdDev**   : Standard deviation of all measurements
* **Scaled**   : Mean(CurrentBenchmark) / Mean(BaselineBenchmark)
* **ScaledSD** : Standard deviation of ratio of distribution of [CurrentBenchmark] and [BaselineBenchmark]
* **Gen 0**    : GC Generation 0 collects per 1k Operations
* **Gen 1**    : GC Generation 1 collects per 1k Operations
* **Allocated**: Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
* **1 ms**     : 1 Millisecond (0.001 sec)
* **1 us**     : 1 Microsecond (0.000001 sec)