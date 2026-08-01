using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.MySQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.MySQLServerTests)]
    public class MySQLServerBatchingTests : BatchingTests
    {
        [TestMethod]
        public void MySQLServerTestInsertExactlyOneBatch() => this.TestInsertExactlyOneBatch();

        [TestMethod]
        public void MySQLServerTestInsertSpansTwoBatches() => this.TestInsertSpansTwoBatches();

        [TestMethod]
        public void MySQLServerTestUpdateExactlyOneBatch() => this.TestUpdateExactlyOneBatch();

        [TestMethod]
        public void MySQLServerTestUpdateSpansTwoBatches() => this.TestUpdateSpansTwoBatches();

        [TestMethod]
        public void MySQLServerTestDeleteExactlyOneBatch() => this.TestDeleteExactlyOneBatch();

        [TestMethod]
        public void MySQLServerTestDeleteSpansTwoBatches() => this.TestDeleteSpansTwoBatches();

        [TestMethod]
        public void MySQLServerTestCompositeKeyBatchedInsertUpdateDelete() => this.TestCompositeKeyBatchedInsertUpdateDelete();

        public MySQLServerBatchingTests()
        {
            this.DatabaseEngine = DbEngine.dbMySQL;
        }
    }
}
