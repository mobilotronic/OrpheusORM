using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusSQLDDLHelper;
using OrpheusTestModels;
using System;
using System.Linq;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    [TestCategory(BaseTestClass.SQLServerTests)]
    public class SQLServerConnectionPoolingTests : ConnectionPoolingTests
    {
        [TestMethod]
        public void SQLServerTestConnectDisconnectConnectCycle() => this.TestConnectDisconnectConnectCycle();

        [TestMethod]
        public void SQLServerTestConnectionStringReflectsPoolingConfig()
        {
            this.Initialize();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            config.Pooling = true;
            config.MinPoolSize = 5;
            config.MaxPoolSize = 42;
            config.ConnectionIdleTimeout = 123;

            var factory = new SqlConnectionFactory { ConnectionConfiguration = config };
            using var connection = (SqlConnection)factory.CreateConnection();
            var builder = new SqlConnectionStringBuilder(connection.ConnectionString);

            Assert.IsTrue(builder.Pooling, "Pooling should reflect the configured value.");
            Assert.AreEqual(5, builder.MinPoolSize, "MinPoolSize should reflect the configured value.");
            Assert.AreEqual(42, builder.MaxPoolSize, "MaxPoolSize should reflect the configured value.");
            Assert.AreEqual(123, builder.LoadBalanceTimeout, "ConnectionIdleTimeout should map to LoadBalanceTimeout.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Verifies CreateAdministrativeConnection() (used by the DDL helper's "master connection"
        /// for database existence checks / CREATE DATABASE) targets SQL Server's system database and
        /// reflects the configured pooling settings — before this session's connection consolidation,
        /// the administrative connection was built independently of SqlConnectionFactory and silently
        /// ignored Pooling/MinPoolSize/MaxPoolSize/ConnectionIdleTimeout entirely.
        /// </summary>
        [TestMethod]
        public void SQLServerTestAdministrativeConnectionTargetsSystemDatabaseAndReflectsPooling()
        {
            this.Initialize();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            config.Pooling = true;
            config.MinPoolSize = 5;
            config.MaxPoolSize = 42;
            config.ConnectionIdleTimeout = 123;

            var factory = new SqlConnectionFactory { ConnectionConfiguration = config };
            using var connection = (SqlConnection)factory.CreateAdministrativeConnection();
            var builder = new SqlConnectionStringBuilder(connection.ConnectionString);

            Assert.AreEqual("master", builder.InitialCatalog, "Administrative connection should target SQL Server's system database.");
            Assert.IsTrue(builder.Pooling, "Pooling should reflect the configured value.");
            Assert.AreEqual(5, builder.MinPoolSize, "MinPoolSize should reflect the configured value.");
            Assert.AreEqual(42, builder.MaxPoolSize, "MaxPoolSize should reflect the configured value.");
            Assert.AreEqual(123, builder.LoadBalanceTimeout, "ConnectionIdleTimeout should map to LoadBalanceTimeout.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Verifies the non-DI OrpheusSQLServerDatabase.CreateDatabase(config) factory produces a
        /// fully working, pooling-aware database on its own — no IServiceCollection/ServiceManager
        /// setup required beyond the connection configuration itself.
        /// </summary>
        [TestMethod]
        public void SQLServerTestStaticFactoryCreatesWorkingDatabase()
        {
            this.Initialize();
            this.ReCreateSchema();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            var factoryDb = OrpheusSQLServerDatabase.CreateDatabase(config);
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

        public SQLServerConnectionPoolingTests()
        {
            this.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
