using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OrpheusCore.Configuration;
using OrpheusCore.ServiceProvider;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            var configurationFileName = this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json";
            ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(configurationFileName));

            var errorId = Guid.NewGuid().ToString();
            var traceId = Guid.NewGuid().ToString();

            var logger = (OrpheusLogger.OrpheusFileLogger)OrpheusServiceProvider.Resolve<ILogger>();
            logger.LogError($"ErrorId {errorId} test Error log entry");

            var logFileContents = File.ReadAllText(logger.CurrentFileName);

            //making sure that the error is logged.
            Assert.AreEqual(true, logFileContents.Contains(errorId));

            OrpheusConfiguration orpheusConfiguration = JsonConvert.DeserializeObject<OrpheusConfiguration>(File.ReadAllText(configurationFileName));

            orpheusConfiguration.Logging.Level = "Trace";

            File.WriteAllText(configurationFileName, JsonConvert.SerializeObject(orpheusConfiguration));

            await Task.Delay(1000);

            logger.LogTrace($"TraceId {traceId} test Trace log entry");

            logFileContents = File.ReadAllText(logger.CurrentFileName);

            //making sure that the trace is logged.
            Assert.AreEqual(true, logFileContents.Contains(traceId));
        }

        [TestMethod]
        public async Task ReloadConfigurationFromFileAsync()
        {
            var configurationFileName = this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json";
            ConfigurationManager.InitializeConfiguration(configurationFileName);

            var errorId = Guid.NewGuid().ToString();
            var traceId = Guid.NewGuid().ToString();

            var logger = (OrpheusLogger.OrpheusFileLogger)OrpheusServiceProvider.Resolve<ILogger>();
            logger.LogError($"ErrorId {errorId} test Error log entry");

            var logFileContents = File.ReadAllText(logger.CurrentFileName);

            //making sure that the error is logged.
            Assert.AreEqual(true, logFileContents.Contains(errorId));

            OrpheusConfiguration orpheusConfiguration = JsonConvert.DeserializeObject<OrpheusConfiguration>(File.ReadAllText(configurationFileName));

            orpheusConfiguration.Logging.Level = "Trace";

            File.WriteAllText(configurationFileName, JsonConvert.SerializeObject(orpheusConfiguration));

            // the default delay for the reload callback in JsonFileProvider is 250ms, so 1 sec would be more than enough in order to reload the latest configuration settings.
            await Task.Delay(1000);

            logger.LogTrace($"TraceId {traceId} test Trace log entry");

            logFileContents = File.ReadAllText(logger.CurrentFileName);

            //making sure that the trace is logged.
            Assert.AreEqual(true, logFileContents.Contains(traceId));
        }

        [TestMethod]
        public async Task ReloadConfigurationFromFileLoggerTransientAsync()
        {
            var configurationFileName = this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json";
            //loading configuration and changing the default service lifetime from singleton to transient.
            OrpheusConfiguration orpheusConfiguration = JsonConvert.DeserializeObject<OrpheusConfiguration>(File.ReadAllText(configurationFileName));
            orpheusConfiguration.Services.First(s => s.Service.Contains("ILogger")).ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient;
            File.WriteAllText(configurationFileName, JsonConvert.SerializeObject(orpheusConfiguration));

            ConfigurationManager.InitializeConfiguration(configurationFileName);

            var errorId = Guid.NewGuid().ToString();
            var traceId = Guid.NewGuid().ToString();

            var logger = (OrpheusLogger.OrpheusFileLogger)OrpheusServiceProvider.Resolve<ILogger>();
            logger.LogError($"ErrorId {errorId} test Error log entry");

            var logFileContents = File.ReadAllText(logger.CurrentFileName);

            //making sure that the error is logged.
            Assert.AreEqual(true, logFileContents.Contains(errorId));

            orpheusConfiguration = JsonConvert.DeserializeObject<OrpheusConfiguration>(File.ReadAllText(configurationFileName));

            orpheusConfiguration.Logging.Level = "Trace";

            File.WriteAllText(configurationFileName, JsonConvert.SerializeObject(orpheusConfiguration));

            // the default delay for the reload callback in JsonFileProvider is 250ms, so 1 sec would be more than enough in order to reload the latest configuration settings.
            await Task.Delay(1000);

            logger = (OrpheusLogger.OrpheusFileLogger)OrpheusServiceProvider.Resolve<ILogger>();
            logger.LogTrace($"TraceId {traceId} test Trace log entry");

            logFileContents = File.ReadAllText(logger.CurrentFileName);

            //making sure that the trace is logged.
            Assert.AreEqual(true, logFileContents.Contains(traceId));
        }
    }
}
