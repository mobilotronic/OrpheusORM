using OrpheusCore;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrpheusTests
{
    public class BaseTestClass
    {
        public IOrpheusDatabase Database
        {
            get
            {
                return TestDatabase.DB;
            }
        }

        /// <summary>
        /// Initializes Orpheus configuration (Unity) and creates and connects the Database object.
        /// </summary>
        public void Initialize()
        {
            this.Database.Connect(TestDatabase.ConnectionString);
        }

        public TestSchema CreateSchema()
        {
            return new TestSchema(this.Database, "Test Schema", 1.1, Guid.NewGuid());
        }

        public void ReCreateSchema()
        {
            var schema = this.CreateSchema();
            schema.Drop();
            schema.Execute();
        }
    }
}
