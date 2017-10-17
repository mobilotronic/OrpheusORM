using BenchmarkDotNet.Attributes;
using OrpheusInterfaces;
using OrpheusTestModels;
using OrpheusTests;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusTestsBenchMark
{
    public class InsertDataBenchMark : BaseBenchMark
    {

        protected override void initializeBenchMark()
        {
            base.initializeBenchMark();
            this.ReCreateSchema();
        }

        [Benchmark(Baseline = true)]
        public void Insert10Rows()
        {
            var transactors = this.Database.CreateTable<TestModelTransactor>();
            var transactorsData = TestDatabase.GetTransactors(10);
            transactors.Add(transactorsData);
            transactors.Save();
        }

        [Benchmark]
        public void Insert100Rows()
        {
            var transactors = this.Database.CreateTable<TestModelTransactor>();
            var transactorsData = TestDatabase.GetTransactors(100);
            transactors.Add(transactorsData);
            transactors.Save();
        }

        [Benchmark]
        public void Insert1000Rows()
        {
            var transactors = this.Database.CreateTable<TestModelTransactor>();
            var transactorsData = TestDatabase.GetTransactors(1000);
            transactors.Add(transactorsData);
            transactors.Save();
        }
    }
}
