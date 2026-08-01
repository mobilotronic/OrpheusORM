using Microsoft.EntityFrameworkCore;
using OrpheusTestModels;
using System.Collections.Generic;
using System.Data;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// Shared setup for the Orpheus vs Dapper vs EF Core comparison benchmarks.
    /// Every subclass measures the same operation (insert/load/update/delete)
    /// across all three frameworks against TestModelTransactor, using the same
    /// row counts, so the BenchmarkDotNet Ratio column is a fair comparison.
    /// </summary>
    public abstract class ComparisonBenchMarkBase : BaseBenchMark
    {
        protected DbContextOptions<BenchmarkDbContext> EfOptions;

        protected override void initializeBenchMark()
        {
            base.initializeBenchMark();
            // EF Core idiomatically owns its own connection lifecycle (short-lived
            // DbContext, pooled ADO.NET connections) rather than sharing the single
            // long-held connection Orpheus/Dapper use here, so it gets its own
            // connection string instead of Database.DbConnection.
            EfOptions = new DbContextOptionsBuilder<BenchmarkDbContext>()
                .UseSqlServer(Database.ConnectionString)
                .Options;
        }

        protected void ClearTable()
        {
            Database.ExecuteDDL("DELETE FROM TestModelTransactor");
        }

        /// <summary>
        /// Inserts rows using plain ADO.NET, independent of Orpheus/Dapper/EF Core,
        /// so seeding data for Load/Update/Delete benchmarks doesn't advantage or
        /// count against any of the three frameworks being compared.
        /// </summary>
        protected void SeedRows(List<TestModelTransactor> rows)
        {
            var connection = Database.DbConnection;
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO TestModelTransactor (TransactorId, Code, Description, Address, Email, [Type]) " +
                "VALUES (@TransactorId, @Code, @Description, @Address, @Email, @Type)";

            var transactorId = command.CreateParameter();
            transactorId.ParameterName = "@TransactorId";
            command.Parameters.Add(transactorId);
            var code = command.CreateParameter();
            code.ParameterName = "@Code";
            command.Parameters.Add(code);
            var description = command.CreateParameter();
            description.ParameterName = "@Description";
            command.Parameters.Add(description);
            var address = command.CreateParameter();
            address.ParameterName = "@Address";
            command.Parameters.Add(address);
            var email = command.CreateParameter();
            email.ParameterName = "@Email";
            command.Parameters.Add(email);
            var type = command.CreateParameter();
            type.ParameterName = "@Type";
            command.Parameters.Add(type);

            foreach (var row in rows)
            {
                transactorId.Value = row.TransactorId;
                code.Value = row.Code;
                description.Value = row.Description;
                address.Value = row.Address;
                email.Value = row.Email;
                type.Value = (int)row.Type;
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }
    }
}
