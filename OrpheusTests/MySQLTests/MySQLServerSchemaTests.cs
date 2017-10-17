using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrpheusTests.MySQLTests
{
    [TestClass]
    public class MySQLServerSchemaTests : SchemaTests
    {
        [TestMethod]
        public void MySQLServerCreateTestSchema()
        {
            this.CreateTestSchema();
        }

        [TestMethod]
        public void MySQLServerDropTestSchema()
        {
            this.DropTestSchema();
        }

        [TestMethod]
        public void MySQLServerDropCreateSchema()
        {
            this.DropCreateSchema();
        }

        [TestMethod]
        public void MySQLServerCreateDynamicSchema()
        {
            this.CreateDynamicSchema();
        }

        public MySQLServerSchemaTests()
        {
            TestDatabase.DatabaseEngine = DbEngine.dbMySQL;
        }
    }
}
