using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusTestModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrpheusTests
{
    /// <summary>
    /// Verifies OrpheusTable/OrpheusDatabase's async surface with real DB-state assertions
    /// (not just "didn't throw") — the async methods were only exercised by benchmark runs
    /// before these tests existed.
    /// </summary>
    public class AsyncTableTests : BaseTestClass
    {
        protected async Task TestAsyncInsertLoadRoundTrip()
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            table.Add(new TestModelTransactor { TransactorId = id1, Code = "A1", Description = "First", Address = "Addr1", Email = "a1@test.com", Type = TestModelTransactorType.ttCustomer });
            table.Add(new TestModelTransactor { TransactorId = id2, Code = "A2", Description = "Second", Address = "Addr2", Email = "a2@test.com", Type = TestModelTransactorType.ttSupplier });
            await table.SaveAsync();

            var reloadTable = this.Database.CreateTable<TestModelTransactor>();
            await reloadTable.LoadAsync();

            Assert.AreEqual(2, reloadTable.Data.Count);
            var first = reloadTable.Data.First(t => t.TransactorId == id1);
            Assert.AreEqual("A1", first.Code);
            Assert.AreEqual(TestModelTransactorType.ttCustomer, first.Type);

            this.DisconnectDatabase();
        }

        protected async Task TestAsyncLoadBySQL()
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            table.Add(new TestModelTransactor { TransactorId = Guid.NewGuid(), Code = "SQLTEST", Description = "SQL Test", Address = "Addr", Email = "sql@test.com", Type = TestModelTransactorType.ttCustomer });
            await table.SaveAsync();

            var queryTable = this.Database.CreateTable<TestModelTransactor>();
            await queryTable.LoadAsync($"SELECT * FROM TestModelTransactor WHERE {this.Database.DDLHelper.SafeFormatField("Code")} = 'SQLTEST'");

            Assert.AreEqual(1, queryTable.Data.Count);
            Assert.AreEqual("SQL Test", queryTable.Data.First().Description);

            this.DisconnectDatabase();
        }

        protected async Task TestAsyncLoadByCommand()
        {
            this.Initialize();
            this.ReCreateSchema();

            var id = Guid.NewGuid();
            var table = this.Database.CreateTable<TestModelTransactor>();
            table.Add(new TestModelTransactor { TransactorId = id, Code = "CMDTEST", Description = "Cmd Test", Address = "Addr", Email = "cmd@test.com", Type = TestModelTransactorType.ttCustomer });
            await table.SaveAsync();

            var cmd = this.Database.CreateCommand();
            cmd.CommandText = $"SELECT * FROM TestModelTransactor WHERE {this.Database.DDLHelper.SafeFormatField("TransactorId")} = @Id";
            var param = cmd.CreateParameter();
            param.ParameterName = "@Id";
            param.Value = id;
            cmd.Parameters.Add(param);

            var queryTable = this.Database.CreateTable<TestModelTransactor>();
            await queryTable.LoadAsync(cmd);

            Assert.AreEqual(1, queryTable.Data.Count);
            Assert.AreEqual("Cmd Test", queryTable.Data.First().Description);

            this.DisconnectDatabase();
        }

        protected async Task TestAsyncUpdatePersists()
        {
            this.Initialize();
            this.ReCreateSchema();

            var id = Guid.NewGuid();
            var table = this.Database.CreateTable<TestModelTransactor>();
            table.Add(new TestModelTransactor { TransactorId = id, Code = "U1", Description = "Before", Address = "Addr", Email = "u@test.com", Type = TestModelTransactorType.ttCustomer });
            await table.SaveAsync();

            table.ClearData();
            await table.LoadAsync();
            var record = table.Data.First(t => t.TransactorId == id);
            record.Description = "After";
            table.Update(record);
            await table.SaveAsync();

            var reloadTable = this.Database.CreateTable<TestModelTransactor>();
            await reloadTable.LoadAsync();
            Assert.AreEqual("After", reloadTable.Data.First(t => t.TransactorId == id).Description);

            this.DisconnectDatabase();
        }

        protected async Task TestAsyncDeleteRemovesRow()
        {
            this.Initialize();
            this.ReCreateSchema();

            var id = Guid.NewGuid();
            var table = this.Database.CreateTable<TestModelTransactor>();
            table.Add(new TestModelTransactor { TransactorId = id, Code = "D1", Description = "ToDelete", Address = "Addr", Email = "d@test.com", Type = TestModelTransactorType.ttCustomer });
            await table.SaveAsync();

            table.ClearData();
            await table.LoadAsync();
            var record = table.Data.First(t => t.TransactorId == id);
            table.Delete(record);
            await table.SaveAsync();

            var reloadTable = this.Database.CreateTable<TestModelTransactor>();
            await reloadTable.LoadAsync();
            Assert.IsFalse(reloadTable.Data.Any(t => t.TransactorId == id));

            this.DisconnectDatabase();
        }

        protected async Task TestAsyncLoadCancellation()
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var threw = false;
            try
            {
                await table.LoadAsync(cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Expected LoadAsync to throw OperationCanceledException for an already-cancelled token.");

            this.DisconnectDatabase();
        }

        protected async Task TestAsyncSaveCancellation()
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            table.Add(new TestModelTransactor { TransactorId = Guid.NewGuid(), Code = "C1", Description = "Cancelled", Address = "Addr", Email = "c@test.com", Type = TestModelTransactorType.ttCustomer });

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var threw = false;
            try
            {
                await table.SaveAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Expected SaveAsync to throw OperationCanceledException for an already-cancelled token.");

            this.DisconnectDatabase();
        }
    }
}
