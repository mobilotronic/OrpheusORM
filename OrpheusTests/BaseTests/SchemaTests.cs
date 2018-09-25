using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusInterfaces.Schema;
using OrpheusTestModels;
using System;
using System.Text.RegularExpressions;

namespace OrpheusTests
{
    public class BaseSchemaTestClass : BaseTestClass
    {
 
        public string TrimWhiteSpace(string value)
        {
            return Regex.Replace(value, @"\s+", "", RegexOptions.IgnoreCase);
        }
    }


    public class SchemaTests : BaseSchemaTestClass
    {

        public void CreateTestSchema()
        {
            this.Initialize();
            var schema = this.CreateSchema();
            schema.Execute();
            
            foreach(ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(true, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }
        }

        public void DropTestSchema()
        {
            this.Initialize();
            var schema = this.CreateSchema();
            schema.Drop();
            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }
        }

        public void DropCreateSchema()
        {
            this.Initialize();
            var schema = this.CreateSchema();
            schema.Drop();
            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }

            schema.Execute();

            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(true, this.Database.DDLHelper.SchemaObjectExists(shemaObj));
            }
        }

        public void CreateDynamicSchema()
        {
            this.Initialize();
            var schema = this.Database.CreateSchema(Guid.NewGuid(),"Dynamic schema", 1);
            schema.AddSchemaTable("TestModelDynamic", null, new TestDynamicModel1());
            schema.Drop();
            schema.Execute();

            var table1 = this.Database.CreateTable<TestDynamicModel1>("TestModelDynamic");

            table1.Add(new TestDynamicModel1() {
                Id = Guid.NewGuid(),
                Code = "Dynamic",
                Description ="Model",
                Price = 10.34,
                Index = 10
            });
            table1.Save();
            table1.Delete(table1.Data[0]);
            table1.Save();

            schema.SchemaObjects.Clear();
            schema.AddSchemaTable("TestModelDynamic", null, new TestDynamicModel2());
            schema.Execute();

            var table2 = this.Database.CreateTable<TestDynamicModel2>("TestModelDynamic");

            table2.Add(new TestDynamicModel2()
            {
                Id = Guid.NewGuid(),
                Description = "Model",
            });
            table2.Save();
            table2.Delete(table2.Data[0]);
            table2.Save();

            schema.SchemaObjects.Clear();
            schema.AddSchemaTable("TestModelDynamic", null, new TestDynamicModel3());
            schema.Execute();

            var table3 = this.Database.CreateTable<TestDynamicModel3>("TestModelDynamic");

            table3.Add(new TestDynamicModel3()
            {
                Id = Guid.NewGuid(),
                Description = "Model",
                Index = "a",
                Price = 10
            });
            table3.Save();
        }

        //[TestMethod]
        //public void SaveToFileSchema()
        //{
        //    this.Initialize();
        //    var schema = this.CreateSchema();
        //    var fileName = @"c:\schema.xml";
        //    schema.SaveToFile(fileName);
        //    Assert.AreEqual(true, File.Exists(fileName));
        //}
    }
}
