using BenchmarkDotNet.Attributes;
using Dapper;
using OrpheusTestModels;
using System.Collections.Generic;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// Orpheus vs Dapper vs EF Core — updating N existing rows.
    /// </summary>
    public class UpdateComparisonBenchMark : ComparisonBenchMarkBase
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
            for (var i = 0; i < rows.Count; i++)
            {
                rows[i].Address = $"Address{i}";
                rows[i].Code = $"Code{i}";
                rows[i].Description = $"Description{i}";
                rows[i].Email = $"Email{i}";
            }
        }

        [Benchmark(Baseline = true)]
        public void Dapper()
        {
            var connection = Database.DbConnection;
            using var tx = connection.BeginTransaction();
            foreach (var r in rows)
                connection.Execute(
                    "UPDATE TestModelTransactor SET Code=@Code, Description=@Description, Address=@Address, Email=@Email WHERE TransactorId=@TransactorId",
                    r, tx);
            tx.Commit();
        }

        [Benchmark]
        public void EFCore()
        {
            using var context = new BenchmarkDbContext(EfOptions);
            context.TestModelTransactors.UpdateRange(rows);
            context.SaveChanges();
        }

        [Benchmark]
        public void Orpheus()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            foreach (var r in rows)
                transactors.Update(r);
            transactors.Save();
        }
    }
}
