using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OrpheusCore.ServiceProvider;
using System.IO;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Orpheus configuration manager.
    /// </summary>
    public static class ConfigurationManager
    {
        private static IConfiguration configurationInstance;

        /// <summary>
        /// Initialize configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public static void InitializeConfiguration(IConfiguration configuration, IServiceCollection services = null)
        {
            configurationInstance = configuration;
            OrpheusServiceProvider.InitializeServiceProvider(services);
        }

        /// <summary>
        /// Initialize configuration from a file.
        /// </summary>
        /// <param name="configurationFile"></param>
        public static void InitializeConfiguration(string configurationFile)
        {
            try
            {
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.SetBasePath(Path.GetDirectoryName(configurationFile));
                configurationBuilder.AddJsonFile(configurationFile, optional: false, reloadOnChange: true);
                InitializeConfiguration(configurationBuilder.Build());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Saves configuration to a file.
        /// </summary>
        /// <param name="configurationFile"></param>
        public static void SaveConfiguration(string configurationFile)
        {
            File.WriteAllText(configurationFile,JsonConvert.SerializeObject(configurationInstance.Get<OrpheusConfiguration>()));
        }

        /// <summary>
        /// Current Orpheus Configuration
        /// </summary>
        public static OrpheusConfiguration Configuration
        {
            get
            {
                return ConfigurationInstance.Get<OrpheusConfiguration>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static IConfiguration ConfigurationInstance { get { return configurationInstance; } }
    }
}
