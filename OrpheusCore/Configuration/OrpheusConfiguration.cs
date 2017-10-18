using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Xml.Serialization;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Orpheus configuration manager.
    /// </summary>
    public static class ConfigurationManager
    {
        private static OrpheusConfiguration configuration;
        private static string configurationFileName = "OrpheusCore.config";
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

        /// <summary>
        /// Initializes default Orpheus configuration. Default configuration file lives in the executing folder of the assembly and its name is 'Orpheus.config'
        /// </summary>
        public static void InitializeConfiguration()
        {
            ConfigurationManager.InitializeConfiguration(ConfigurationManager.assemblyDirectory + @"\" + ConfigurationManager.configurationFileName);
        }

        /// <summary>
        /// Initialize configuration with an existing configuration object.
        /// </summary>
        /// <param name="configuration"></param>
        public static void InitializeConfiguration(OrpheusConfiguration configuration)
        {
            ConfigurationManager.configuration = configuration;
        }

        /// <summary>
        /// Initialize configuration from a file.
        /// </summary>
        /// <param name="configurationFile"></param>
        public static void InitializeConfiguration(string configurationFile)
        {
            var xmlReader = new XmlSerializer(typeof(OrpheusConfiguration));
            using (var fs = new FileStream(configurationFile,FileMode.Open))
            {
                try
                {
                    ConfigurationManager.configuration =  (OrpheusConfiguration)xmlReader.Deserialize(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// Saves configuration to a file.
        /// </summary>
        /// <param name="configurationFile"></param>
        public static void SaveConfiguration(string configurationFile)
        {
            var xmlWriter = new XmlSerializer(typeof(OrpheusConfiguration));
            using (var fs = new FileStream(configurationFile, FileMode.Open))
            {
                try
                {
                    xmlWriter.Serialize(fs, ConfigurationManager.configuration);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

       /// <summary>
        /// Returns the current directory, where Orpheus is being executed.
        /// </summary>
        /// <returns></returns>
        public static string CurrentDirectory { get { return assemblyDirectory; } }

        /// <summary>
        /// Current Orpheus Configuration
        /// </summary>
        public static OrpheusConfiguration Configuration
        {
            get
            {
                return ConfigurationManager.configuration;
            }
        }
    }
}
