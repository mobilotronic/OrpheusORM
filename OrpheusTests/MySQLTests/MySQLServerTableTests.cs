using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.MySQLTests
{
    [TestClass]
    public class MySQLServerTableTests : TableTests
    {
        [TestMethod]
        public void MySQLServerTestCreateCommandRandomData()
        {
            this.TestCreateCommandRandomData();
        }

        [TestMethod]
        public void MySQLServerTestUpdateCommandRandomData()
        {
            this.TestUpdateCommandRandomData();
        }

        [TestMethod]
        public void MySQLServerTestDeleteCommandRandomData()
        {
            this.TestDeleteCommandRandomData();
        }

        [TestMethod]
        public void MySQLServerTestPrimaryKeyInfer()
        {
            this.TestPrimaryKeyInfer();
        }

        [TestMethod]
        public void MySQLServerTestKeyValueAutoGenerate()
        {
            this.TestKeyValueAutoGenerate();
        }

        [TestMethod]
        public void MySQLServerTestLoadSpecificKeyValues()
        {
            this.TestLoadSpecificKeyValues();
        }

        [TestMethod]
        public void MySQLServerTestUserDefinedSQL()
        {
            this.TestUserDefinedSQL();
        }

        public MySQLServerTableTests()
        {
            TestDatabase.DatabaseEngine = DbEngine.dbMySQL;
        }
    }
}
