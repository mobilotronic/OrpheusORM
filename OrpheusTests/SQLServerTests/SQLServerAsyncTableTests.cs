using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    [TestCategory(BaseTestClass.SQLServerTests)]
    public class SQLServerAsyncTableTests : AsyncTableTests
    {
        [TestMethod]
        public async Task SQLServerTestAsyncInsertLoadRoundTrip() => await this.TestAsyncInsertLoadRoundTrip();

        [TestMethod]
        public async Task SQLServerTestAsyncLoadBySQL() => await this.TestAsyncLoadBySQL();

        [TestMethod]
        public async Task SQLServerTestAsyncLoadByCommand() => await this.TestAsyncLoadByCommand();

        [TestMethod]
        public async Task SQLServerTestAsyncUpdatePersists() => await this.TestAsyncUpdatePersists();

        [TestMethod]
        public async Task SQLServerTestAsyncDeleteRemovesRow() => await this.TestAsyncDeleteRemovesRow();

        [TestMethod]
        public async Task SQLServerTestAsyncLoadCancellation() => await this.TestAsyncLoadCancellation();

        [TestMethod]
        public async Task SQLServerTestAsyncSaveCancellation() => await this.TestAsyncSaveCancellation();

        public SQLServerAsyncTableTests()
        {
            this.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
