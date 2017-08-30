using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    public class SQLServerSchemaTests : SchemaTests
    {
        [TestMethod]
        public void SQLServerCreateTestSchema()
        {
            this.CreateTestSchema();
        }

        [TestMethod]
        public void SQLServerDropTestSchema()
        {
            this.DropTestSchema();
        }

        [TestMethod]
        public void SQLServerDropCreateSchema()
        {
            this.DropCreateSchema();
        }

        public SQLServerSchemaTests()
        {
            TestDatabase.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
