using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OrpheusTests.PostgreSQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.PostgreSQLTests)]
    public class PostgreSQLAsyncTableTests : AsyncTableTests
    {
        [TestMethod]
        public async Task PostgreSQLTestAsyncInsertLoadRoundTrip() => await this.TestAsyncInsertLoadRoundTrip();

        [TestMethod]
        public async Task PostgreSQLTestAsyncLoadBySQL() => await this.TestAsyncLoadBySQL();

        [TestMethod]
        public async Task PostgreSQLTestAsyncLoadByCommand() => await this.TestAsyncLoadByCommand();

        [TestMethod]
        public async Task PostgreSQLTestAsyncUpdatePersists() => await this.TestAsyncUpdatePersists();

        [TestMethod]
        public async Task PostgreSQLTestAsyncDeleteRemovesRow() => await this.TestAsyncDeleteRemovesRow();

        [TestMethod]
        public async Task PostgreSQLTestAsyncLoadCancellation() => await this.TestAsyncLoadCancellation();

        [TestMethod]
        public async Task PostgreSQLTestAsyncSaveCancellation() => await this.TestAsyncSaveCancellation();

        public PostgreSQLAsyncTableTests()
        {
            this.DatabaseEngine = DbEngine.dbPostgreSQL;
        }
    }
}
