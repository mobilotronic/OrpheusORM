using BenchmarkDotNet.Attributes;
using Dapper;
using OrpheusTestModels;
using System.Collections.Generic;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// Orpheus vs Dapper vs EF Core — inserting N new rows.
    /// </summary>
    public class InsertComparisonBenchMark : ComparisonBenchMarkBase
    {
        [Params(10, 100, 1000)]
        public int RowCount { get; set; }

        private List<TestModelTransactor> rows;

        [IterationSetup]
        public void IterationSetup()
        {
            ClearTable();
            rows = GetTransactors(RowCount);
        }

        [Benchmark(Baseline = true)]
        public void Dapper()
        {
            var connection = Database.DbConnection;
            using var tx = connection.BeginTransaction();
            foreach (var r in rows)
                connection.Execute(
                    "INSERT INTO TestModelTransactor (TransactorId, Code, Description, Address, Email, [Type]) " +
                    "VALUES (@TransactorId, @Code, @Description, @Address, @Email, @Type)",
                    new { r.TransactorId, r.Code, r.Description, r.Address, r.Email, Type = (int)r.Type }, tx);
            tx.Commit();
        }

        [Benchmark]
        public void EFCore()
        {
            using var context = new BenchmarkDbContext(EfOptions);
            context.TestModelTransactors.AddRange(rows);
            context.SaveChanges();
        }

        [Benchmark]
        public void Orpheus()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(rows);
            transactors.Save();
        }
    }
}
