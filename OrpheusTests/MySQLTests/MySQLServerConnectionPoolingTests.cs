using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using OrpheusMySQLDDLHelper;
using OrpheusTestModels;
using System;
using System.Linq;

namespace OrpheusTests.MySQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.MySQLServerTests)]
    public class MySQLServerConnectionPoolingTests : ConnectionPoolingTests
    {
        [TestMethod]
        public void MySQLServerTestConnectDisconnectConnectCycle() => this.TestConnectDisconnectConnectCycle();

        [TestMethod]
        public void MySQLServerTestConnectionStringReflectsPoolingConfig()
        {
            this.Initialize();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            config.Pooling = true;
            config.MinPoolSize = 5;
            config.MaxPoolSize = 42;
            config.ConnectionIdleTimeout = 123;

            var factory = new MySqlConnectionFactory { ConnectionConfiguration = config };
            using var connection = (MySqlConnection)factory.CreateConnection();
            var builder = new MySqlConnectionStringBuilder(connection.ConnectionString);

            Assert.IsTrue(builder.Pooling, "Pooling should reflect the configured value.");
            Assert.AreEqual(5u, builder.MinimumPoolSize, "MinPoolSize should reflect the configured value.");
            Assert.AreEqual(42u, builder.MaximumPoolSize, "MaxPoolSize should reflect the configured value.");
            Assert.AreEqual(123u, builder.ConnectionLifeTime, "ConnectionIdleTimeout should map to ConnectionLifeTime.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Verifies CreateAdministrativeConnection() targets MySQL's "sys" administrative database
        /// and reflects the configured pooling settings.
        /// </summary>
        [TestMethod]
        public void MySQLServerTestAdministrativeConnectionTargetsSystemDatabaseAndReflectsPooling()
        {
            this.Initialize();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            config.Pooling = true;
            config.MinPoolSize = 5;
            config.MaxPoolSize = 42;
            config.ConnectionIdleTimeout = 123;

            var factory = new MySqlConnectionFactory { ConnectionConfiguration = config };
            using var connection = (MySqlConnection)factory.CreateAdministrativeConnection();
            var builder = new MySqlConnectionStringBuilder(connection.ConnectionString);

            Assert.AreEqual("sys", builder.Database, "Administrative connection should target MySQL's system database.");
            Assert.IsTrue(builder.Pooling, "Pooling should reflect the configured value.");
            Assert.AreEqual(5u, builder.MinimumPoolSize, "MinPoolSize should reflect the configured value.");
            Assert.AreEqual(42u, builder.MaximumPoolSize, "MaxPoolSize should reflect the configured value.");
            Assert.AreEqual(123u, builder.ConnectionLifeTime, "ConnectionIdleTimeout should map to ConnectionLifeTime.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Verifies the non-DI OrpheusMySQLServerDatabase.CreateDatabase(config) factory produces a
        /// fully working, pooling-aware database on its own.
        /// </summary>
        [TestMethod]
        public void MySQLServerTestStaticFactoryCreatesWorkingDatabase()
        {
            this.Initialize();
            this.ReCreateSchema();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            var factoryDb = OrpheusMySQLServerDatabase.CreateDatabase(config);
            factoryDb.Connect();
            try
            {
                Assert.IsTrue(factoryDb.Connected, "Database created via the static factory should be connected.");

                var table = factoryDb.CreateTable<TestModelTransactor>();
                table.Add(new TestModelTransactor
                {
                    TransactorId = Guid.NewGuid(),
                    Code = "FACTORY1",
                    Description = "Created via static factory",
                    Address = "Addr",
                    Email = "factory@test.com",
                    Type = TestModelTransactorType.ttCustomer
                });
                table.Save();

                var reloadTable = factoryDb.CreateTable<TestModelTransactor>();
                reloadTable.Load();
                Assert.IsTrue(reloadTable.Data.Any(t => t.Code == "FACTORY1"), "Row inserted via the factory-created database should be readable back.");
            }
            finally
            {
                factoryDb.Disconnect();
            }

            this.DisconnectDatabase();
        }

        public MySQLServerConnectionPoolingTests()
        {
            this.DatabaseEngine = DbEngine.dbMySQL;
        }
    }
}
