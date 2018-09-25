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
