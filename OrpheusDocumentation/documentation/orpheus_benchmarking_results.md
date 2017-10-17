# Benchmark results
 
## Machine details 
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Operating System          |Windows 10  |
 |CPU                       |I7          |
 |Database engine           |SQL Server  |
 |Database engine location  |Local       |
 |HDD type                  |SSD         |
 |RAM                       |8GB         |

## Benchmark type
 |Attribute                 | Value      |
 |---------------           |-----------:|
 |Serialization type        |POCO        |
 |Benchmark framework       |[BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)|
 |Performed on              |October 14th 2017|

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

 |         Method |      Mean |     Error |    StdDev | Scaled | ScaledSD |    Gen 0 |    Gen 1 |    Gen 2 |  Allocated |
 |--------------- |----------:|----------:|----------:|-------:|---------:|---------:|---------:|---------:|-----------:|
 |   Insert10Rows |  1.564 ms |  1.666 ms | 0.4328 ms |   1.00 |     0.00 |  11.7188 |   5.8594 |        - |    72.2 KB |
 |  Insert100Rows |  9.711 ms |  3.261 ms | 0.8470 ms |   6.62 |     1.75 |  93.7500 |  46.8750 |        - |   553.4 KB |
 | Insert1000Rows | 92.750 ms | 31.579 ms | 8.2026 ms |  63.21 |    16.78 | 875.0000 | 250.0000 | 125.0000 | 5391.61 KB |

## Load results
Loading and serializing one model/record per iteration. 

 |                 Method |       Mean |     Error |     StdDev | Scaled | ScaledSD |      Gen 0 |   Allocated |
 |----------------------- |-----------:|----------:|-----------:|-------:|---------:|-----------:|------------:|
 |   Load50RowsOneAtATime |   4.804 ms |  1.870 ms |  0.4856 ms |   1.00 |     0.00 |   156.2500 |   333.16 KB |
 |  Load500RowsOneAtATime |  44.574 ms |  5.552 ms |  1.4420 ms |   9.35 |     0.86 |  1625.0000 |  3331.54 KB |
 | Load5000RowsOneAtATime | 456.932 ms | 86.088 ms | 22.3610 ms |  95.87 |     9.36 | 16250.0000 | 33315.86 KB |

## Update results
Updating rows in a batch. One transaction per test. 4 model fields have been updated.

 |         Method |      Mean |     Error |    StdDev | Scaled | ScaledSD |     Gen 0 |    Gen 1 |  Allocated |
 |--------------- |----------:|----------:|----------:|-------:|---------:|----------:|---------:|-----------:|
 |   Update10Rows |  14.27 ms | 13.302 ms | 3.4551 ms |   1.00 |     0.00 |   93.7500 |  46.8750 |  578.02 KB |
 |  Update100Rows |  13.31 ms |  2.445 ms | 0.6352 ms |   0.97 |     0.16 |   93.7500 |  46.8750 |  578.02 KB |
 | Update1000Rows | 130.70 ms | 15.357 ms | 3.9890 ms |   9.50 |     1.59 | 1000.0000 | 250.0000 | 5592.98 KB |

## Delete results
Deleting rows in a batch. One transaction per test.

 |         Method |      Mean |      Error |    StdDev | Scaled | ScaledSD |    Gen 0 |    Gen 1 |  Allocated |
 |--------------- |----------:|-----------:|----------:|-------:|---------:|---------:|---------:|-----------:|
 |   Delete10Rows |  11.22 ms |   7.682 ms |  1.995 ms |   1.00 |     0.00 |        - |        - |   25.96 KB |
 |  Delete100Rows | 121.19 ms |  32.724 ms |  8.500 ms |  11.04 |     1.68 | 125.0000 |        - |  277.24 KB |
 | Delete1000Rows | 890.56 ms | 139.418 ms | 36.213 ms |  81.14 |    11.60 | 750.0000 | 250.0000 | 2578.13 KB |

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