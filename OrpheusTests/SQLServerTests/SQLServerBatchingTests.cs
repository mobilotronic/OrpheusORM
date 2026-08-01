using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    [TestCategory(BaseTestClass.SQLServerTests)]
    public class SQLServerBatchingTests : BatchingTests
    {
        [TestMethod]
        public void SQLServerTestInsertExactlyOneBatch() => this.TestInsertExactlyOneBatch();

        [TestMethod]
        public void SQLServerTestInsertSpansTwoBatches() => this.TestInsertSpansTwoBatches();

        [TestMethod]
        public void SQLServerTestUpdateExactlyOneBatch() => this.TestUpdateExactlyOneBatch();

        [TestMethod]
        public void SQLServerTestUpdateSpansTwoBatches() => this.TestUpdateSpansTwoBatches();

        [TestMethod]
        public void SQLServerTestDeleteExactlyOneBatch() => this.TestDeleteExactlyOneBatch();

        [TestMethod]
        public void SQLServerTestDeleteSpansTwoBatches() => this.TestDeleteSpansTwoBatches();

        [TestMethod]
        public void SQLServerTestCompositeKeyBatchedInsertUpdateDelete() => this.TestCompositeKeyBatchedInsertUpdateDelete();

        public SQLServerBatchingTests()
        {
            this.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
