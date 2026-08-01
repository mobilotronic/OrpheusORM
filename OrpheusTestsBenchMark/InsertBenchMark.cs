using BenchmarkDotNet.Attributes;
using OrpheusTestModels;

namespace OrpheusTestsBenchMark
{
    public class InsertDataBenchMark : BaseBenchMark
    {
        protected override void initializeBenchMark()
        {
            base.initializeBenchMark();
        }

        // BenchmarkDotNet invokes each [Benchmark] method many times (pilot + warmup + actual
        // stages) to calibrate and measure. TransactorId is a randomly-ordered Guid clustered
        // primary key, so without resetting between invocations the table keeps growing across
        // the run and later invocations pay increasing index page-split cost, skewing the
        // reported Mean upward. Reset before every invocation so each one measures against the
        // same, bounded table size.
        [IterationSetup]
        public void IterationSetup()
        {
            this.Database.ExecuteDDL("DELETE FROM TestModelTransactor");
        }

        [Benchmark(Baseline = true)]
        public void Insert10Rows()
        {
            var transactors = this.Database.CreateTable<TestModelTransactor>();
            transactors.Add(this.GetTransactors(10));
            transactors.Save();
        }

        [Benchmark]
        public void Insert100Rows()
        {
            var transactors = this.Database.CreateTable<TestModelTransactor>();
            transactors.Add(this.GetTransactors(100));
            transactors.Save();
        }

        [Benchmark]
        public void Insert1000Rows()
        {
            var transactors = this.Database.CreateTable<TestModelTransactor>();
            transactors.Add(this.GetTransactors(1000));
            transactors.Save();
        }
    }
}
