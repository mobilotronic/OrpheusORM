using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrpheusCore.Configuration.Models;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Orpheus configuration manager.
    /// </summary>
    public static class ConfigurationManager
    {
        private static IConfiguration configurationInstance;
        private static OrpheusConfiguration configuration;

        /// <summary>
        /// Extension method to initialize Orpheus configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IConfiguration InitializeOrpheusConfiguration(this IConfiguration configuration)
        {
            ConfigurationManager.configurationInstance = configuration;
            return configuration;
        }
        /// <summary>
        /// Current Orpheus configuration.
        /// </summary>
        public static OrpheusConfiguration Configuration
        {
            get
            {
                if (ConfigurationManager.configuration == null)
                {
                    var config = new OrpheusConfiguration();
                    ConfigurationManager.configurationInstance.Bind(config);
                    ConfigurationManager.configuration = config;
                }
                return ConfigurationManager.configuration;
            }
        }
    }
}
