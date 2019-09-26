using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusInterfaces.Core;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    [TestCategory(BaseTestClass.SQLServerTests)]
    public class SQLServerSchemaTests : SchemaTests
    {
        //[TestMethod]
        //public void SQLServerCreateTestSchema()
        //{
        //    this.CreateTestSchema();
        //}

        //[TestMethod]
        //public void SQLServerDropTestSchema()
        //{
        //    this.DropTestSchema();
        //}

        [TestMethod]
        public void SQLServerDropCreateSchema()
        {
            this.DropCreateSchema();
        }

        [TestMethod]
        public void SQLServerCreateDynamicSchema()
        {
            this.CreateDynamicSchema();
        }

        public void SQLServerEnableContainedDatabase()
        {
            this.DropCreateSchema();
            this.Database.DDLHelperAs<ISQLServerDDLHelper>().EnableContainedDatabases(true);
            this.Database.DDLHelperAs<ISQLServerDDLHelper>().SetDatabaseContainment("PARTIAL");
        }

        public SQLServerSchemaTests()
        {
            this.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
