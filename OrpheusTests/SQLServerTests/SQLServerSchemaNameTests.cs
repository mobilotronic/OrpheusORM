using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusCore.SchemaBuilder;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Reflection;

namespace OrpheusTests.SQLServerTests
{
    [TestClass]
    [TestCategory(BaseTestClass.SQLServerTests)]
    public class SQLServerSchemaNameTests : BaseTestClass
    {
        [TestMethod]
        public void SQLNameSchemaDropCreate()
        {
            this.Initialize();
            var schemaName = "TestSchema";

            var schemaCommand = this.Database.CreateCommand();



            var schema = new TestSchema(this.Database, "Named Test Schema", 1.1, Guid.Parse("331503CC-10EC-4639-8BB7-4A4609BDF7EB"), schemaName);
            if (this.Database.DDLHelperAs<ISQLServerDDLHelper>().SchemaExists(schemaName))
            {
                schema.Drop();
                foreach (ISchemaObject shemaObj in schema.SchemaObjects)
                {
                    Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
                }
            }

            this.Database.DDLHelperAs<ISQLServerDDLHelper>().DropSchema(schemaName);

            this.Database.DDLHelperAs<ISQLServerDDLHelper>().CreateSchema(schemaName);

            foreach(SchemaDataObject schemaObject in schema.SchemaObjects)
            {
                var propInfo = schemaObject.GetType().BaseType.GetField("modelHelper", BindingFlags.NonPublic | BindingFlags.Instance);
                if (propInfo != null)
                {
                    var modelHelper = (IOrpheusModelHelper)propInfo.GetValue(schemaObject);
                    foreach(var fk in modelHelper.ForeignKeys)
                    {
                        fk.Value.SchemaName = schemaName;
                    }
                    if(modelHelper.ForeignKeys.Count > 0)
                        modelHelper.CreateSchemaFields(schemaObject);
                }
            }

            schema.Execute();

            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(true, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }
        }

        //[TestMethod]
        //public void SQLNamedSchemaDrop()
        //{
        //    this.Initialize();
        //    var schemaName = "TestSchema";

        //    var schemaCommand = this.Database.CreateCommand();



        //    var schema = new TestSchema(this.Database, "Named Test Schema", 1.1, Guid.Parse("331503CC-10EC-4639-8BB7-4A4609BDF7EB"), schemaName);
        //    if (this.Database.DDLHelperAs<ISQLServerDDLHelper>().SchemaExists(schemaName))
        //    {
        //        schema.Drop();
        //        foreach (ISchemaObject shemaObj in schema.SchemaObjects)
        //        {
        //            Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
        //        }
        //    }
        //}

        public SQLServerSchemaNameTests()
        {
            this.DatabaseEngine = DbEngine.dbSQLServer;
        }
    }
}
