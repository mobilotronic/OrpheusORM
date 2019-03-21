using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.MySQLTests
{
    [TestClass]
    [TestCategory(BaseTestClass.MySQLServerTests)]
    public class MySQLServerModuleTests : ModuleTests
    {
        [TestMethod]
        public void MySQLServerTestModuleSingleTable()
        {
            this.TestModuleSingleTable();
        }

        [TestMethod]
        public void MySQLServerTestModuleMasterDetailTables()
        {
            this.TestModuleMasterDetailTables();
        }

        [TestMethod]
        public void MySQLServerTestModuleDefinition()
        {
            this.TestModuleDefinition();
        }

        [TestMethod]
        public void MySQLServerTestModuleDefinitionLoadSave()
        {
            this.TestModuleDefinitionLoadSave();
        }

        [TestMethod]
        public void MySQLServerTestMasterDetailTenLevelsDeep()
        {
            this.TestMasterDetailTenLevelsDeep();
        }

        [TestMethod]
        public void MySQLServerTestSaveBinaryData()
        {
            this.TestSaveBinaryData();
        }

        [TestMethod]
        public void MySQLServerTestModuleLoadSpecificKeyValues()
        {
            this.TestModuleLoadSpecificKeyValues();
        }

        public MySQLServerModuleTests()
        {
            this.DatabaseEngine = DbEngine.dbMySQL;
        }

    }
}
