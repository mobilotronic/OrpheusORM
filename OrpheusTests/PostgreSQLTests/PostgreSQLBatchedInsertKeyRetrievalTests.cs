using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.PostgreSQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.PostgreSQLTests)]
    public class PostgreSQLBatchedInsertKeyRetrievalTests : BatchedInsertKeyRetrievalTests
    {
        [TestMethod]
        public void PostgreSQLTestBatchedInsertWithKeyRetrieval()
        {
            this.TestBatchedInsertWithKeyRetrieval();
        }

        public PostgreSQLBatchedInsertKeyRetrievalTests()
        {
            this.DatabaseEngine = DbEngine.dbPostgreSQL;
        }
    }
}
