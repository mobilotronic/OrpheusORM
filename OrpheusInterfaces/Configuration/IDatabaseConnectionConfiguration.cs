namespace OrpheusInterfaces.Configuration
{
    /// <summary>
    /// Database connection configuration.
    /// </summary>
    public interface IDatabaseConnectionConfiguration
    {
        /// <summary>
        /// Database configuration name.
        /// </summary>
        string ConfigurationName { get; set; }

        /// <summary>
        /// The database name.
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// Server name or IP address.
        /// </summary>
        string Server { get; set; }

        /// <summary>
        /// User name.
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// SQL Server specific.
        /// </summary>
        bool UseIntegratedSecurity { get; set; }

        /// <summary>
        /// Implicitly Orpheus makes a second connection to the database, to perform mainly schema related/DDL functionality.
        /// This boolean sets this second connection, integrated security setting.
        /// </summary>
        bool UseIntegratedSecurityForServiceConnection { get; set; }

        /// <summary>
        /// Creates a clone of this database configuration.
        /// </summary>
        /// <returns></returns>
        IDatabaseConnectionConfiguration Clone();
    }
}
