using System;
using System.Configuration;
using System.IO;
using System.Reflection;


namespace OrpheusCore
{
    public static class Orpheus
    {
        private static Configuration configuration;
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
            Orpheus.InitializeConfiguration(Orpheus.assemblyDirectory + @"\" + Orpheus.configurationFileName);
        }

        /// <summary>
        /// Initialize configuration with an existing configuration object.
        /// </summary>
        /// <param name="configuration"></param>
        public static void InitializeConfiguration(Configuration configuration)
        {
            Orpheus.configuration = configuration;
        }

        /// <summary>
        /// Initialize configuration from a file.
        /// </summary>
        /// <param name="configurationFile"></param>
        public static void InitializeConfiguration(string configurationFile)
        {
            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configurationFile };
            Orpheus.configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        public static Configuration Configuration
        {
            get
            {
                return Orpheus.configuration;
            }
        }
    }
}
