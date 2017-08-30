using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    public class SQLServerTableTests : TableTests
    {
        [TestMethod]
        public void SQLServerTestCreateCommandRandomData()
        {
            this.TestCreateCommandRandomData();
        }

        [TestMethod]
        public void SQLServerTestUpdateCommandRandomData()
        {
            this.TestUpdateCommandRandomData();
        }

        [TestMethod]
        public void SQLServerTestDeleteCommandRandomData()
        {
            this.TestDeleteCommandRandomData();
        }

        [TestMethod]
        public void SQLServerTestPrimaryKeyInfer()
        {
            this.TestPrimaryKeyInfer();
        }

        [TestMethod]
        public void SQLServerTestKeyValueAutoGenerate()
        {
            this.TestKeyValueAutoGenerate();
        }

        [TestMethod]
        public void SQLServerTestLoadSpecificKeyValues()
        {
            this.TestLoadSpecificKeyValues();
        }

        public SQLServerTableTests()
        {
            TestDatabase.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
