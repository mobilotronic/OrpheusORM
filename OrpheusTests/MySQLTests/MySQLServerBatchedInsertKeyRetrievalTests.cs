using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.MySQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.MySQLServerTests)]
    public class MySQLServerBatchedInsertKeyRetrievalTests : BatchedInsertKeyRetrievalTests
    {
        [TestMethod]
        public void MySQLServerTestBatchedInsertWithKeyRetrieval()
        {
            this.TestBatchedInsertWithKeyRetrieval();
        }

        public MySQLServerBatchedInsertKeyRetrievalTests()
        {
            this.DatabaseEngine = DbEngine.dbMySQL;
        }
    }
}
