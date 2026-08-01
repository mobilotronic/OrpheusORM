using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OrpheusTests.MySQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.MySQLServerTests)]
    public class MySQLServerAsyncTableTests : AsyncTableTests
    {
        [TestMethod]
        public async Task MySQLServerTestAsyncInsertLoadRoundTrip() => await this.TestAsyncInsertLoadRoundTrip();

        [TestMethod]
        public async Task MySQLServerTestAsyncLoadBySQL() => await this.TestAsyncLoadBySQL();

        [TestMethod]
        public async Task MySQLServerTestAsyncLoadByCommand() => await this.TestAsyncLoadByCommand();

        [TestMethod]
        public async Task MySQLServerTestAsyncUpdatePersists() => await this.TestAsyncUpdatePersists();

        [TestMethod]
        public async Task MySQLServerTestAsyncDeleteRemovesRow() => await this.TestAsyncDeleteRemovesRow();

        [TestMethod]
        public async Task MySQLServerTestAsyncLoadCancellation() => await this.TestAsyncLoadCancellation();

        [TestMethod]
        public async Task MySQLServerTestAsyncSaveCancellation() => await this.TestAsyncSaveCancellation();

        public MySQLServerAsyncTableTests()
        {
            this.DatabaseEngine = DbEngine.dbMySQL;
        }
    }
}
