using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    public class SQLServerSchemaNameTests : BaseTestClass
    {
        [TestMethod]
        public void SQLNameSchemaDropCreate()
        {
            this.Initialize();
            var schemaName = "TestSchema";

            var schemaCommand = this.Database.CreateCommand();



            var schema = new TestSchema(this.Database, "Named Test Schema", 1.1, Guid.Parse("331503CC-10EC-4639-8BB7-4A4609BDF7EB"), schemaName);
            schema.Drop();
            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }

            this.Database.DDLHelperAs<ISQLServerDDLHelper>().DropSchema(schemaName);

            this.Database.DDLHelperAs<ISQLServerDDLHelper>().CreateSchema(schemaName);

            schema.Execute();

            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(true, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }
        }

        [TestMethod]
        public void SQLNamedSchemaDrop()
        {
            this.Initialize();
            var schemaName = "TestSchema";

            var schemaCommand = this.Database.CreateCommand();



            var schema = new TestSchema(this.Database, "Named Test Schema", 1.1, Guid.Parse("331503CC-10EC-4639-8BB7-4A4609BDF7EB"), schemaName);
            schema.Drop();
            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }

        }

        public SQLServerSchemaNameTests()
        {
            TestDatabase.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
