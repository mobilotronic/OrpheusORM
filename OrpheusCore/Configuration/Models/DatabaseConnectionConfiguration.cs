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
                UseIntegratedSecurityForServiceConnection = this.UseIntegratedSecurityForServiceConnection,
                ServicePassword = this.ServicePassword,
                ServiceUserName = this.ServiceUserName,
                TrustServerCertificate = this.TrustServerCertificate,
                EncyrptConnection = this.EncyrptConnection
            };
        }
    }
}
