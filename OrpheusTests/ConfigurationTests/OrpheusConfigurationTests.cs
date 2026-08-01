using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusCore;
using OrpheusInterfaces.Core;
using OrpheusSQLDDLHelper;
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
        [TestMethod]
        [TestCategory(BaseTestClass.SQLServerTests)]
        public void AddOrpheusSqlServerFromConfigurationOverload()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(this.CurrentDirectory)
                .AddJsonFile(BaseTestClass.ConfigurationFileName, optional: false);
            //CI runners have no Windows/Kerberos environment, so integrated security can't work there;
            //this overlay swaps SQL Server to SQL authentication, matching BaseTestClass.createConfiguration.
            if (string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase))
            {
                configurationBuilder.AddJsonFile("OrpheusConfig.CI.json", optional: true);
            }
            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddOrpheusSqlServer(configuration, "SQLServer");
            using var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<IOrpheusDatabase>();

            db.Connect();
            Assert.IsTrue(db.Connected);
            db.Disconnect();
        }

        [TestMethod]
        [TestCategory(BaseTestClass.SQLServerTests)]
        public void AddOrpheusSqlServerFromConfigurationOverloadWithCustomSection()
        {
            //CI runners have no Windows/Kerberos environment, so integrated security can't work there.
            var isCI = string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);
            var json = isCI
                ? @"{
                ""MyApp"": {
                    ""Orpheus"": {
                        ""DatabaseConnections"": [
                            {
                                ""ConfigurationName"": ""SQLServer"",
                                ""Server"": ""localhost"",
                                ""DatabaseName"": ""orpheusTestDB"",
                                ""UseIntegratedSecurity"": false,
                                ""UseIntegratedSecurityForServiceConnection"": false,
                                ""UserName"": ""sa"",
                                ""Password"": ""1StrongPwd!!"",
                                ""ServiceUserName"": ""sa"",
                                ""ServicePassword"": ""1StrongPwd!!""
                            }
                        ]
                    }
                }
            }"
                : @"{
                ""MyApp"": {
                    ""Orpheus"": {
                        ""DatabaseConnections"": [
                            {
                                ""ConfigurationName"": ""SQLServer"",
                                ""Server"": ""localhost"",
                                ""DatabaseName"": ""orpheusTestDB"",
                                ""UseIntegratedSecurity"": true,
                                ""UseIntegratedSecurityForServiceConnection"": true
                            }
                        ]
                    }
                }
            }";
            using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(jsonStream)
                .Build();

            var services = new ServiceCollection();
            services.AddOrpheusSqlServer(configuration, "SQLServer", "MyApp:Orpheus");
            using var provider = services.BuildServiceProvider();
            var db = provider.GetRequiredService<IOrpheusDatabase>();

            db.Connect();
            Assert.IsTrue(db.Connected);
            db.Disconnect();
        }

        [TestMethod]
        public async Task ReloadConfigurationAsync()
        {
            this.InitializeConfiguration();

            var errorId = Guid.NewGuid().ToString();
            var traceId = Guid.NewGuid().ToString();
            var logFileContentsName = $"{this.CurrentDirectory}/nlog-all-{DateTime.Now.ToString("yyyy-MM-dd")}.log";

            var logger = ServiceManager.CreateLogger<OrpheusConfigurationTests>();
            logger.LogError($"ErrorId {errorId} test Error log entry");

            //loading the log file content.

            using var fileStream = new StreamReader(logFileContentsName, new FileStreamOptions() { Mode = FileMode.Open, Access = FileAccess.Read, Share = FileShare.ReadWrite });
            string logFileContents = fileStream.ReadToEnd();

            //making sure that the error is logged.
            Assert.AreEqual(true, logFileContents.Contains(errorId));

            //loading the NLog XML configuration file and updating the logging level.
            XmlDocument doc = new XmlDocument();
            doc.Load($"{this.CurrentDirectory}/nlog.config");
            XmlNodeList nlogRules = doc.DocumentElement.SelectNodes("//*[name()='nlog']/*[name()='rules']/*[name()='logger']");
            foreach (XmlNode nlogRule in nlogRules)
            {
                XmlAttribute logLevel = nlogRule.Attributes["minlevel"];
                if (logLevel != null)
                {
                    logLevel.Value = "Trace";
                }
            }
            doc.Save($"{this.CurrentDirectory}/nlog.config");

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
