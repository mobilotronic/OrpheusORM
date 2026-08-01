using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.PostgreSQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.PostgreSQLTests)]
    public class PostgreSQLBatchingTests : BatchingTests
    {
        [TestMethod]
        public void PostgreSQLTestInsertExactlyOneBatch() => this.TestInsertExactlyOneBatch();

        [TestMethod]
        public void PostgreSQLTestInsertSpansTwoBatches() => this.TestInsertSpansTwoBatches();

        [TestMethod]
        public void PostgreSQLTestUpdateExactlyOneBatch() => this.TestUpdateExactlyOneBatch();

        [TestMethod]
        public void PostgreSQLTestUpdateSpansTwoBatches() => this.TestUpdateSpansTwoBatches();

        [TestMethod]
        public void PostgreSQLTestDeleteExactlyOneBatch() => this.TestDeleteExactlyOneBatch();

        [TestMethod]
        public void PostgreSQLTestDeleteSpansTwoBatches() => this.TestDeleteSpansTwoBatches();

        [TestMethod]
        public void PostgreSQLTestCompositeKeyBatchedInsertUpdateDelete() => this.TestCompositeKeyBatchedInsertUpdateDelete();

        public PostgreSQLBatchingTests()
        {
            this.DatabaseEngine = DbEngine.dbPostgreSQL;
        }
    }
}
