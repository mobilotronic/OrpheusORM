using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusTests.SchemaCreationTests
{
    [TestClass]
    public class SchemaCreationTests : BaseTestClass
    {
        [TestMethod]
        public void TestSchemaObjectDependency()
        {
            var schema = this.CreateSchema();

            schema.Schema.SchemaObjects.Clear();

            schema.Schema.AddSchemaTable<TestModelTransactor>();
            schema.Schema.AddSchemaTable<TestModelItem>();

            var orders = schema.Schema.AddSchemaTable<TestModelOrder>();

            orders.AddDependency<TestModelTransactor>();
            orders.AddDependency<TestModelItem>();
        }
    }
}
