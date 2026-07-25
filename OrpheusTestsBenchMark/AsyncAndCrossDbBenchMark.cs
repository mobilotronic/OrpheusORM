using BenchmarkDotNet.Attributes;
using OrpheusTestModels;
using System.Threading.Tasks;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// Async benchmarks — measure throughput of the new async API vs sync baseline.
    /// </summary>
    public class AsyncInsertBenchMark : BaseBenchMark
    {
        [Benchmark(Baseline = true)]
        public void Insert100Rows_Sync()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(GetTransactors(100));
            transactors.Save();
        }

        [Benchmark]
        public async Task Insert100Rows_Async()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(GetTransactors(100));
            await transactors.SaveAsync();
        }

        [Benchmark]
        public void Insert1000Rows_Sync()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(GetTransactors(1000));
            transactors.Save();
        }

        [Benchmark]
        public async Task Insert1000Rows_Async()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(GetTransactors(1000));
            await transactors.SaveAsync();
        }
    }

    /// <summary>
    /// Cross-DB benchmark harness — runs identical operations against
    /// SQL Server, MySQL, and PostgreSQL for comparison.
    /// Configure connection strings via OrpheusSQLServer.config,
    /// OrpheusMySQL.config, and OrpheusPostgreSQL.config.
    /// </summary>
    public class CrossDatabaseBenchMark : BaseBenchMark
    {
        [Params("SQLServer", "MySQL", "PostgreSQL")]
        public new string DatabaseEngine { get; set; }

        [Benchmark]
        public void Insert100Rows()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(GetTransactors(100));
            transactors.Save();
        }

        [Benchmark]
        public async Task Insert100Rows_Async()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(GetTransactors(100));
            await transactors.SaveAsync();
        }
    }
}
