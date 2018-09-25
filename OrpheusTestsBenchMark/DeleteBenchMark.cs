using BenchmarkDotNet.Attributes;
using OrpheusInterfaces.Core;
using OrpheusTestModels;
using System.Collections.Generic;

namespace OrpheusTestsBenchMark
{
    public class DeleteBenchMark : BaseBenchMark
    {
        private List<TestModelTransactor> data;
        private IOrpheusTable<TestModelTransactor> transactorsTable;
        protected override void initializeBenchMark()
        {
            base.initializeBenchMark();
            this.transactorsTable = this.Database.CreateTable<TestModelTransactor>();
            this.transactorsTable.Load();
            this.data = this.transactorsTable.Data;
        }

        [Benchmark(Baseline = true)]
        public void Delete10Rows()
        {
            for (var i = 0; i <= 9; i++)
            {
                this.transactorsTable.Delete(this.data[i]);
            }
            this.transactorsTable.Save();
        }

        [Benchmark]
        public void Delete100Rows()
        {
            for (var i = 100; i <= 199; i++)
            {
                this.transactorsTable.Delete(this.data[i]);
            }
            this.transactorsTable.Save();
        }

        [Benchmark]
        public void Delete1000Rows()
        {
            for (var i = 1000; i <= 1999; i++)
            {
                this.transactorsTable.Delete(this.data[i]);
            }
            this.transactorsTable.Save();
        }
    }
}
