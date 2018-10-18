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
        /// Gets or sets the implementation.
        /// </summary>
        /// <value>
        /// The implementation.
        /// </value>
        public string Implementation { get; set; }

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the service lifetime.
        /// </summary>
        /// <value>
        /// The service lifetime.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public ServiceLifetime ServiceLifetime { get; set; }

        /// <summary>
        /// Gets or sets the constructor parameters.
        /// </summary>
        /// <value>
        /// The constructor parameters.
        /// </value>
        /// <remarks>Obsolete</remarks>
        public List<string> ConstructorParameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderItem"/> class.
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
        /// <value>
        /// Logging level.
        /// </value>
        public string Level { get; set; }

        /// <value>
        /// Log file path.
        /// </value>
        public string FilePath { get; set; }

        /// <value>
        /// Maximum log file size.
        /// </value>
        public int MaxFileSize { get; set; }

        /// <value>
        /// Constructor
        /// </value>
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
        /// <value>
        /// Database configuration name.
        /// </value>
        public string ConfigurationName { get; set; }

        /// <value>
        /// The database name.
        /// </value>
        public string DatabaseName { get; set; }

        /// <value>
        /// Server name or IP address.
        /// </value>
        public string Server { get; set; }

        /// <value>
        /// User name.
        /// </value>
        public string UserName { get; set; }

        /// <value>
        /// Password.
        /// </value>
        public string Password { get; set; }

        /// <value>
        /// SQL Server specific.
        /// </value>
        public bool UseIntegratedSecurity { get; set; }

        /// <value>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// This boolean sets this second connection, integrated security setting.
        /// </value>
        public bool UseIntegratedSecurityForServiceConnection { get; set; }

        /// <value>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// The ServiceUserName is the one that will be used for that connection.
        /// </value>
        public string ServiceUserName { get; set; }

        /// <value>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// The ServicePassword is the one that will be used for that connection.
        /// </value>
        public string ServicePassword { get; set; }

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
        /// Gets or sets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public List<ServiceProviderItem> Services { get; set; }

        /// <summary>
        /// Gets or sets the database connections.
        /// </summary>
        /// <value>
        /// The database connections.
        /// </value>
        public List<DatabaseConnectionConfiguration> DatabaseConnections { get; set; }

        /// <summary>
        /// Gets or sets the logging.
        /// </summary>
        /// <value>
        /// The logging.
        /// </value>
        public LoggingConfiguration Logging { get; set; }

        /// <summary>
        /// Gets or sets the default size of the string.
        /// </summary>
        /// <value>
        /// The default size of a string field, when creating the db schema.
        /// </value>
        public int DefaultStringSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrpheusConfiguration"/> class.
        /// </summary>
        public OrpheusConfiguration()
        {
            this.DefaultStringSize = 60;
        }
    }
}
