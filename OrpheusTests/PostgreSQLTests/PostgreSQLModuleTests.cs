using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.PostgreSQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.PostgreSQLTests)]
    public class PostgreSQLModuleTests : ModuleTests
    {
        [TestMethod]
        public void PostgreSQLTestModuleSingleTable()
        {
            this.TestModuleSingleTable();
        }

        [TestMethod]
        public void PostgreSQLTestModuleMasterDetailTables()
        {
            this.TestModuleMasterDetailTables();
        }

        [TestMethod]
        public void PostgreSQLTestModuleDefinition()
        {
            this.TestModuleDefinition();
        }

        [TestMethod]
        public void PostgreSQLTestModuleDefinitionLoadSave()
        {
            this.TestModuleDefinitionLoadSave();
        }

        [TestMethod]
        public void PostgreSQLTestMasterDetailTenLevelsDeep()
        {
            this.TestMasterDetailTenLevelsDeep();
        }

        [TestMethod]
        public void PostgreSQLTestSaveBinaryData()
        {
            this.TestSaveBinaryData();
        }

        [TestMethod]
        public void PostgreSQLTestModuleLoadSpecificKeyValues()
        {
            this.TestModuleLoadSpecificKeyValues();
        }

        public PostgreSQLModuleTests()
        {
            this.DatabaseEngine = DbEngine.dbPostgreSQL;
        }
    }
}
