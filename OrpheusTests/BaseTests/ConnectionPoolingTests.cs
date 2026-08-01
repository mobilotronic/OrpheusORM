using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusTestModels;
using System;
using System.Linq;

namespace OrpheusTests
{
    /// <summary>
    /// Verifies the connection-pooling MVP added this session: Connect() leases from
    /// IOrpheusConnectionFactory and Disconnect()/reconnect actually works. The engine-specific
    /// pooling-config/administrative-connection/static-factory checks live in each engine's own
    /// test class (SQLServerConnectionPoolingTests etc.) instead of here, since each engine has a
    /// different connection factory type, connection-string-builder property names, and
    /// administrative database name.
    /// </summary>
    public class ConnectionPoolingTests : BaseTestClass
    {
        protected void TestConnectDisconnectConnectCycle()
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            table.Add(new TestModelTransactor
            {
                TransactorId = Guid.NewGuid(),
                Code = "POOL1",
                Description = "Before disconnect",
                Address = "Addr",
                Email = "pool@test.com",
                Type = TestModelTransactorType.ttCustomer
            });
            table.Save();

            this.DisconnectDatabase();
            Assert.IsFalse(this.Database.Connected, "Database should report disconnected after Disconnect().");

            this.Database.Connect();
            Assert.IsTrue(this.Database.Connected, "Database should report connected after reconnecting.");

            var reloadTable = this.Database.CreateTable<TestModelTransactor>();
            reloadTable.Load();
            Assert.IsTrue(reloadTable.Data.Any(t => t.Code == "POOL1"), "Data inserted before disconnect should still be readable after reconnecting.");

            this.DisconnectDatabase();
        }
    }
}
