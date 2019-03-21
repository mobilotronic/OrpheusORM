using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrpheusInterfaces.Core;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
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
        private ILogger logger;
        private string schemaId = "6E8653BE-CB9C-4855-8909-2846AFBB72E1";
        private IConfiguration configuration;
        private IOrpheusDatabase db;
        private string fileName;
        private byte[] currentAppsettingsHash = new byte[0];

        private string assemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        ////taken from https://docs.microsoft.com/en-us/aspnet/core/fundamentals/primitives/change-tokens
        //private byte[] computeHash(string filePath)
        //{
        //    var runCount = 1;

        //    while (runCount < 4)
        //    {
        //        try
        //        {
        //            if (File.Exists(filePath))
        //            {
        //                using (var fs = File.OpenRead(filePath))
        //                {
        //                    return System.Security.Cryptography.SHA1.Create().ComputeHash(fs);
        //                }
        //            }
        //        }
        //        catch (IOException ex)
        //        {
        //            if (runCount == 3 || ex.HResult != -2147024864)
        //            {
        //                throw;
        //            }
        //            else
        //            {
        //                Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, runCount)));
        //                runCount++;
        //            }
        //        }
        //    }

        //    return new byte[20];
        //}

        //// monitoring configuration file for changes.
        //// the logger is registered as a Singleton, therefore it loads configuration only once during initialization.
        //// if we want to make the logger react to changes made to the logging configuration, we have to manually monitor the file.
        //// there is always the option to make the logger a transient, and through IOptionsSnapShot, always load the latest configuration
        //// i am not sure how performant that would be in this case.
        //private void configurationChanged()
        //{
        //    byte[] newAppsettingsHash = this.computeHash(this.fileName);
        //    if (!this.currentAppsettingsHash.SequenceEqual(newAppsettingsHash))
        //    {
        //        this.currentAppsettingsHash = newAppsettingsHash;
        //        var logger = (IOrpheusFileLogger)OrpheusServiceProvider.Resolve<ILogger>();
        //        logger.LoggingConfiguration = this.configuration.Get<OrpheusConfiguration>().Logging;
        //    }
        //}

        #endregion

        #region public declarations
        public const string SQLServerTests = "SQLServer";
        public const string MySQLServerTests = "MySQLServerTests";
        public const string LoggerTests = "LoggerTests";
        public const string ConfigurationTests = "ConfigurationTests";

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
            return new TestSchema(this.Database,"Test Schema", 1.1, Guid.Parse(this.schemaId), name);
        }

        public IConfiguration CreateConfiguration(string configurationFile)
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

        public IConfiguration Configuration => this.configuration;

        public DbEngine DatabaseEngine { get; set; }

        public IOrpheusDatabase Database
        {
            get
            {
                if (this.db == null)
                {
                    switch (this.DatabaseEngine)
                    {
                        case DbEngine.dbSQLServer:
                            {
                                OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(
                                    this.CreateConfiguration(this.assemblyDirectory + @"\" + "OrpheusSQLServerConfig.json")
                                    );
                                this.db = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<IOrpheusDatabase>();
                                this.db.DatabaseConnectionConfiguration = OrpheusCore.Configuration.ConfigurationManager.Configuration.DatabaseConnections.FirstOrDefault(c => c.ConfigurationName.ToLower() == "default").Clone();
                                break;
                            }
                        case DbEngine.dbMySQL:
                            {
                                OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(
                                    this.CreateConfiguration(this.assemblyDirectory + @"\" + "OrpheusMySQLServerConfig.json")
                                    );
                                this.db = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<IOrpheusDatabase>();
                                this.db.DatabaseConnectionConfiguration = OrpheusCore.Configuration.ConfigurationManager.Configuration.DatabaseConnections.FirstOrDefault(c => c.ConfigurationName.ToLower() == "default");
                                break;
                            }
                    }
                }
                return this.db;
            }
        }

        public string ConnectionString
        {
            get
            {
                switch (this.DatabaseEngine)
                {
                    case DbEngine.dbSQLServer:
                        {
                            return TestDatabaseConnectionStrings.SQLServer;
                        }
                    case DbEngine.dbMySQL:
                        {
                            #if MYSQL_DEBUG
                            return TestDatabaseConnectionStrings.MySQLProfiling;
                            #else
                            return TestDatabaseConnectionStrings.MySQL;
                            #endif
                        }
                    default: return null;
                }
            }
        }

        public string CurrentDirectory { get { return this.assemblyDirectory; } }

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
            this.Logger.LogInformation(message);
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
