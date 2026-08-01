using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    [TestCategory(BaseTestClass.SQLServerTests)]
    public class SQLServerBatchedInsertKeyRetrievalTests : BatchedInsertKeyRetrievalTests
    {
        [TestMethod]
        public void SQLServerTestBatchedInsertWithKeyRetrieval()
        {
            this.TestBatchedInsertWithKeyRetrieval();
        }

        public SQLServerBatchedInsertKeyRetrievalTests()
        {
            this.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
