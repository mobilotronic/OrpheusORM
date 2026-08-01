using BenchmarkDotNet.Attributes;
using Dapper;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// Dapper vs Orpheus comparison — identical operations (individual parameterized INSERTs).
    /// </summary>
    public class DapperInsertBenchMark : BaseBenchMark
    {
        private IDbConnection GetConnection()
        {
            var c = Database.DbConnection;
            if (c.State != ConnectionState.Open) c.Open();
            return c;
        }

        [Benchmark(Baseline = true)]
        public void Insert10Rows()
        {
            var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            var records = GetTransactors(10);
            foreach (var r in records)
                conn.Execute(
                    "INSERT INTO TestModelTransactor (TransactorId, Code, Description, Address, Email, [Type]) " +
                    "VALUES (@TransactorId, @Code, @Description, @Address, @Email, @Type)",
                    new { r.TransactorId, r.Code, r.Description, r.Address, r.Email, Type = (int)r.Type }, tx);
            tx.Commit();
        }

        [Benchmark]
        public void Insert100Rows()
        {
            var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            var records = GetTransactors(100);
            foreach (var r in records)
                conn.Execute(
                    "INSERT INTO TestModelTransactor (TransactorId, Code, Description, Address, Email, [Type]) " +
                    "VALUES (@TransactorId, @Code, @Description, @Address, @Email, @Type)",
                    new { r.TransactorId, r.Code, r.Description, r.Address, r.Email, Type = (int)r.Type }, tx);
            tx.Commit();
        }

        [Benchmark]
        public void Insert1000Rows()
        {
            var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            var records = GetTransactors(1000);
            foreach (var r in records)
                conn.Execute(
                    "INSERT INTO TestModelTransactor (TransactorId, Code, Description, Address, Email, [Type]) " +
                    "VALUES (@TransactorId, @Code, @Description, @Address, @Email, @Type)",
                    new { r.TransactorId, r.Code, r.Description, r.Address, r.Email, Type = (int)r.Type }, tx);
            tx.Commit();
        }

        [Benchmark]
        public void OrpheusInsert100Rows()
        {
            var transactors = Database.CreateTable<TestModelTransactor>();
            transactors.Add(GetTransactors(100));
            transactors.Save();
        }
    }
}
