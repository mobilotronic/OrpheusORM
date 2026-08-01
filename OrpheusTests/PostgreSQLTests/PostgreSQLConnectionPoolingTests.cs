using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OrpheusPostgreSQLDDLHelper;
using OrpheusTestModels;
using System;
using System.Linq;

namespace OrpheusTests.PostgreSQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.PostgreSQLTests)]
    public class PostgreSQLConnectionPoolingTests : ConnectionPoolingTests
    {
        [TestMethod]
        public void PostgreSQLTestConnectDisconnectConnectCycle() => this.TestConnectDisconnectConnectCycle();

        [TestMethod]
        public void PostgreSQLTestConnectionStringReflectsPoolingConfig()
        {
            this.Initialize();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            config.Pooling = true;
            config.MinPoolSize = 5;
            config.MaxPoolSize = 42;
            config.ConnectionIdleTimeout = 123;

            var factory = new NpgsqlConnectionFactory { ConnectionConfiguration = config };
            using var connection = (NpgsqlConnection)factory.CreateConnection();
            var builder = new NpgsqlConnectionStringBuilder(connection.ConnectionString);

            Assert.IsTrue(builder.Pooling, "Pooling should reflect the configured value.");
            Assert.AreEqual(5, builder.MinPoolSize, "MinPoolSize should reflect the configured value.");
            Assert.AreEqual(42, builder.MaxPoolSize, "MaxPoolSize should reflect the configured value.");
            Assert.AreEqual(123, builder.ConnectionIdleLifetime, "ConnectionIdleTimeout should map to ConnectionIdleLifetime.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Verifies CreateAdministrativeConnection() targets PostgreSQL's "postgres" administrative
        /// database and reflects the configured pooling settings.
        /// </summary>
        [TestMethod]
        public void PostgreSQLTestAdministrativeConnectionTargetsSystemDatabaseAndReflectsPooling()
        {
            this.Initialize();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            config.Pooling = true;
            config.MinPoolSize = 5;
            config.MaxPoolSize = 42;
            config.ConnectionIdleTimeout = 123;

            var factory = new NpgsqlConnectionFactory { ConnectionConfiguration = config };
            using var connection = (NpgsqlConnection)factory.CreateAdministrativeConnection();
            var builder = new NpgsqlConnectionStringBuilder(connection.ConnectionString);

            Assert.AreEqual("postgres", builder.Database, "Administrative connection should target PostgreSQL's system database.");
            Assert.IsTrue(builder.Pooling, "Pooling should reflect the configured value.");
            Assert.AreEqual(5, builder.MinPoolSize, "MinPoolSize should reflect the configured value.");
            Assert.AreEqual(42, builder.MaxPoolSize, "MaxPoolSize should reflect the configured value.");
            Assert.AreEqual(123, builder.ConnectionIdleLifetime, "ConnectionIdleTimeout should map to ConnectionIdleLifetime.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Verifies the non-DI OrpheusPostgreSQLServerDatabase.CreateDatabase(config) factory
        /// produces a fully working, pooling-aware database on its own.
        /// </summary>
        [TestMethod]
        public void PostgreSQLTestStaticFactoryCreatesWorkingDatabase()
        {
            this.Initialize();
            this.ReCreateSchema();

            var config = this.Database.DatabaseConnectionConfiguration.Clone();
            var factoryDb = OrpheusPostgreSQLServerDatabase.CreateDatabase(config);
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

        public PostgreSQLConnectionPoolingTests()
        {
            this.DatabaseEngine = DbEngine.dbPostgreSQL;
        }
    }
}
