using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Logging;
using System.Collections.Generic;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Service DI configuration item.
    /// </summary>
    public class ServiceProviderItem
    {
        /// <summary>
        /// Fully qualified name for the service implementation.
        /// </summary>
        public string Implementation { get; set; }

        /// <summary>
        /// Fully qualified name for the service interface.
        /// </summary>
        public string Service { get; set; }
        
        /// <summary>
        /// Service life time.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ServiceLifetime ServiceLifetime { get; set; }

        /// <summary>
        /// Obsolete, to be removed.
        /// </summary>
        public List<string> ConstructorParameters { get; set; }

        /// <summary>
        /// Creates a ServiceProviderItem.
        /// </summary>
        public ServiceProviderItem()
        {
            this.ServiceLifetime = ServiceLifetime.Transient;
        }
    }

    /// <summary>
    /// Orpheus logging configuration.
    /// </summary>
    public class LoggingConfiguration : ILoggingConfiguration
    {
        /// <summary>
        /// Logging level.
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// Log file path.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Maximum log file size.
        /// </summary>
        public int MaxFileSize { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LoggingConfiguration()
        {
            this.Level = "None";
        }
    }

    /// <summary>
    /// Orpheus database configuration.
    /// </summary>
    public class DatabaseConnectionConfiguration : IDatabaseConnectionConfiguration
    {
        /// <summary>
        /// Database configuration name.
        /// </summary>
        public string ConfigurationName { get; set; }
        
        /// <summary>
        /// The database name.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Server name or IP address.
        /// </summary>
        public string Server { get; set; }
        
        /// <summary>
        /// User name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// SQL Server specific.
        /// </summary>
        public bool UseIntegratedSecurity { get; set; }

        /// <summary>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// This boolean sets this second connection, integrated security setting.
        /// </summary>
        public bool UseIntegratedSecurityForServiceConnection { get; set; }

        /// <summary>
        /// Creates a clone of this database configuration.
        /// </summary>
        /// <returns></returns>
        public IDatabaseConnectionConfiguration Clone()
        {
            return new DatabaseConnectionConfiguration()
            {
                ConfigurationName = this.ConfigurationName,
                DatabaseName = this.DatabaseName,
                Server = this.Server,
                UserName = this.UserName,
                Password = this.Password,
                UseIntegratedSecurity = this.UseIntegratedSecurity,
                UseIntegratedSecurityForServiceConnection = this.UseIntegratedSecurityForServiceConnection
            };
        }
    }

    /// <summary>
    /// Orpheus's configuration.
    /// </summary>
    public class OrpheusConfiguration
    {
        /// <summary>
        /// List of services.
        /// </summary>
        public List<ServiceProviderItem> Services { get; set; }

        /// <summary>
        /// Database connections information.
        /// </summary>
        public List<DatabaseConnectionConfiguration> DatabaseConnections { get; set; }

        /// <summary>
        /// Logging configuration.
        /// </summary>
        public LoggingConfiguration Logging { get; set; }

        /// <summary>
        /// Default size for string field, when creating a db schema.
        /// </summary>
        public int DefaultStringSize { get; set; }

        /// <summary>
        /// Creates an Orpheus configuration.
        /// </summary>
        public OrpheusConfiguration()
        {
            this.DefaultStringSize = 60;
        }
    }
}
