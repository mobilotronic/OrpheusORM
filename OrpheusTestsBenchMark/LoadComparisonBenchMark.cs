using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.EntityFrameworkCore;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// Orpheus vs Dapper vs EF Core — loading N existing rows one at a time by primary key.
    /// </summary>
    public class LoadComparisonBenchMark : ComparisonBenchMarkBase
    {
        [Params(10, 100, 1000)]
        public int RowCount { get; set; }

        private List<Guid> ids;

        [GlobalSetup]
        public void GlobalSetup()
        {
            ClearTable();
            var rows = GetTransactors(RowCount);
            SeedRows(rows);
            ids = rows.ConvertAll(r => r.TransactorId);
        }

        [Benchmark(Baseline = true)]
        public void Dapper()
        {
            var connection = Database.DbConnection;
            foreach (var id in ids)
            {
                connection.QuerySingleOrDefault<TestModelTransactor>(
                    "SELECT * FROM TestModelTransactor WHERE TransactorId = @Id", new { Id = id });
            }
        }

        [Benchmark]
        public void EFCore()
        {
            using var context = new BenchmarkDbContext(EfOptions);
            foreach (var id in ids)
            {
                context.TestModelTransactors.AsNoTracking().FirstOrDefault(t => t.TransactorId == id);
            }
        }

        [Benchmark]
        public void Orpheus()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            foreach (var id in ids)
            {
                transactors.Load(new List<object>() { id });
            }
        }
    }
}
