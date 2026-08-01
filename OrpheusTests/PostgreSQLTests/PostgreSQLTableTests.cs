using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.PostgreSQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.PostgreSQLTests)]
    public class PostgreSQLTableTests : TableTests
    {
        [TestMethod]
        public void PostgreSQLTestCreateCommandRandomData()
        {
            this.TestCreateCommandRandomData();
        }

        [TestMethod]
        public void PostgreSQLTestUpdateCommandRandomData()
        {
            this.TestUpdateCommandRandomData();
        }

        [TestMethod]
        public void PostgreSQLTestDeleteCommandRandomData()
        {
            this.TestDeleteCommandRandomData();
        }

        [TestMethod]
        public void PostgreSQLTestPrimaryKeyInfer()
        {
            this.TestPrimaryKeyInfer();
        }

        [TestMethod]
        public void PostgreSQLTestKeyValueAutoGenerate()
        {
            this.TestKeyValueAutoGenerate();
        }

        [TestMethod]
        public void PostgreSQLTestLoadSpecificKeyValues()
        {
            this.TestLoadSpecificKeyValues();
        }

        [TestMethod]
        public void PostgreSQLTestLoadBenchMark()
        {
            this.TestLoadBenchMark();
        }

        [TestMethod]
        public void PostgreSQLTestUserDefinedSQL()
        {
            this.TestUserDefinedSQL();
        }

        public PostgreSQLTableTests()
        {
            this.DatabaseEngine = DbEngine.dbPostgreSQL;
        }
    }
}
