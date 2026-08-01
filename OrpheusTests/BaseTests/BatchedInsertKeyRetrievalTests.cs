using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusTestModels;
using System.Collections.Generic;
using System.Linq;

namespace OrpheusTests
{
    /// <summary>
    /// Verifies OrpheusTable's batched-insert-with-DB-generated-key-retrieval path
    /// (see IOrpheusDDLHelper.ExecuteBatchedInsertWithKeyRetrieval). Orpheus's schema builder
    /// doesn't create identity/serial columns, so the underlying table is provisioned by hand.
    /// </summary>
    public class BatchedInsertKeyRetrievalTests : BaseTestClass
    {
        // Orpheus's schema builder can't create DB-generated (identity/serial) columns, so each
        // engine's DDL for the auto-generated key column is hand-provisioned here.
        private (string drop, string create) getIdentityKeyedTableDDL()
        {
            switch (this.DatabaseEngine)
            {
                case DbEngine.dbMySQL:
                    return ("DROP TABLE IF EXISTS TestModelIdentityKeyed",
                        "CREATE TABLE TestModelIdentityKeyed (Id INT AUTO_INCREMENT PRIMARY KEY, Description VARCHAR(120) NULL)");
                case DbEngine.dbPostgreSQL:
                    // Orpheus's own generated INSERT/SELECT statements quote column names (preserving
                    // exact case), so this hand-written table needs matching quoted columns too —
                    // otherwise "Description" (quoted) and description (folded-lowercase, unquoted)
                    // refer to different things as far as PostgreSQL is concerned.
                    return ("DROP TABLE IF EXISTS TestModelIdentityKeyed",
                        "CREATE TABLE TestModelIdentityKeyed (\"Id\" SERIAL PRIMARY KEY, \"Description\" VARCHAR(120) NULL)");
                case DbEngine.dbSQLServer:
                default:
                    return ("IF OBJECT_ID('TestModelIdentityKeyed', 'U') IS NOT NULL DROP TABLE TestModelIdentityKeyed",
                        "CREATE TABLE TestModelIdentityKeyed (Id INT IDENTITY(1,1) PRIMARY KEY, Description NVARCHAR(120) NULL)");
            }
        }

        protected void TestBatchedInsertWithKeyRetrieval()
        {
            this.Initialize();
            var ddl = this.getIdentityKeyedTableDDL();
            this.Database.ExecuteDDL(ddl.drop);
            this.Database.ExecuteDDL(ddl.create);

            var table = this.Database.CreateTable<TestModelIdentityKeyed>();
            table.BatchSize = 5; // force multiple batches for 12 rows, to exercise chunking too.

            var records = new List<TestModelIdentityKeyed>();
            for (var i = 0; i < 12; i++)
                records.Add(new TestModelIdentityKeyed { Description = $"Row {i}" });
            table.Add(records);
            table.Save();

            var ids = records.Select(r => r.Id).ToList();
            Assert.IsTrue(ids.All(id => id > 0), "Every record should have a positive generated Id.");
            Assert.AreEqual(ids.Count, ids.Distinct().Count(), "Generated Ids should be unique.");

            // Reload from the DB and confirm each generated Id correlates to the correct row
            // (not a different one — this is exactly what the MERGE+OUTPUT correlation column protects against).
            var reloadTable = this.Database.CreateTable<TestModelIdentityKeyed>();
            reloadTable.Load();
            foreach (var record in records)
            {
                var dbRecord = reloadTable.Data.FirstOrDefault(d => d.Id == record.Id);
                Assert.IsNotNull(dbRecord, $"Row with generated Id {record.Id} should exist in the DB.");
                Assert.AreEqual(record.Description, dbRecord.Description, "The generated key should correlate to the correct row, not a different one.");
            }

            this.DisconnectDatabase();
        }
    }
}
