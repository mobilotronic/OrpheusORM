using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusCore.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace OrpheusTests.ConfigurationTests
{
    [TestClass]
    [TestCategory(BaseTestClass.ConfigurationTests)]
    public class OrpheusConfigurationTests : BaseTestClass
    {
        //public void SaveFullServicesConfiguration()
        //{
        //    var configuration = new OrpheusConfiguration();
        //    configuration.Services = new List<ServiceProviderItem>()
        //    {
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IDbConnection).AssemblyQualifiedName,
        //            Implementation = typeof(SqlConnection).AssemblyQualifiedName,
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusDatabase).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusDatabase).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(ISchemaField).AssemblyQualifiedName,
        //            Implementation = typeof(SchemaField).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(ISchema).AssemblyQualifiedName,
        //            Implementation = typeof(Schema).AssemblyQualifiedName,
        //            ConstructorParameters = new List<string>()
        //            {
        //                "db","description","version","id"
        //            }
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(ISchemaTable).AssemblyQualifiedName,
        //            Implementation = typeof(SchemaObjectTable).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(ISchemaView).AssemblyQualifiedName,
        //            Implementation = typeof(SchemaObjectView).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IPrimaryKeySchemaConstraint).AssemblyQualifiedName,
        //            Implementation = typeof(PrimaryKeySchemaConstraint).AssemblyQualifiedName,
        //            ConstructorParameters = new List<string>{ "schemaObject"}
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IForeignKeySchemaConstraint).AssemblyQualifiedName,
        //            Implementation = typeof(ForeignKeySchemaConstraint).AssemblyQualifiedName,
        //            ConstructorParameters = new List<string>{ "schemaObject"}
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IUniqueKeySchemaConstraint).AssemblyQualifiedName,
        //            Implementation = typeof(UniqueKeySchemaConstraint).AssemblyQualifiedName,
        //            ConstructorParameters = new List<string>{ "schemaObject"}
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusModule).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusModule).AssemblyQualifiedName,
        //            ConstructorParameters = new List<string>{ "database","definition"}
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusModuleDefinition).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusModuleDefinition).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusTable<>).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusTable<>).AssemblyQualifiedName,
        //            ConstructorParameters = new List<string>{ "options"}
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusTableKeyField).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusTableKeyField).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusTableOptions).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusTableOptions).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusDDLHelper).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusSQLServerDDLHelper).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(ILogger).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusLogger.OrpheusFileLogger).AssemblyQualifiedName,
        //            ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton
        //        }
        //    };
        //    ConfigurationManager.InitializeConfiguration(configuration);
        //    ConfigurationManager.SaveConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServer.config");
        //}

        //[TestMethod]
        //public void SaveSQLServerServicesConfiguration()
        //{
        //    var configuration = new OrpheusConfiguration();
        //    configuration.Services = new List<ServiceProviderItem>()
        //    {
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IDbConnection).AssemblyQualifiedName,
        //            Implementation = typeof(SqlConnection).AssemblyQualifiedName,
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusDatabase).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusDatabase).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusDDLHelper).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusSQLServerDDLHelper).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(ILogger).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusLogger.OrpheusFileLogger).AssemblyQualifiedName,
        //            ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton
        //        }
        //    };
        //    configuration.Logging = new LoggingConfiguration()
        //    {
        //        Level = "Error",
        //        MaxFileSize = 1
        //    };
        //    ConfigurationManager.InitializeConfiguration(configuration);
        //    ConfigurationManager.SaveConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServer.config");
        //}

        //[TestMethod]
        //public void SaveMySQLServerServicesConfiguration()
        //{
        //    var configuration = new OrpheusConfiguration();
        //    configuration.Services = new List<ServiceProviderItem>()
        //    {
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IDbConnection).AssemblyQualifiedName,
        //            Implementation = typeof(MySql.Data.MySqlClient.MySqlConnection).AssemblyQualifiedName,
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusDatabase).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusDatabase).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(IOrpheusDDLHelper).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusSQLServerDDLHelper).AssemblyQualifiedName
        //        },
        //        new ServiceProviderItem()
        //        {
        //            Service = typeof(ILogger).AssemblyQualifiedName,
        //            Implementation = typeof(OrpheusLogger.OrpheusFileLogger).AssemblyQualifiedName,
        //            ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton
        //        }
        //    };
        //    ConfigurationManager.InitializeConfiguration(configuration);
        //    ConfigurationManager.SaveConfiguration(this.CurrentDirectory + @"\" + "OrpheusMySQLServer.config");
        //}

        //[TestMethod]
        //public void LoadServicesXMLConfiguration()
        //{
        //    ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServer.config"));
        //    var database = OrpheusServiceProvider.Resolve<IOrpheusDatabase>();
        //    this.DatabaseEngine = DbEngine.dbSQLServer;
        //    database.Connect(this.ConnectionString);
        //    var module = database.CreateModule();
        //    var table = database.CreateTable<TestModelItem>();
        //}

        //[TestMethod]
        //public void LoadServicesConfiguration()
        //{
        //    ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json"));
        //    var database = OrpheusServiceProvider.Resolve<IOrpheusDatabase>();
        //    this.DatabaseEngine = DbEngine.dbSQLServer;
        //    database.Connect(this.ConnectionString);
        //    var module = database.CreateModule();
        //    var table = database.CreateTable<TestModelItem>();
        //}

        [TestMethod]
        public async Task ReloadConfigurationAsync()
        {
            this.InitializeConfiguration();

            var errorId = Guid.NewGuid().ToString();
            var traceId = Guid.NewGuid().ToString();
            var logFileContentsName = $"c:\\temp\\orpheus\\nlog-all-{DateTime.Now.ToString("yyyy-MM-dd")}.log";

            var logger = ConfigurationManager.LoggerFactory.CreateLogger<OrpheusConfigurationTests>();
            logger.LogError($"ErrorId {errorId} test Error log entry");

            //loading the log file content.

            using var fileStream = new StreamReader(logFileContentsName, new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.ReadWrite});
            string logFileContents = fileStream.ReadToEnd();

            //making sure that the error is logged.
            Assert.AreEqual(true, logFileContents.Contains(errorId));

            //loading the NLog XML configuration file and updating the logging level.
            XmlDocument doc = new XmlDocument();
            doc.Load(this.CurrentDirectory + @"\" + "nlog.config");
            XmlNodeList nlogRules = doc.DocumentElement.SelectNodes("//*[name()='nlog']/*[name()='rules']/*[name()='logger']");
            foreach(XmlNode nlogRule in nlogRules)
            {
                XmlAttribute logLevel = nlogRule.Attributes["minlevel"];
                if(logLevel != null)
                {
                    logLevel.Value = "Trace";
                }
            }
            doc.Save(this.CurrentDirectory + @"\" + "nlog.config");

            //give NLog some time to reload its configuration.
            await Task.Delay(3000);
            logger.LogTrace($"TraceId {traceId} test Trace log entry");

            //reload log file content.
            using var fileStream2 = new StreamReader(logFileContentsName, new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.ReadWrite });
            logFileContents = fileStream2.ReadToEnd();

            //making sure that the trace is logged.
            Assert.AreEqual(true, logFileContents.Contains(traceId));
        }
    }
}
