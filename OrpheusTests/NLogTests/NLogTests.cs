using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog.Extensions.Logging;
using System;
using System.IO;

namespace OrpheusTests.NLogTests
{
    [TestClass]
    public class NLogTests : BaseTestClass
    {
        [TestMethod]
        public void TestLogFilePath()
        {
            OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(this.CreateConfiguration(this.CurrentDirectory + @"\" + "OrpheusNLogConfiguration.json"));
            var loggerFactory = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<ILoggerFactory>();
            loggerFactory.AddNLog();
            NLog.LogManager.LoadConfiguration($"{CurrentDirectory}\\nlog.config");
            var logger = loggerFactory.CreateLogger<NLogTests>();

            var errorId = Guid.NewGuid().ToString();
            logger.LogError("Error message {0}", errorId);

            //NLog.LogManager.Configuration.AllTargets.Where(t => t.Name == "target1").First().

            var logFileContents = File.ReadAllText(@"c:\temp\console-example.log");
            Assert.AreEqual(true, logFileContents.Contains(errorId));
        }
    }
}
