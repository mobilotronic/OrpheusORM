﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NLog;
using NLog.Extensions.Logging;
using OrpheusCore;
using OrpheusCore.Configuration;
using OrpheusCore.Configuration.Models;
using OrpheusInterfaces.Core;
using OrpheusMySQLDDLHelper;
using OrpheusSQLDDLHelper;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OrpheusTests
{

    internal class TestData
    {
        public static string[] UserNames = new string[] { "Admin", "Root", "User1", "User2", "User3" };
        public static string[] PasswordHashes = new string[] { "asd1df!@#", "1#$#*(*&", "!@#54$%", "hash", "%$^%(()123" };
        public static string[] PasswordSalts = new string[] { "fdsfa@#$)", "SAD#$FD$", "#$$%!%$)_+", "salt", "FDsfasdf*(*&$#%" };
        public static string[] Emails = new string[] { "admin@test.com", "sales@test.com", "info@test.com", "support@test.com", "webadmin@test.com" };
    }

    public enum DbEngine
    {
        dbSQLServer,
        dbMySQL
    }

    public class BaseTestClass
    {
        #region private declarations
        private Microsoft.Extensions.Logging.ILogger logger;
        private string schemaId = "6E8653BE-CB9C-4855-8909-2846AFBB72E1";
        private IConfiguration configuration;
        private IOrpheusDatabase db;
        private string fileName;
        private byte[] currentAppsettingsHash = new byte[0];
        private string _assemblyDirectory = null;

        private string assemblyDirectory
        {
            get
            {
                if (_assemblyDirectory == null)
                {
                    string codeBase = Assembly.GetExecutingAssembly().Location;
                    //Console.WriteLine($"Assembly path is: {codeBase}");
                    _assemblyDirectory = Path.GetDirectoryName(codeBase);
                }

                return _assemblyDirectory;
            }
        }

        private IConfiguration createConfiguration(string configurationFile)
        {
            if (this.configuration == null)
            {
                this.fileName = configurationFile;
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.SetBasePath(Path.GetDirectoryName(configurationFile));
                configurationBuilder.AddJsonFile(configurationFile, optional: false, reloadOnChange: true);
                this.configuration = configurationBuilder.Build();
            }
            return this.configuration;
        }

        #endregion

        #region public declarations
        public const string SQLServerTests = "SQLServer";
        public const string MySQLServerTests = "MySQLServerTests";
        public const string LoggerTests = "LoggerTests";
        public const string ConfigurationTests = "ConfigurationTests";
        public const string ConfigurationFileName = "OrpheusConfig.json";

        /// <summary>
        /// Initializes Orpheus configuration (Unity) and creates and connects the Database object.
        /// </summary>
        public void Initialize()
        {
            this.Database.Connect();
        }

        public void DisconnectDatabase()
        {
            if (this.db != null)
                this.db.Disconnect();
        }

        public TestSchema CreateSchema(string name = null)
        {
            return new TestSchema(this.Database, "Test Schema", 1.1, Guid.Parse(this.schemaId), name);
        }



        public void InitializeConfiguration(string configurationFileName = null)
        {
            //we only need to initialize once.
            if (this.configuration == null)
            {
                LogManager.Setup().LoadConfigurationFromFile($"{this.assemblyDirectory}/nlog.config");
                var logger = LogManager.GetCurrentClassLogger();
                try
                {
                    if (configurationFileName == null)
                        configurationFileName = $"{this.assemblyDirectory}/{ConfigurationFileName}";
                    //Console.WriteLine($"Configuration file is: {configurationFileName}");
                    IServiceCollection serviceCollection = new ServiceCollection();
                    this.configuration = this.createConfiguration($"{this.assemblyDirectory}/{ConfigurationFileName}");
                    serviceCollection.AddTransient<IOrpheusDatabase, OrpheusDatabase>();
                    serviceCollection.Configure<OrpheusConfiguration>(this.configuration.GetSection("OrpheusConfiguration"));
                    switch (this.DatabaseEngine)
                    {
                        case DbEngine.dbSQLServer:
                            {
                                serviceCollection.AddTransient<IDbConnection, SqlConnection>();
                                serviceCollection.AddTransient<IOrpheusDDLHelper, OrpheusSQLServerDDLHelper>();
                                //Console.WriteLine($"SQL services configured");
                                break;
                            }
                        case DbEngine.dbMySQL:
                            {
                                serviceCollection.AddTransient<IDbConnection, MySqlConnection>();
                                serviceCollection.AddTransient<IOrpheusDDLHelper, OrpheusMySQLServerDDLHelper>();
                                //Console.WriteLine($"MySQL services configured");
                                break;
                            }
                    }
                    serviceCollection.AddLogging((builder) =>
                    {
                        builder.ClearProviders();
                        //setting the MEL minimum level to trace, will essentially cancel whatever logging settings might present in the appsettings.json file
                        //the logging level would be controlled solely from the NLog configuration file.
                        builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                        builder.AddNLog(configuration);
                    });
                    serviceCollection.AddOrpheusServices();
                    this.configuration.InitializeOrpheusConfiguration();
                    var serviceProvider = serviceCollection.BuildServiceProvider();
                    ServiceManager.ServiceProvider = serviceProvider;
                    //Console.WriteLine($"Configuration initialized");
                }
                catch (Exception e)
                {
                    logger.Log(NLog.LogLevel.Error, e);
                }
            }
        }

        public IConfiguration Configuration => this.configuration;

        public DbEngine DatabaseEngine { get; set; }

        public IOrpheusDatabase Database
        {
            get
            {
                if (this.db == null)
                {
                    this.InitializeConfiguration();
                    string databaseConnectionName = this.DatabaseEngine == DbEngine.dbSQLServer ? "SQLServer" : "MySQL";
                    var config = new OrpheusConfiguration();
                    this.configuration.GetSection("OrpheusConfiguration").Bind(config);
                    this.db = ServiceManager.Resolve<IOrpheusDatabase>();
                    this.db.DatabaseConnectionConfiguration = config.DatabaseConnections.FirstOrDefault(c => string.Equals(c.ConfigurationName, databaseConnectionName));
                }
                return this.db;
            }
        }

        public string CurrentDirectory => this.assemblyDirectory;

        #endregion

        #region schema related
        public void ReCreateSchema()
        {
            var schema = this.CreateSchema();
            schema.Drop();
            schema.Execute();
        }
        #endregion

        #region logging 
        public Microsoft.Extensions.Logging.ILogger Logger
        {
            get
            {
                if (this.logger == null)
                {
                    this.logger = ServiceManager.CreateLogger<BaseTestClass>();
                }
                return this.logger;
            }
        }

        public Stopwatch CreateAndStartStopWatch(string message)
        {
            this.Logger.LogInformation(message);
            var result = new Stopwatch();
            result.Start();
            return result;
        }

        public Stopwatch CreateAndStartStopWatch(string message, object[] args)
        {
            return this.CreateAndStartStopWatch(String.Format(message, args));
        }

        public void StopAndLogWatch(Stopwatch stopwatch)
        {
            this.Logger.LogInformation("Elapsed milliseconds {0}", stopwatch.ElapsedMilliseconds);
        }

        public void LogBenchMarkInfo(string message)
        {
            this.Logger.LogInformation(message);
        }
        #endregion

        #region demo data
        public List<TestModelUser> GetRandomUsersForTesting(int count = 1000)
        {
            var result = new List<TestModelUser>();
            Random rnd = new Random();
            for (var i = 0; i <= count - 1; i++)
            {
                var rndIdx = rnd.Next(0, 5);
                result.Add(new TestModelUser()
                {
                    UserId = Guid.NewGuid(),
                    UserName = TestData.UserNames[rndIdx] + (i.ToString()),
                    PasswordHash = TestData.PasswordHashes[rndIdx],
                    PasswordSalt = TestData.PasswordSalts[rndIdx],
                    Email = TestData.Emails[rndIdx],
                    Active = 1,
                    UserProfileId = TestSchemaConstants.AdminUserProfileId,
                    UserGroupId = TestSchemaConstants.AdminUserGroupId
                });
            }
            return result;
        }

        public List<TestModelTransactor> GetTransactors(int count = 1000, TestModelTransactorType transactorType = TestModelTransactorType.ttCustomer)
        {
            var result = new List<TestModelTransactor>();
            for (var i = 0; i <= count - 1; i++)
            {
                result.Add(
                        new TestModelTransactor()
                        {
                            TransactorId = Guid.NewGuid(),
                            Code = "TR" + i.ToString(),
                            Description = "Transactor" + i.ToString(),
                            Address = "Address" + i.ToString(),
                            Email = "Email" + i.ToString(),
                            Type = transactorType
                        }
                    );
            }
            return result;
        }

        public List<TestModelItem> GetItems(int count = 1000, TestModelTransactorType transactorType = TestModelTransactorType.ttCustomer)
        {
            var result = new List<TestModelItem>();
            for (var i = 0; i <= count - 1; i++)
            {
                result.Add(
                        new TestModelItem()
                        {
                            ItemId = Guid.NewGuid(),
                            Code = "TR" + i.ToString(),
                            Description = "Transactor" + i.ToString(),
                            Price = i
                        }
                    );
            }
            return result;
        }
        #endregion
    }
}
