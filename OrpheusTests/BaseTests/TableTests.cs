using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusInterfaces.Core;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace OrpheusTests
{
    public class TableTests : BaseTestClass
    {
        /// <summary>
        /// Tests the command text and parameters created for a model.
        /// </summary>
        protected void TestCreateCommandRandomData()
        {
            this.Initialize();
            this.ReCreateSchema();
            var tableOptions = this.Database.CreateTableOptions();
            tableOptions.TableName = "TestModelUser";
            tableOptions.KeyFields = new List<IOrpheusTableKeyField>();
            var usersTable = this.Database.CreateTable<TestModelUser>(tableOptions);
            var recordCount = 1000;
            usersTable.Add(this.GetRandomUsersForTesting(recordCount));

            IDbTransaction trans = this.Database.BeginTransaction();
            //var deleteCommand = this.Database.CreateCommand();
            Stopwatch sw = new Stopwatch();
            try
            {
                //deleteCommand.CommandText = "DELETE FROM TestModelUser";
                //deleteCommand.Transaction = trans;
                //deleteCommand.ExecuteNonQuery();
                Trace.TraceInformation(DateTime.Now.ToString() + " Running TestCreateCommand - Creating " + recordCount.ToString() + " records");
                sw.Start();
                usersTable.ExecuteInserts(trans);
                this.Database.CommitTransaction(trans);
                sw.Stop();
                Trace.TraceInformation("{0:c}", sw.Elapsed);
                Trace.Close();
                this.DisconnectDatabase();
            }
            catch
            {
                this.Database.RollbackTransaction(trans);
                this.DisconnectDatabase();
                throw;
            }
        }

        protected void TestUpdateCommandRandomData()
        {
            this.Initialize();
            this.ReCreateSchema();
            var tableOptions = this.Database.CreateTableOptions();
            tableOptions.TableName = "TestModelUser";
            tableOptions.KeyFields = new List<IOrpheusTableKeyField>();
            var keyField = this.Database.CreateTableKeyField();
            keyField.Name = "UserId";
            tableOptions.KeyFields.Add(keyField);

            var usersTable = this.Database.CreateTable<TestModelUser>(tableOptions);
            //adding data.
            usersTable.Add(this.GetRandomUsersForTesting());
            using(var tr = this.Database.BeginTransaction())
            {
                usersTable.ExecuteInserts(tr);
                try
                {
                    this.Database.CommitTransaction(tr);
                }
                catch
                {
                    this.Database.RollbackTransaction(tr);
                    this.DisconnectDatabase();
                    throw;
                }
            }

            usersTable.Load();
            var tempData = usersTable.Data;
            usersTable.ClearData();

            foreach (var usr in tempData)
            {
                usr.PasswordHash = "test";
                usersTable.Update(usr);
            }
            Stopwatch sw = new Stopwatch();
            IDbTransaction trans = this.Database.BeginTransaction();
            try
            {
                Trace.TraceInformation(DateTime.Now.ToString() + " Running TestUpdateCommand - Updating " + usersTable.Data.Count.ToString() + " records");
                sw.Start();
                usersTable.ExecuteUpdates(trans);
                this.Database.CommitTransaction(trans);
                sw.Stop();
                Trace.TraceInformation("{0:c}", sw.Elapsed);
                Trace.Close();
            }
            catch
            {
                this.Database.RollbackTransaction(trans);
                this.DisconnectDatabase();
                throw;
            }
            this.DisconnectDatabase();
        }

        protected void TestDeleteCommandRandomData()
        {
            this.Initialize();
            this.ReCreateSchema();
            var tableOptions = this.Database.CreateTableOptions();
            tableOptions.TableName = "TestModelUser";
            tableOptions.KeyFields = new List<IOrpheusTableKeyField>();
            var keyField = this.Database.CreateTableKeyField();
            keyField.Name = "UserId";
            tableOptions.KeyFields.Add(keyField);
            var usersTable = this.Database.CreateTable<TestModelUser>(tableOptions);

            //adding data.
            usersTable.Add(this.GetRandomUsersForTesting());
            using (var tr = this.Database.BeginTransaction())
            {
                usersTable.ExecuteInserts(tr);
                try
                {
                    this.Database.CommitTransaction(tr);
                }
                catch
                {
                    this.Database.RollbackTransaction(tr);
                    this.DisconnectDatabase();
                    throw;
                }
            }

            usersTable.Load();

            usersTable.Delete(usersTable.Data.Where(usr => usr.Email.IndexOf("info") >= 0).ToList());

            Stopwatch sw = new Stopwatch();
            IDbTransaction trans = this.Database.BeginTransaction();
            try
            {
                Trace.TraceInformation(DateTime.Now.ToString() + " Running TestDeleteCommand - Updating " + usersTable.Data.Count.ToString() + " records");
                sw.Start();
                usersTable.ExecuteDeletes(trans);
                this.Database.CommitTransaction(trans);
                sw.Stop();
                Trace.TraceInformation("{0:c}", sw.Elapsed);
                Trace.Close();
            }
            catch
            {
                this.Database.RollbackTransaction(trans);
                this.DisconnectDatabase();
                throw;
            }
            this.DisconnectDatabase();
        }

        protected void TestPrimaryKeyInfer()
        {
            this.Initialize();
            var transactorsTable = this.Database.CreateTable<TestModelTransactor>("TEST");

            Assert.IsTrue(transactorsTable.KeyFields.Count == 1, "Expected to have one primary key.");
            Assert.IsTrue(transactorsTable.KeyFields.First().Name == "TransactorId", "Expected TransactorId to be the primary key.");
            this.DisconnectDatabase();
        }

        protected void TestKeyValueAutoGenerate()
        {
            this.Initialize();
            var transactorsTable = this.Database.CreateTable<TestModelTransactor>("TEST");
            transactorsTable.Add(new TestModelTransactor() {
                Address = "Test",
                Code = "001",
                Description = "Transactor",
                Type = TestModelTransactorType.ttCustomer,
                Email = "test@email.com"
            });
            Assert.IsTrue(transactorsTable.Data.Where(t => t.Code == "001").First().TransactorId != Guid.Empty, "Expected to have a transactor Id");
            var transactorId = Guid.NewGuid();
            transactorsTable.Add(new TestModelTransactor()
            {
                TransactorId = transactorId,
                Address = "Test",
                Code = "002",
                Description = "Transactor",
                Type = TestModelTransactorType.ttCustomer,
                Email = "test@email.com"
            });
            Assert.IsTrue(transactorsTable.Data.Where(t => t.Code == "002").First().TransactorId.Equals(transactorId), "Expected to keep the existing transactor Id");

            var itemsTable = this.Database.CreateTable<TestModelItem>("TEST_ITEMS");
            itemsTable.Add(new TestModelItem() {
                Code = "001",
                Description = "Item",
                Price = 10
            });
            Assert.IsTrue(itemsTable.Data.Where(t => t.Code == "001").First().ItemId == Guid.Empty, "Expected to have a null itemId");
            itemsTable.Add(new TestModelItem()
            {
                ItemId = Guid.NewGuid(),
                Code = "002",
                Description = "Item",
                Price = 10
            });
            Assert.IsTrue(itemsTable.Data.First(t => t.Code == "002").ItemId != Guid.Empty, "Expected to have a itemId");
            this.DisconnectDatabase();
        }

        protected void TestLoadSpecificKeyValues()
        {
            this.Initialize();
            this.ReCreateSchema();

            var tableOptions = this.Database.CreateTableOptions();
            tableOptions.TableName = "TestModelUser";
            tableOptions.KeyFields = new List<IOrpheusTableKeyField>();
            var keyField = this.Database.CreateTableKeyField();
            keyField.Name = "UserId";
            tableOptions.KeyFields.Add(keyField);

            var usersTable = this.Database.CreateTable<TestModelUser>(tableOptions);
            usersTable.Add(this.GetRandomUsersForTesting());

            object[] userKeys = new object[] { usersTable.Data[0].UserId, usersTable.Data[10].UserId, usersTable.Data[99].UserId };

            IDbTransaction trans = this.Database.BeginTransaction();
            try
            {
                usersTable.ExecuteInserts(trans);
                this.Database.CommitTransaction(trans);
            }
            catch
            {
                this.Database.RollbackTransaction(trans);
                this.DisconnectDatabase();
                throw;
            }
            usersTable.ClearData();


            usersTable.Load(new Dictionary<string, List<object>>() {
                {
                    usersTable.KeyFields.First().Name,
                    new List<object>()
                    {
                        userKeys[0],
                        userKeys[1],
                        userKeys[2]
                    }
                }
            });

            Assert.AreEqual(true, usersTable.Data.Where(usr => Guid.Equals(usr.UserId, (Guid)userKeys[0])).Count() == 1);
            Assert.AreEqual(true, usersTable.Data.Where(usr => Guid.Equals(usr.UserId, (Guid)userKeys[1])).Count() == 1);
            Assert.AreEqual(true, usersTable.Data.Where(usr => Guid.Equals(usr.UserId, (Guid)userKeys[2])).Count() == 1);

            usersTable.Load(new List<object>()
                    {
                        userKeys[0],
                        userKeys[1],
                        userKeys[2]
                    });

            Assert.AreEqual(true, usersTable.Data.Where(usr => Guid.Equals(usr.UserId, (Guid)userKeys[0])).Count() == 1);
            Assert.AreEqual(true, usersTable.Data.Where(usr => Guid.Equals(usr.UserId, (Guid)userKeys[1])).Count() == 1);
            Assert.AreEqual(true, usersTable.Data.Where(usr => Guid.Equals(usr.UserId, (Guid)userKeys[2])).Count() == 1);
            this.DisconnectDatabase();
        }

        protected void TestLoadBenchMark()
        {
            this.Initialize();
            this.ReCreateSchema();

            var tableOptions = this.Database.CreateTableOptions();
            tableOptions.TableName = "TestModelUser";
            tableOptions.KeyFields = new List<IOrpheusTableKeyField>();
            var keyField = this.Database.CreateTableKeyField();
            keyField.Name = "UserId";
            tableOptions.KeyFields.Add(keyField);

            var usersTable = this.Database.CreateTable<TestModelUser>(tableOptions);
            var usersData = this.GetRandomUsersForTesting(500);
            usersTable.Add(usersData);

            object[] userKeys = new object[] { usersTable.Data[0].UserId, usersTable.Data[10].UserId, usersTable.Data[99].UserId };

            IDbTransaction trans = this.Database.BeginTransaction();
            try
            {
                var insertStopWatch = this.CreateAndStartStopWatch(DateTime.Now.ToString() + " Running TestLoadBenchMark - Inserting " + usersData.Count.ToString() + " records");
                usersTable.ExecuteInserts(trans);
                this.StopAndLogWatch(insertStopWatch);
                this.Database.CommitTransaction(trans);
            }
            catch
            {
                this.Database.RollbackTransaction(trans);
                this.DisconnectDatabase();
                throw;
            }
            usersTable.ClearData();

            var loadStopWatch = this.CreateAndStartStopWatch(DateTime.Now.ToString() + " Running TestLoadBenchMark - Loading " + usersData.Count.ToString() + " records, one at a time");
            usersData.ForEach(usr =>
            {
                usersTable.Load(new List<object>() { usr.UserId });
            });
            this.StopAndLogWatch(loadStopWatch);

            var loadAllStopWatch = this.CreateAndStartStopWatch(DateTime.Now.ToString() + " Running TestLoadBenchMark - Loading all " + usersData.Count.ToString() + " records");
            usersTable.Load();
            this.StopAndLogWatch(loadAllStopWatch);
            Trace.Close();
            this.DisconnectDatabase();
        }

        protected void TestUserDefinedSQL()
        {
            this.Initialize();
            this.ReCreateSchema();

            var usersTable = this.Database.CreateTable<TestModelUser>();
            var usersData = this.GetRandomUsersForTesting(500);
            usersTable.Add(usersData);
            usersTable.Save();

            var users = this.Database.SQL<TestModelUser>("select * from TestModelUser where email ='admin@test.com'");
            foreach(var usr in users)
            {
                Assert.AreEqual("admin@test.com", usr.Email);
            }
            this.DisconnectDatabase();
        }
    }
}
