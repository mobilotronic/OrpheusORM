namespace OrpheusInterfaces.Configuration
{
    /// <summary>
    /// Database connection configuration.
    /// </summary>
    public interface IDatabaseConnectionConfiguration
    {
        /// <value>
        /// Database configuration name.
        /// </value>
        string ConfigurationName { get; set; }

        /// <value>
        /// The database name.
        /// </value>
        string DatabaseName { get; set; }

        /// <value>
        /// Server name or IP address.
        /// </value>
        string Server { get; set; }

        /// <value>
        /// User name.
        /// </value>
        string UserName { get; set; }

        /// <value>
        /// Password.
        /// </value>
        string Password { get; set; }

        /// <value>
        /// SQL Server specific.
        /// </value>
        bool UseIntegratedSecurity { get; set; }

        /// <value>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// This boolean sets this second connection, integrated security setting.
        /// </value>
        bool UseIntegratedSecurityForServiceConnection { get; set; }

        /// <summary>
        /// Creates a clone of this database configuration.
        /// </summary>
        /// <returns></returns>
        IDatabaseConnectionConfiguration Clone();

        /// <value>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// The ServiceUserName is the one that will be used for that connection.
        /// </value>
        string ServiceUserName { get; set; }

        /// <value>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// The ServicePassword is the one that will be used for that connection.
        /// </value>
        string ServicePassword { get; set; }
    }
}
