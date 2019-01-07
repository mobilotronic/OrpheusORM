using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OrpheusCore.Configuration;
using OrpheusInterfaces.Logging;
using OrpheusLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OrpheusTests.LoggerTests
{
    [TestClass]
    public class OrpheusLoggerTests : BaseTestClass
    {
        private static string assemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        [TestMethod]
        public void TestSeverityLevel()
        {
            ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json"));
            var logger = (IOrpheusLogger)OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
            logger.Configuration.Level = "Information";
            var errorId = Guid.NewGuid().ToString();
            var informationId = Guid.NewGuid().ToString();
            var debugId = Guid.NewGuid().ToString();
            logger.LogError("Error message {0}", errorId);
            logger.LogInformation("Information message {0}", informationId);
            logger.Configuration.Level = "Error";
            logger.LogDebug("Debug message {0}", debugId);

            var logFileContents = File.ReadAllText(logger.CurrentFileName);

            Assert.AreEqual(true, logFileContents.Contains(informationId));
            Assert.AreEqual(true, logFileContents.Contains(errorId));
            Assert.AreEqual(false, logFileContents.Contains(debugId));
        }

        [TestMethod]
        public async Task TestCallStackLogging()
        {
            var configurationFileName = this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json";
            ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(configurationFileName));
            OrpheusConfiguration orpheusConfiguration = JsonConvert.DeserializeObject<OrpheusConfiguration>(File.ReadAllText(configurationFileName));

            var debugId = Guid.NewGuid().ToString();
            var traceId = Guid.NewGuid().ToString();

            //make sure the logging level is set to debug.
            var logger = (IOrpheusLogger)OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
            orpheusConfiguration.Logging.Level = "Debug";
            File.WriteAllText(configurationFileName, JsonConvert.SerializeObject(orpheusConfiguration));
            await Task.Delay(1000);
            //log the debug message, which should not have a stack trace.
            logger.LogDebug("Debug message {0}", debugId);
            var logLines = new List<string>(File.ReadAllLines(logger.CurrentFileName));
            var logLine = logLines.Find(l => l.Contains(debugId));
            var logEntry = JsonConvert.DeserializeObject<LogEntry>(logLine);
            Assert.IsNull(logEntry.StackTrace);

            //now change the logging level back to trace.
            orpheusConfiguration.Logging.Level = "Trace";
            File.WriteAllText(configurationFileName, JsonConvert.SerializeObject(orpheusConfiguration));
            await Task.Delay(1000);

            //log any message, which should  have a stack trace.
            logger.LogInformation("Trace message {0}", traceId);
            logLines = new List<string>(File.ReadAllLines(logger.CurrentFileName));
            logLine = logLines.Find(l => l.Contains(traceId));
            logEntry = JsonConvert.DeserializeObject<LogEntry>(logLine);
            Assert.IsNotNull(logEntry.StackTrace);

        }

        [TestMethod]
        public void TestLogFilePath()
        {
            var newFolder = @"C:\Temp";
            OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json"));
            Directory.CreateDirectory(newFolder);
            var logger = (IOrpheusLogger)OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
            ConfigurationManager.Configuration.Logging.FilePath = newFolder;
            if (Directory.Exists(newFolder + @"\Orpheus"))
            {
                foreach (var fileInfo in Directory.GetFiles(newFolder + @"\Orpheus", "*.log", SearchOption.AllDirectories))
                {
                    File.Delete(fileInfo);
                }
            }
            var errorId = Guid.NewGuid().ToString();
            logger.LogError("Error message {0}", errorId);

            Assert.AreEqual(true, File.Exists(logger.CurrentFileName));

            var logFileContents = File.ReadAllText(logger.CurrentFileName);
            Assert.AreEqual(true, logFileContents.Contains(errorId));
        }

        //[TestMethod]
        public void TestLogFileSize()
        {
            ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServer.config"));
            var logger = (IOrpheusLogger)OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
            ConfigurationManager.Configuration.Logging.Level = "Debug";
            ConfigurationManager.Configuration.Logging.MaxFileSize = 1;
            var logFilePath = Path.GetDirectoryName(logger.CurrentFileName);
            if (Directory.Exists(logFilePath))
            {
                foreach (var fileInfo in Directory.GetFiles(logFilePath, "*.log", SearchOption.AllDirectories))
                {
                    File.Delete(fileInfo);
                }
            }
            for (var i = 0; i <= 10000; i++)
            {
                logger.LogInformation("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum");
            }
            //Thread.Sleep(3000);
            Assert.AreEqual(true,Directory.GetFiles(Path.GetDirectoryName(logger.CurrentFileName)).Length == 3);
        }
    }
}
