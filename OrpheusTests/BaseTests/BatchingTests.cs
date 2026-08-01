using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusInterfaces.Core;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrpheusTests
{
    /// <summary>
    /// Verifies OrpheusTable's batched Insert/Update/Delete at and across BatchSize boundaries,
    /// asserting exact DB state after Save() — this is the class of bug (a row's data landing on
    /// the wrong key) that stays invisible until row counts cross a batch boundary. Includes a
    /// composite-key model, since everywhere else in the test suite only single-key models are used.
    /// </summary>
    public class BatchingTests : BaseTestClass
    {
        private const int TestBatchSize = 3;

        private List<TestModelTransactor> makeTransactors(int count)
        {
            var result = new List<TestModelTransactor>();
            for (var i = 0; i < count; i++)
            {
                result.Add(new TestModelTransactor
                {
                    TransactorId = Guid.NewGuid(),
                    Code = $"BATCH{i}",
                    Description = $"Row {i}",
                    Address = "Addr",
                    Email = $"row{i}@test.com",
                    Type = TestModelTransactorType.ttCustomer
                });
            }
            return result;
        }

        private void assertInsertedCorrectly(int rowCount)
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            table.BatchSize = TestBatchSize;
            var rows = this.makeTransactors(rowCount);
            table.Add(rows);
            table.Save();

            var reloadTable = this.Database.CreateTable<TestModelTransactor>();
            reloadTable.Load();

            Assert.AreEqual(rowCount, reloadTable.Data.Count);
            foreach (var row in rows)
            {
                var dbRow = reloadTable.Data.FirstOrDefault(r => r.TransactorId == row.TransactorId);
                Assert.IsNotNull(dbRow, $"Row {row.Code} should exist.");
                Assert.AreEqual(row.Description, dbRow.Description);
                Assert.AreEqual(row.Code, dbRow.Code);
            }

            this.DisconnectDatabase();
        }

        protected void TestInsertExactlyOneBatch()
        {
            this.assertInsertedCorrectly(TestBatchSize);
        }

        protected void TestInsertSpansTwoBatches()
        {
            this.assertInsertedCorrectly(TestBatchSize + 1);
        }

        private void assertUpdatedCorrectly(int updateCount)
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            table.BatchSize = TestBatchSize;
            // Two extra rows that are never queued for update — they must remain unchanged,
            // which is what would catch a batch spilling an update onto the wrong row.
            var rows = this.makeTransactors(updateCount + 2);
            table.Add(rows);
            table.Save();

            table.ClearData();
            table.Load();
            var toUpdate = table.Data.Take(updateCount).ToList();
            var toLeaveAlone = table.Data.Skip(updateCount).ToDictionary(r => r.TransactorId, r => r.Description);
            for (var i = 0; i < toUpdate.Count; i++)
            {
                toUpdate[i].Description = $"Updated-{i}-{toUpdate[i].TransactorId}";
                table.Update(toUpdate[i]);
            }
            table.Save();

            var reloadTable = this.Database.CreateTable<TestModelTransactor>();
            reloadTable.Load();
            foreach (var updatedRow in toUpdate)
            {
                var dbRow = reloadTable.Data.First(r => r.TransactorId == updatedRow.TransactorId);
                Assert.AreEqual(updatedRow.Description, dbRow.Description, $"Row {updatedRow.Code} should have its own, correctly-correlated update.");
            }
            foreach (var kv in toLeaveAlone)
            {
                var dbRow = reloadTable.Data.First(r => r.TransactorId == kv.Key);
                Assert.AreEqual(kv.Value, dbRow.Description, "Rows not queued for update should remain unchanged.");
            }

            this.DisconnectDatabase();
        }

        protected void TestUpdateExactlyOneBatch()
        {
            this.assertUpdatedCorrectly(TestBatchSize);
        }

        protected void TestUpdateSpansTwoBatches()
        {
            this.assertUpdatedCorrectly(TestBatchSize + 1);
        }

        private void assertDeletedCorrectly(int deleteCount)
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            table.BatchSize = TestBatchSize;
            // Two extra rows that are never queued for delete — they must survive, which is
            // what would catch a batch deleting the wrong row.
            var rows = this.makeTransactors(deleteCount + 2);
            table.Add(rows);
            table.Save();

            table.ClearData();
            table.Load();
            var toDelete = table.Data.Take(deleteCount).ToList();
            var toKeep = table.Data.Skip(deleteCount).ToList();
            foreach (var record in toDelete)
                table.Delete(record);
            table.Save();

            var reloadTable = this.Database.CreateTable<TestModelTransactor>();
            reloadTable.Load();
            Assert.AreEqual(toKeep.Count, reloadTable.Data.Count, "Only the targeted rows should have been deleted.");
            foreach (var keptRow in toKeep)
                Assert.IsTrue(reloadTable.Data.Any(r => r.TransactorId == keptRow.TransactorId), $"Row {keptRow.Code} should NOT have been deleted.");
            foreach (var deletedRow in toDelete)
                Assert.IsFalse(reloadTable.Data.Any(r => r.TransactorId == deletedRow.TransactorId), $"Row {deletedRow.Code} SHOULD have been deleted.");

            this.DisconnectDatabase();
        }

        protected void TestDeleteExactlyOneBatch()
        {
            this.assertDeletedCorrectly(TestBatchSize);
        }

        protected void TestDeleteSpansTwoBatches()
        {
            this.assertDeletedCorrectly(TestBatchSize + 1);
        }

        private List<TestModelCompositeKeyed> makeCompositeKeyed(int count)
        {
            var result = new List<TestModelCompositeKeyed>();
            for (var i = 0; i < count; i++)
            {
                result.Add(new TestModelCompositeKeyed
                {
                    KeyPart1 = Guid.NewGuid(),
                    KeyPart2 = Guid.NewGuid(),
                    Description = $"Composite {i}"
                });
            }
            return result;
        }

        // intializeModelProperties() only auto-populates KeyFields from single-property
        // [PrimaryKey] attributes, not the class-level [PrimaryCompositeKey] attribute — so
        // composite keys must be passed explicitly, same as TestLoadSpecificKeyValues does
        // elsewhere in this suite for a manually-specified single key.
        private IOrpheusTable<TestModelCompositeKeyed> createCompositeKeyedTable()
        {
            var tableOptions = this.Database.CreateTableOptions();
            tableOptions.TableName = "TestModelCompositeKeyed";
            tableOptions.AddKeyField("KeyPart1");
            tableOptions.AddKeyField("KeyPart2");
            return this.Database.CreateTable<TestModelCompositeKeyed>(tableOptions);
        }

        // OrpheusTable.Load() with no arguments resolves to the "simple load" overload, which
        // explicitly rejects tables with more than one key field (there's no single value list
        // to match against multiple keys). The Dictionary overload has no such restriction and
        // with no filters produces the same "load everything" SELECT.
        private static void loadAll(IOrpheusTable<TestModelCompositeKeyed> table)
        {
            table.Load(new Dictionary<string, List<object>>());
        }

        protected void TestCompositeKeyBatchedInsertUpdateDelete()
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.createCompositeKeyedTable();
            table.BatchSize = TestBatchSize;
            var rows = this.makeCompositeKeyed(TestBatchSize + 1); // span two batches throughout
            table.Add(rows);
            table.Save();

            var afterInsert = this.createCompositeKeyedTable();
            loadAll(afterInsert);
            Assert.AreEqual(rows.Count, afterInsert.Data.Count, "All composite-key rows should be inserted.");

            table.ClearData();
            loadAll(table);
            for (var i = 0; i < table.Data.Count; i++)
            {
                table.Data[i].Description = $"Updated-{i}";
                table.Update(table.Data[i]);
            }
            table.Save();

            var afterUpdate = this.createCompositeKeyedTable();
            loadAll(afterUpdate);
            foreach (var row in rows)
            {
                var dbRow = afterUpdate.Data.First(r => r.KeyPart1 == row.KeyPart1 && r.KeyPart2 == row.KeyPart2);
                Assert.IsTrue(dbRow.Description.StartsWith("Updated-"), "Composite-key row should have been updated via its correct key pair.");
            }

            table.ClearData();
            loadAll(table);
            var toDelete = table.Data.Take(TestBatchSize).ToList();
            var toKeep = table.Data.Skip(TestBatchSize).ToList();
            foreach (var record in toDelete)
                table.Delete(record);
            table.Save();

            var afterDelete = this.createCompositeKeyedTable();
            loadAll(afterDelete);
            Assert.AreEqual(toKeep.Count, afterDelete.Data.Count);
            foreach (var kept in toKeep)
                Assert.IsTrue(afterDelete.Data.Any(r => r.KeyPart1 == kept.KeyPart1 && r.KeyPart2 == kept.KeyPart2), "Kept composite-key row should still exist.");
            foreach (var deleted in toDelete)
                Assert.IsFalse(afterDelete.Data.Any(r => r.KeyPart1 == deleted.KeyPart1 && r.KeyPart2 == deleted.KeyPart2), "Deleted composite-key row should be gone.");

            this.DisconnectDatabase();
        }
    }
}
