using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusCore.Configuration;
using OrpheusLogger;
using System;
using System.IO;
using System.Reflection;

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
            var logger = (IOrpheusFileLogger)OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
            logger.LoggingConfiguration.Level = "Information";
            var errorId = Guid.NewGuid().ToString();
            var informationId = Guid.NewGuid().ToString();
            var debugId = Guid.NewGuid().ToString();
            logger.LogError("Error message {0}", errorId);
            logger.LogInformation("Information message {0}", informationId);
            logger.LoggingConfiguration.Level = "Error";
            logger.LogDebug("Debug message {0}", debugId);

            var logFileContents = File.ReadAllText(logger.LogFileName);

            Assert.AreEqual(true, logFileContents.Contains(informationId));
            Assert.AreEqual(true, logFileContents.Contains(errorId));
            Assert.AreEqual(false, logFileContents.Contains(debugId));


        }

        [TestMethod]
        public void TestLogFilePath()
        {
            var newFolder = @"C:\Temp";
            OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServerConfig.json"));
            Directory.CreateDirectory(newFolder);
            var logger = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
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

            Assert.AreEqual(true, File.Exists((logger as OrpheusFileLogger).LogFileName));

            var logFileContents = File.ReadAllText((logger as OrpheusFileLogger).LogFileName);
            Assert.AreEqual(true, logFileContents.Contains(errorId));
        }

        //[TestMethod]
        public void TestLogFileSize()
        {
            ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusSQLServer.config"));
            var logger = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
            ConfigurationManager.Configuration.Logging.Level = "Debug";
            ConfigurationManager.Configuration.Logging.MaxFileSize = 1;
            var logFilePath = Path.GetDirectoryName((logger as OrpheusFileLogger).LogFileName);
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
            Assert.AreEqual(true,Directory.GetFiles(Path.GetDirectoryName((logger as OrpheusFileLogger).LogFileName)).Length == 3);
        }
    }
}
