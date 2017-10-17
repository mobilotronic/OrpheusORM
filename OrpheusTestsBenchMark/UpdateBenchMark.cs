using BenchmarkDotNet.Attributes;
using OrpheusInterfaces;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusTestsBenchMark
{
    public class UpdateBenchMark : BaseBenchMark
    {
        private List<TestModelTransactor> data;
        private IOrpheusTable<TestModelTransactor> transactorsTable;
        protected override void initializeBenchMark()
        {
            base.initializeBenchMark();
            this.transactorsTable = this.Database.CreateTable<TestModelTransactor>();
            this.transactorsTable.Load();
            this.data = this.transactorsTable.Data;
            for(var i = 0; i <= this.data.Count-1; i++)
            {
                var transactor = this.data[i];
                transactor.Address = String.Format("Address{0}", i);
                transactor.Code = String.Format("Code{0}", i);
                transactor.Description = String.Format("Description{0}", i);
                transactor.Email = String.Format("Email{0}", i);
                this.data[i] = transactor;
            }

        }

        [Benchmark(Baseline = true)]
        public void Update10Rows()
        {
            for (var i = 0; i <= 99; i++)
            {
                this.transactorsTable.Update(this.data[i]);
            }
            this.transactorsTable.Save();
        }

        [Benchmark]
        public void Update100Rows()
        {
            for (var i = 0; i <= 99; i++)
            {
                this.transactorsTable.Update(this.data[i]);
            }
            this.transactorsTable.Save();
        }

        [Benchmark]
        public void Update1000Rows()
        {
            for (var i = 0; i <= 999; i++)
            {
                this.transactorsTable.Update(this.data[i]);
            }
            this.transactorsTable.Save();
        }
    }
}
