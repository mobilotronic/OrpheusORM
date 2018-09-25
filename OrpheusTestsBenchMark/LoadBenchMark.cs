using BenchmarkDotNet.Attributes;
using OrpheusInterfaces.Core;
using OrpheusTestModels;
using System.Collections.Generic;

namespace OrpheusTestsBenchMark
{
    public class LoadBenchMark : BaseBenchMark
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

        [Benchmark(Baseline =true)]
        public void Load50RowsOneAtATime()
        {
            for(var i = 0; i <= 49; i++)
            {
                this.transactorsTable.Load(new List<object>()
                {
                    this.data[i].TransactorId
                });
            }
        }

        [Benchmark]
        public void Load500RowsOneAtATime()
        {
            for (var i = 0; i <= 499; i++)
            {
                this.transactorsTable.Load(new List<object>()
                {
                    this.data[i].TransactorId
                });
            }
        }

        [Benchmark]
        public void Load5000RowsOneAtATime()
        {
            for (var i = 0; i <= 4999; i++)
            {
                this.transactorsTable.Load(new List<object>()
                {
                    this.data[i].TransactorId
                });
            }
        }
    }
}
