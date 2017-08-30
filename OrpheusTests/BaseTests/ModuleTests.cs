using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusCore;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OrpheusTests
{
    public class ModuleTests : BaseTestClass
    {
        protected void TestModuleSingleTable()
        {
            this.Initialize();
            this.ReCreateSchema();

            var module = OrpheusIocContainer.Resolve<IOrpheusModule>(new ResolverOverride[] {
                new ParameterOverride("database",this.Database)
            });
            var tableOptions = OrpheusIocContainer.Resolve<IOrpheusTableOptions>();
            tableOptions.TableName = "TestModelTransactor";
            var transactorsTable = this.Database.CreateTable<TestModelTransactor>(tableOptions);

            module.MainTable = transactorsTable;

            transactorsTable.Add(new TestModelTransactor() {
                TransactorId = Guid.NewGuid(),
                Type = TestModelTransactorType.ttCustomer,
                Address = "Test Address",
                Code = "001",
                Description="Test customer",
                Email="test@customer.com"
            });

            module.Save();
            module.Load();

            Assert.IsTrue(module.GetTable<TestModelTransactor>(0).Data.First().Type == TestModelTransactorType.ttCustomer,"Expected to have a customer type transactor.");
        }

        protected void TestModuleMasterDetailTables()
        {
            this.Initialize();
            this.ReCreateSchema();

            var module = OrpheusIocContainer.Resolve<IOrpheusModule>(new ResolverOverride[] {
                new ParameterOverride("database",this.Database)
            });

            module.ReferenceTables.Add(this.Database.CreateTable<TestModelTransactor>("TestModelTransactor"));
            module.ReferenceTables.Add(this.Database.CreateTable<TestModelItem>("TestModelItem"));


            module.Tables.Add(this.Database.CreateTable<TestModelOrder>("TestModelOrder"));
            var order = module.GetTable<TestModelOrder>("TestModelOrder");


            var orderLineOptions = OrpheusIocContainer.Resolve<IOrpheusTableOptions>();
            orderLineOptions.TableName = "TestModelOrderLine";
            orderLineOptions.MasterTableKeyFields = new List<IOrpheusTableKeyField>();
            orderLineOptions.Database = this.Database;


            var orderMasterKeyField = OrpheusIocContainer.Resolve<IOrpheusTableKeyField>();
            orderMasterKeyField.Name = "OrderId";
            orderLineOptions.MasterTableKeyFields.Add(orderMasterKeyField);
            orderLineOptions.MasterTableName = "TestModelOrder";
            module.Tables.Add(this.Database.CreateTable<TestModelOrderLine>(orderLineOptions));
            

            var transactors = module.GetReferenceTable<TestModelTransactor>("TestModelTransactor");
            var items = module.GetReferenceTable<TestModelItem>("TestModelItem");

            
            var orderLines = module.GetTable<TestModelOrderLine>("TestModelOrderLine");
            orderLines.MasterTable = order;

            //populating auxiliary data.
            transactors.Add(TestDatabase.GetTransactors());
            items.Add(TestDatabase.GetItems());
            using(var tr = this.Database.BeginTransaction())
            {
                transactors.ExecuteInserts(tr);
                items.ExecuteInserts(tr);
                try
                {
                    tr.Commit();
                }
                catch
                {
                    throw;
                }
            }

            transactors.Load();
            items.Load();

            order.Add(new TestModelOrder() {
                OrderId = Guid.NewGuid(),
                OrderDateTime = DateTime.Now,
                TransactorId = transactors.Data.First().TransactorId
            });

            orderLines.Add(new TestModelOrderLine() {
                ItemId = items.Data.First().ItemId,
                OrderLineId = Guid.NewGuid(),
                Price = 5,
                Quantity = 10,
                TotalPrice = 50
            });

            module.Save();
        }

        protected void TestModuleDefinition()
        {
            this.Initialize();
            this.ReCreateSchema();
            var moduleDefinition = OrpheusIocContainer.Resolve<IOrpheusModuleDefinition>();
            moduleDefinition.MainTableOptions = moduleDefinition.CreateTableOptions("TestModelOrder",typeof(TestModelOrder));

            moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TestModelTransactor", typeof(TestModelTransactor)));
            moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TestModelItem", typeof(TestModelItem)));

            var detailTableOptions = moduleDefinition.CreateTableOptions("TestModelOrderLine", typeof(TestModelOrderLine));
            detailTableOptions.MasterTableName = "TestModelOrder";
            detailTableOptions.AddMasterKeyField("OrderId");
            moduleDefinition.DetailTableOptions.Add(detailTableOptions);
            var module = this.Database.CreateModule(moduleDefinition);

            var transactors = module.GetReferenceTable<TestModelTransactor>("TestModelTransactor");
            var items = module.GetReferenceTable<TestModelItem>("TestModelItem");
            var orderLines = module.GetTable<TestModelOrderLine>("TestModelOrderLine");
            var order = module.GetTable<TestModelOrder>("TestModelOrder");

            //populating auxiliary data.
            transactors.Add(TestDatabase.GetTransactors());
            items.Add(TestDatabase.GetItems());
            using (var tr = this.Database.BeginTransaction())
            {
                transactors.ExecuteInserts(tr);
                items.ExecuteInserts(tr);
                try
                {
                    tr.Commit();
                }
                catch
                {
                    throw;
                }
            }

            transactors.Load();
            items.Load();

            order.Add(new TestModelOrder()
            {
                OrderId = Guid.NewGuid(),
                OrderDateTime = DateTime.Now,
                TransactorId = transactors.Data.First().TransactorId
            });

            orderLines.Add(new TestModelOrderLine()
            {
                ItemId = items.Data.First().ItemId,
                OrderLineId = Guid.NewGuid(),
                Price = 5,
                Quantity = 10,
                TotalPrice = 50
            });

            module.Save();
        }

        protected void TestModuleDefinitionLoadSave()
        {
            this.Initialize();
            var filename = Directory.GetCurrentDirectory() + @"\orpheusModuleDefinition.xml";

            var moduleDefinition = OrpheusIocContainer.Resolve<IOrpheusModuleDefinition>();
            moduleDefinition.MainTableOptions = moduleDefinition.CreateTableOptions("TEST_MODEL_ORDERS", typeof(TestModelOrder));

            moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TEST_MODEL_TRANSACTORS", typeof(TestModelTransactor)));
            moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TEST_MODEL_ITEMS", typeof(TestModelItem)));

            var detailTableOptions = moduleDefinition.CreateTableOptions("TEST_MODEL_ORDER_LINES", typeof(TestModelOrderLine));
            detailTableOptions.MasterTableName = "TEST_MODEL_ORDERS";
            detailTableOptions.AddMasterKeyField("OrderId");
            moduleDefinition.DetailTableOptions.Add(detailTableOptions);
            moduleDefinition.SaveTo(filename);

            var newModuleDefinition = OrpheusIocContainer.Resolve<IOrpheusModuleDefinition>();
            newModuleDefinition.LoadFrom(filename);

            var newDetailTableCount = (from detailTable in moduleDefinition.DetailTableOptions
                        from newDetailTable in newModuleDefinition.DetailTableOptions
                        where detailTable.TableName == newDetailTable.TableName
                        select newDetailTable).Count();

            var newReferenceCount = (from referenceTable in moduleDefinition.ReferenceTableOptions
                                       from newReferenceTable in newModuleDefinition.ReferenceTableOptions
                                     where referenceTable.TableName == newReferenceTable.TableName
                                       select newReferenceTable).Count();


            Assert.IsTrue(moduleDefinition.DetailTableOptions.Count() == newDetailTableCount,"New and old detail tables definition must be equal.");
            Assert.IsTrue(moduleDefinition.ReferenceTableOptions.Count() == newReferenceCount, "New and old reference tables definition must be equal.");
            Assert.IsTrue(moduleDefinition.Name == newModuleDefinition.Name, "New and old name must be equal.");
            Assert.IsTrue(moduleDefinition.MainTableOptions.TableName == newModuleDefinition.MainTableOptions.TableName, "New and old main table name must be equal.");
        }

        protected void TestModuleDefinitionLoadSaveFromDB()
        {
            this.Initialize();
            var filename = Directory.GetCurrentDirectory() + @"\orpheusModuleDefinition.xml";

            var moduleDefinition = this.Database.CreateModuleDefinition();
            moduleDefinition.MainTableOptions = moduleDefinition.CreateTableOptions("TEST_MODEL_ORDERS", typeof(TestModelOrder));
            moduleDefinition.Name = "TEST_MODULE";

            moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TEST_MODEL_TRANSACTORS", typeof(TestModelTransactor)));
            moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TEST_MODEL_ITEMS", typeof(TestModelItem)));

            var detailTableOptions = moduleDefinition.CreateTableOptions("TEST_MODEL_ORDER_LINES", typeof(TestModelOrderLine));
            detailTableOptions.MasterTableName = "TEST_MODEL_ORDERS";
            detailTableOptions.AddMasterKeyField("OrderId");
            moduleDefinition.DetailTableOptions.Add(detailTableOptions);
            moduleDefinition.SaveToDB();

            var newModuleDefinition = this.Database.CreateModuleDefinition();
            newModuleDefinition.LoadFromDB(moduleDefinition.Name);

            var newDetailTableCount = (from detailTable in moduleDefinition.DetailTableOptions
                                       from newDetailTable in newModuleDefinition.DetailTableOptions
                                       where detailTable.TableName == newDetailTable.TableName
                                       select newDetailTable).Count();

            var newReferenceCount = (from referenceTable in moduleDefinition.ReferenceTableOptions
                                     from newReferenceTable in newModuleDefinition.ReferenceTableOptions
                                     where referenceTable.TableName == newReferenceTable.TableName
                                     select newReferenceTable).Count();

            Assert.IsTrue(moduleDefinition.DetailTableOptions.Count() == newDetailTableCount, "New and old detail tables definition must be equal.");
            Assert.IsTrue(moduleDefinition.ReferenceTableOptions.Count() == newReferenceCount, "New and old reference tables definition must be equal.");
            Assert.IsTrue(moduleDefinition.Name == newModuleDefinition.Name, "New and old name must be equal.");
            Assert.IsTrue(moduleDefinition.MainTableOptions.TableName == newModuleDefinition.MainTableOptions.TableName, "New and old main table name must be equal.");

        }

        protected void TestMasterDetailTenLevelsDeep()
        {
            this.Initialize();
            this.ReCreateSchema();
            var moduleDefinition = this.Database.CreateModuleDefinition();

            moduleDefinition.MainTableOptions = moduleDefinition.CreateTableOptions("TestMasterModel", typeof(TestMasterModel));

            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel1", typeof(TestDetailModelLevel1)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel2", typeof(TestDetailModelLevel2)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel3", typeof(TestDetailModelLevel3)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel4", typeof(TestDetailModelLevel4)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel5", typeof(TestDetailModelLevel5)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel6", typeof(TestDetailModelLevel6)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel7", typeof(TestDetailModelLevel7)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel8", typeof(TestDetailModelLevel8)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel9", typeof(TestDetailModelLevel9)));
            moduleDefinition.DetailTableOptions.Add(moduleDefinition.CreateTableOptions("TestDetailModelLevel10", typeof(TestDetailModelLevel10)));

            moduleDefinition.MainTableOptions.MasterTableName = "TestMasterModel";
            moduleDefinition.Name = "TEN_LEVEL_MODULE";

            moduleDefinition.DetailTableOptions[0].MasterTableName = "TestMasterModel";
            for (var i = 2; i <= 10; i++)
            {
                moduleDefinition.DetailTableOptions.Where(t => t.TableName == "TestDetailModelLevel" + i.ToString()).First().MasterTableName = "TestDetailModelLevel" + (i - 1).ToString();
                //detailTableOptions.MasterTableName =;
            }

            var module = this.Database.CreateModule(moduleDefinition);

            module.GetTable<TestMasterModel>("TestMasterModel").Add(new TestMasterModel() { });
            module.GetTable<TestDetailModelLevel1>("TestDetailModelLevel1").Add(new TestDetailModelLevel1() { });
            module.GetTable<TestDetailModelLevel2>("TestDetailModelLevel2").Add(new TestDetailModelLevel2() { });
            module.GetTable<TestDetailModelLevel3>("TestDetailModelLevel3").Add(new TestDetailModelLevel3() { });
            module.GetTable<TestDetailModelLevel4>("TestDetailModelLevel4").Add(new TestDetailModelLevel4() { });
            module.GetTable<TestDetailModelLevel5>("TestDetailModelLevel5").Add(new TestDetailModelLevel5() { });
            module.GetTable<TestDetailModelLevel6>("TestDetailModelLevel6").Add(new TestDetailModelLevel6() { });
            module.GetTable<TestDetailModelLevel7>("TestDetailModelLevel7").Add(new TestDetailModelLevel7() { });
            module.GetTable<TestDetailModelLevel8>("TestDetailModelLevel8").Add(new TestDetailModelLevel8() { });
            module.GetTable<TestDetailModelLevel9>("TestDetailModelLevel9").Add(new TestDetailModelLevel9() { });
            module.GetTable<TestDetailModelLevel10>("TestDetailModelLevel10").Add(new TestDetailModelLevel10() { });

            module.Save();

            //loading the module, clears the record from memory and loads it from the DB.
            module.Load();

            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel10>("TestDetailModelLevel10").Data[0].MasterKey == module.GetTable<TestDetailModelLevel9>("TestDetailModelLevel9").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel9>("TestDetailModelLevel9").Data[0].MasterKey == module.GetTable<TestDetailModelLevel8>("TestDetailModelLevel8").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel8>("TestDetailModelLevel8").Data[0].MasterKey == module.GetTable<TestDetailModelLevel7>("TestDetailModelLevel7").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel7>("TestDetailModelLevel7").Data[0].MasterKey == module.GetTable<TestDetailModelLevel6>("TestDetailModelLevel6").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel6>("TestDetailModelLevel6").Data[0].MasterKey == module.GetTable<TestDetailModelLevel5>("TestDetailModelLevel5").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel5>("TestDetailModelLevel5").Data[0].MasterKey == module.GetTable<TestDetailModelLevel4>("TestDetailModelLevel4").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel4>("TestDetailModelLevel4").Data[0].MasterKey == module.GetTable<TestDetailModelLevel3>("TestDetailModelLevel3").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel3>("TestDetailModelLevel3").Data[0].MasterKey == module.GetTable<TestDetailModelLevel2>("TestDetailModelLevel2").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel2>("TestDetailModelLevel2").Data[0].MasterKey == module.GetTable<TestDetailModelLevel1>("TestDetailModelLevel1").Data[0].Key);
            Assert.AreEqual(true, module.GetTable<TestDetailModelLevel1>("TestDetailModelLevel1").Data[0].MasterKey == module.GetTable<TestMasterModel>("TestMasterModel").Data[0].Key);
        }

        protected void TestSaveBinaryData()
        {
            this.Initialize();
            this.ReCreateSchema();
            var moduleDefinition = this.Database.CreateModuleDefinition();

            moduleDefinition.MainTableOptions = moduleDefinition.CreateTableOptions("TestBinaryDataModel", typeof(TestBinaryDataModel));

            moduleDefinition.MainTableOptions.MasterTableName = "TestBinaryDataModel";
            moduleDefinition.Name = "Binary test module";

            var module = this.Database.CreateModule(moduleDefinition);

            string testString = "This is a test byte array.";

            byte[] byteArrayToSave = Encoding.UTF8.GetBytes(testString);
            module.GetTable<TestBinaryDataModel>().Add(new TestBinaryDataModel() {
                Image = byteArrayToSave
            });

            module.Save();
            //loading the module, clears the record from memory and loads it from the DB.
            module.Load();

            string savedString = Encoding.UTF8.GetString(module.GetTable<TestBinaryDataModel>().Data[0].Image);

            Assert.AreEqual(true, testString == savedString);
        }

        protected void TestModuleLoadSpecificKeyValues()
        {
            this.Initialize();
            this.ReCreateSchema();

            var module = OrpheusIocContainer.Resolve<IOrpheusModule>(new ResolverOverride[] {
                new ParameterOverride("database",this.Database)
            });

            module.ReferenceTables.Add(this.Database.CreateTable<TestModelTransactor>("TestModelTransactor"));
            module.ReferenceTables.Add(this.Database.CreateTable<TestModelItem>("TestModelItem"));


            module.Tables.Add(this.Database.CreateTable<TestModelOrder>("TestModelOrder"));
            var order = module.GetTable<TestModelOrder>("TestModelOrder");


            var orderLineOptions = OrpheusIocContainer.Resolve<IOrpheusTableOptions>();
            orderLineOptions.TableName = "TestModelOrderLine";
            orderLineOptions.MasterTableKeyFields = new List<IOrpheusTableKeyField>();
            orderLineOptions.Database = this.Database;


            var orderMasterKeyField = OrpheusIocContainer.Resolve<IOrpheusTableKeyField>();
            orderMasterKeyField.Name = "OrderId";
            orderLineOptions.MasterTableKeyFields.Add(orderMasterKeyField);
            orderLineOptions.MasterTableName = "TestModelOrder";
            module.Tables.Add(this.Database.CreateTable<TestModelOrderLine>(orderLineOptions));
            module.MainTable = order;


            var transactors = module.GetReferenceTable<TestModelTransactor>("TestModelTransactor");
            var items = module.GetReferenceTable<TestModelItem>("TestModelItem");


            var orderLines = module.GetTable<TestModelOrderLine>("TestModelOrderLine");
            orderLines.MasterTable = order;

            //populating auxiliary data.
            transactors.Add(TestDatabase.GetTransactors(100));
            items.Add(TestDatabase.GetItems(100));
            using (var tr = this.Database.BeginTransaction())
            {
                transactors.ExecuteInserts(tr);
                items.ExecuteInserts(tr);
                try
                {
                    tr.Commit();
                }
                catch
                {
                    throw;
                }
            }

            transactors.Load();
            items.Load();
            var firstOrderId = Guid.NewGuid();
            order.Add(new TestModelOrder()
            {
                OrderId = firstOrderId,
                OrderDateTime = DateTime.Now,
                TransactorId = transactors.Data.First().TransactorId
            });

            orderLines.Add(new TestModelOrderLine()
            {
                ItemId = items.Data.First().ItemId,
                OrderLineId = Guid.NewGuid(),
                Price = 1,
                Quantity = 1,
                TotalPrice = 1
            });

            order.Add(new TestModelOrder()
            {
                OrderId = Guid.NewGuid(),
                OrderDateTime = DateTime.Now,
                TransactorId = transactors.Data.First().TransactorId
            });

            orderLines.Add(new TestModelOrderLine()
            {
                ItemId = items.Data.First().ItemId,
                OrderLineId = Guid.NewGuid(),
                Price = 2,
                Quantity = 2,
                TotalPrice = 2,
                OrderId = order.Data.Last().OrderId
            });

            module.Save();

            module.Load(new List<object>() { firstOrderId });

            Assert.AreEqual(true, order.Data.Count() == 1);
            Assert.AreEqual(true, Guid.Equals(order.Data.First().OrderId, firstOrderId));
            Assert.AreEqual(true, Guid.Equals(orderLines.Data.First().OrderId, firstOrderId));
        }
    }
}
