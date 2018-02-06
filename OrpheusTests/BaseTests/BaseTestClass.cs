using Microsoft.Extensions.Logging;
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
        private ILogger logger;
        private string schemaId = "6E8653BE-CB9C-4855-8909-2846AFBB72E1";
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

        public TestSchema CreateSchema(string name = null)
        {
            return new TestSchema(this.Database,"Test Schema", 1.1, Guid.Parse(this.schemaId), name);
        }

        public ILogger Logger
        {
            get
            {
                if (this.logger == null)
                {
                    this.logger = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
                }
                return this.logger;
            }
        }

        public Stopwatch CreateAndStartStopWatch(string message)
        {
            this.Logger.LogError(message);
            var result = new Stopwatch();
            result.Start();
            return result;
        }

        public Stopwatch CreateAndStartStopWatch(string message,object[] args)
        {
            return this.CreateAndStartStopWatch(String.Format(message, args));
        }

        public void StopAndLogWatch(Stopwatch stopwatch)
        {
            this.Logger.LogError("Elapsed milliseconds {0}", stopwatch.ElapsedMilliseconds);
        }

        public void LogBenchMarkInfo(string message)
        {
            this.Logger.LogError(message);
        }

        public void ReCreateSchema()
        {
            var schema = this.CreateSchema();
            schema.Drop();
            schema.Execute();
        }
    }
}
