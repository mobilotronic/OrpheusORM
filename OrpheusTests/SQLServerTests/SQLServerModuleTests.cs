using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    public class SQLServerModuleTests : ModuleTests
    {
        [TestMethod]
        public void SQLServerTestModuleSingleTable()
        {
            this.TestModuleSingleTable();
        }

        [TestMethod]
        public void SQLServerTestModuleMasterDetailTables()
        {
            this.TestModuleMasterDetailTables();
        }

        [TestMethod]
        public void SQLServerTestModuleDefinition()
        {
            this.TestModuleDefinition();
        }

        [TestMethod]
        public void SQLServerTestModuleDefinitionLoadSave()
        {
            this.TestModuleDefinitionLoadSave();
        }

        [TestMethod]
        public void SQLServerTestMasterDetailTenLevelsDeep()
        {
            this.TestMasterDetailTenLevelsDeep();
        }

        [TestMethod]
        public void SQLServerTestSaveBinaryData()
        {
            this.TestSaveBinaryData();
        }

        [TestMethod]
        public void SQLServerTestModuleLoadSpecificKeyValues()
        {
            this.TestModuleLoadSpecificKeyValues();
        }

        public SQLServerModuleTests()
        {
            this.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
