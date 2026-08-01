using BenchmarkDotNet.Attributes;
using Dapper;
using OrpheusTestModels;
using System.Collections.Generic;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// Orpheus vs Dapper vs EF Core — deleting N existing rows.
    /// </summary>
    public class DeleteComparisonBenchMark : ComparisonBenchMarkBase
    {
        [Params(10, 100, 1000)]
        public int RowCount { get; set; }

        private List<TestModelTransactor> rows;

        [IterationSetup]
        public void IterationSetup()
        {
            ClearTable();
            rows = GetTransactors(RowCount);
            SeedRows(rows);
        }

        [Benchmark(Baseline = true)]
        public void Dapper()
        {
            var connection = Database.DbConnection;
            using var tx = connection.BeginTransaction();
            foreach (var r in rows)
                connection.Execute("DELETE FROM TestModelTransactor WHERE TransactorId=@TransactorId", new { r.TransactorId }, tx);
            tx.Commit();
        }

        [Benchmark]
        public void EFCore()
        {
            using var context = new BenchmarkDbContext(EfOptions);
            context.TestModelTransactors.RemoveRange(rows);
            context.SaveChanges();
        }

        [Benchmark]
        public void Orpheus()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            foreach (var r in rows)
                transactors.Delete(r);
            transactors.Save();
        }
    }
}
