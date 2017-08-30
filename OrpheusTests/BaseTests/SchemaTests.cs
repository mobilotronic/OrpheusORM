using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusInterfaces;
using OrpheusCore;

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
                Assert.AreEqual(true, this.Database.DDLHelper.SchemaObjectExists(shemaObj.SQLName));
            }
        }

        public void DropTestSchema()
        {
            this.Initialize();
            var schema = this.CreateSchema();
            schema.Drop();
            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj.SQLName));
            }
        }

        public void DropCreateSchema()
        {
            this.Initialize();
            var schema = this.CreateSchema();
            schema.Drop();
            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(false, this.Database.DDLHelper.SchemaObjectExists(shemaObj.SQLName));
            }

            schema.Execute();

            foreach (ISchemaObject shemaObj in schema.SchemaObjects)
            {
                Assert.AreEqual(true, this.Database.DDLHelper.SchemaObjectExists(shemaObj.SQLName));
            }
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
