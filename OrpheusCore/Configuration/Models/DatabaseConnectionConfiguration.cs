using OrpheusInterfaces.Configuration;

namespace OrpheusCore.Configuration.Models
{
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
        /// Port number. 0 means use the database engine default.
        /// </value>
        public int Port { get; set; }


        /// <value>
        /// User name.
        /// </value>
        public string UserName { get; set; }

        /// <value>
        /// Password.
        /// </value>
        public string Password { get; set; }

        /// <value>
        /// SQL Server specific. If true, any UserName/Password configured will be ignored.
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
        /// True to trust the server certificate. Default is true.
        /// </summary>
        public bool TrustServerCertificate { get; set; } = true;

        /// <summary>
        /// Set if the connection will be encrytped or not.
        /// </summary>
        public EncyrptConnection EncyrptConnection { get; set; }

        /// <summary>
        /// Whether ADO.NET connection pooling is enabled. Default: true.
        /// </summary>
        public bool Pooling { get; set; } = true;

        /// <summary>
        /// Minimum number of connections maintained in the pool. Default: 0.
        /// </summary>
        public int MinPoolSize { get; set; } = 0;

        /// <summary>
        /// Maximum number of connections allowed in the pool. Default: 100.
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// Time, in seconds, a connection can remain idle in the pool before being removed. Default: 300.
        /// </summary>
        public int ConnectionIdleTimeout { get; set; } = 300;

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
                Port = this.Port,
                Password = this.Password,
                UseIntegratedSecurity = this.UseIntegratedSecurity,
                UseIntegratedSecurityForServiceConnection = this.UseIntegratedSecurityForServiceConnection,
                ServicePassword = this.ServicePassword,
                ServiceUserName = this.ServiceUserName,
                TrustServerCertificate = this.TrustServerCertificate,
                EncyrptConnection = this.EncyrptConnection,
                Pooling = this.Pooling,
                MinPoolSize = this.MinPoolSize,
                MaxPoolSize = this.MaxPoolSize,
                ConnectionIdleTimeout = this.ConnectionIdleTimeout
            };
        }
    }
}
