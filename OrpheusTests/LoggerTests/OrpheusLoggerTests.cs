using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using OrpheusLogger;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace OrpheusTests.LoggerTests
{
    [TestClass]
    public class OrpheusLoggerTests
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
            OrpheusCore.Orpheus.InitializeConfiguration(assemblyDirectory + @"\" + "OrpheusSQLServer.config");
            var logger = OrpheusCore.OrpheusIocContainer.Resolve<ILogger>();
            OrpheusLoggerConfiguration.Configuration.Level = "Information";
            var errorId = Guid.NewGuid().ToString();
            var informationId = Guid.NewGuid().ToString();
            var debugId = Guid.NewGuid().ToString();
            logger.LogError("Error message {0}", errorId);
            logger.LogInformation("Information message {0}", informationId);
            OrpheusLoggerConfiguration.Configuration.Level = "Error";
            logger.LogDebug("Debug message {0}", debugId);

            var logFileContents = File.ReadAllText((logger as OrpheusFileLogger).LogFileName);

            Assert.AreEqual(true, logFileContents.Contains(informationId));
            Assert.AreEqual(true, logFileContents.Contains(errorId));
            Assert.AreEqual(false, logFileContents.Contains(debugId));


        }

        [TestMethod]
        public void TestLogFilePath()
        {
            var newFolder = @"C:\Temp";
            OrpheusCore.Orpheus.InitializeConfiguration(assemblyDirectory + @"\" + "OrpheusSQLServer.config");
            Directory.CreateDirectory(newFolder);
            var logger = OrpheusCore.OrpheusIocContainer.Resolve<ILogger>();
            OrpheusLoggerConfiguration.Configuration.FilePath = newFolder;
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

        [TestMethod]
        public void TestLogFileSize()
        {
            OrpheusCore.Orpheus.InitializeConfiguration(assemblyDirectory + @"\" + "OrpheusSQLServer.config");
            var logger = OrpheusCore.OrpheusIocContainer.Resolve<ILogger>();
            OrpheusLoggerConfiguration.Configuration.Level = "Debug";
            OrpheusLoggerConfiguration.Configuration.MaxFileSize = 1;
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
            Assert.AreEqual(true,Directory.GetFiles(Path.GetDirectoryName((logger as OrpheusFileLogger).LogFileName)).Length == 3);
        }
    }
}
