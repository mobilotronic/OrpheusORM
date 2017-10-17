using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using OrpheusCore.Configuration;
using OrpheusCore.ServiceProvider;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using OrpheusInterfaces;
using OrpheusCore;
using OrpheusSQLDDLHelper;
using OrpheusCore.SchemaBuilder;
using OrpheusTestModels;

namespace OrpheusTests.ConfigurationTests
{
    [TestClass]
    public class OrpheusConfigurationTests
    {
        public void SaveFullServicesConfiguration()
        {
            var configuration = new OrpheusConfiguration();
            configuration.Services = new List<ServiceProviderItem>()
            {
                new ServiceProviderItem()
                {
                    Service = typeof(IDbConnection).AssemblyQualifiedName,
                    Implementation = typeof(SqlConnection).AssemblyQualifiedName,
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusDatabase).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusDatabase).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(ISchemaField).AssemblyQualifiedName,
                    Implementation = typeof(SchemaField).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(ISchema).AssemblyQualifiedName,
                    Implementation = typeof(Schema).AssemblyQualifiedName,
                    ConstructorParameters = new List<string>()
                    {
                        "db","description","version","id"
                    }
                },
                new ServiceProviderItem()
                {
                    Service = typeof(ISchemaTable).AssemblyQualifiedName,
                    Implementation = typeof(SchemaObjectTable).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(ISchemaView).AssemblyQualifiedName,
                    Implementation = typeof(SchemaObjectView).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IPrimaryKeySchemaConstraint).AssemblyQualifiedName,
                    Implementation = typeof(PrimaryKeySchemaConstraint).AssemblyQualifiedName,
                    ConstructorParameters = new List<string>{ "schemaObject"}
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IForeignKeySchemaConstraint).AssemblyQualifiedName,
                    Implementation = typeof(ForeignKeySchemaConstraint).AssemblyQualifiedName,
                    ConstructorParameters = new List<string>{ "schemaObject"}
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IUniqueKeySchemaConstraint).AssemblyQualifiedName,
                    Implementation = typeof(UniqueKeySchemaConstraint).AssemblyQualifiedName,
                    ConstructorParameters = new List<string>{ "schemaObject"}
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusModule).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusModule).AssemblyQualifiedName,
                    ConstructorParameters = new List<string>{ "database","definition"}
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusModuleDefinition).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusModuleDefinition).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusTable<>).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusTable<>).AssemblyQualifiedName,
                    ConstructorParameters = new List<string>{ "options"}
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusTableKeyField).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusTableKeyField).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusTableOptions).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusTableOptions).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusDDLHelper).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusSQLServerDDLHelper).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(ILogger).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusLogger.OrpheusFileLogger).AssemblyQualifiedName,
                    ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton
                }
            };
            ConfigurationManager.InitializeConfiguration(configuration);
            ConfigurationManager.SaveConfiguration(ConfigurationManager.CurrentDirectory + @"\" + "OrpheusSQLServer.config");
        }

        [TestMethod]
        public void SaveSQLServerServicesConfiguration()
        {
            var configuration = new OrpheusConfiguration();
            configuration.Services = new List<ServiceProviderItem>()
            {
                new ServiceProviderItem()
                {
                    Service = typeof(IDbConnection).AssemblyQualifiedName,
                    Implementation = typeof(SqlConnection).AssemblyQualifiedName,
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusDatabase).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusDatabase).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusDDLHelper).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusSQLServerDDLHelper).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(ILogger).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusLogger.OrpheusFileLogger).AssemblyQualifiedName,
                    ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton
                }
            };
            configuration.Logging = new LoggingConfiguration()
            {
                Level = "Error",
                MaxFileSize = 1
            };
            ConfigurationManager.InitializeConfiguration(configuration);
            ConfigurationManager.SaveConfiguration(ConfigurationManager.CurrentDirectory + @"\" + "OrpheusSQLServer.config");
        }

        [TestMethod]
        public void SaveMySQLServerServicesConfiguration()
        {
            var configuration = new OrpheusConfiguration();
            configuration.Services = new List<ServiceProviderItem>()
            {
                new ServiceProviderItem()
                {
                    Service = typeof(IDbConnection).AssemblyQualifiedName,
                    Implementation = typeof(MySql.Data.MySqlClient.MySqlConnection).AssemblyQualifiedName,
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusDatabase).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusDatabase).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(IOrpheusDDLHelper).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusSQLServerDDLHelper).AssemblyQualifiedName
                },
                new ServiceProviderItem()
                {
                    Service = typeof(ILogger).AssemblyQualifiedName,
                    Implementation = typeof(OrpheusLogger.OrpheusFileLogger).AssemblyQualifiedName,
                    ServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton
                }
            };
            ConfigurationManager.InitializeConfiguration(configuration);
            ConfigurationManager.SaveConfiguration(ConfigurationManager.CurrentDirectory + @"\" + "OrpheusMySQLServer.config");
        }

        [TestMethod]
        public void LoadServicesConfiguration()
        {
            ConfigurationManager.InitializeConfiguration(ConfigurationManager.CurrentDirectory + @"\" + "OrpheusSQLServer.config");
            var database = OrpheusServiceProvider.Resolve<IOrpheusDatabase>();
            TestDatabase.DatabaseEngine = DbEngine.dbSQLServer;
            database.Connect(TestDatabase.ConnectionString);
            var module = database.CreateModule();
            var table = database.CreateTable<TestModelItem>();
        }
    }
}
