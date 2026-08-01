using MySql.Data.MySqlClient;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OrpheusMySQLDDLHelper
{
    /// <summary>
    /// Creates pooled MySQL connections from an <see cref="IDatabaseConnectionConfiguration"/>.
    /// </summary>
    public class MySqlConnectionFactory : IOrpheusConnectionFactory
    {
        /// <summary>
        /// MySQL's administrative/system database, used for database-level operations
        /// (existence checks, CREATE DATABASE) that can't run against the target database itself.
        /// </summary>
        private const string AdministrativeDatabaseName = "sys";

        /// <inheritdoc/>
        public IDatabaseConnectionConfiguration ConnectionConfiguration { get; set; }

        /// <param name="databaseNameOverride">
        /// When set, connects to this database instead of <see cref="IDatabaseConnectionConfiguration.DatabaseName"/>.
        /// </param>
        /// <param name="useServiceCredentials">
        /// When true, uses ServiceUserName/ServicePassword, falling back to the main credentials
        /// when the service-specific ones aren't configured.
        /// </param>
        private string buildConnectionString(string databaseNameOverride = null, bool useServiceCredentials = false)
        {
            if (this.ConnectionConfiguration == null)
                throw new ArgumentNullException(nameof(ConnectionConfiguration), "Missing database configuration. This is required so Orpheus can connect to the database.");

            var connBuilder = new MySqlConnectionStringBuilder();
            if (this.ConnectionConfiguration.Port > 0)
                connBuilder.Port = (uint)this.ConnectionConfiguration.Port;
            connBuilder.Server = this.ConnectionConfiguration.Server;
            connBuilder.Database = databaseNameOverride ?? this.ConnectionConfiguration.DatabaseName;
            connBuilder.Pooling = this.ConnectionConfiguration.Pooling;
            connBuilder.MinimumPoolSize = (uint)this.ConnectionConfiguration.MinPoolSize;
            connBuilder.MaximumPoolSize = (uint)this.ConnectionConfiguration.MaxPoolSize;
            connBuilder.ConnectionLifeTime = (uint)this.ConnectionConfiguration.ConnectionIdleTimeout;

            switch (this.ConnectionConfiguration.EncyrptConnection)
            {
                case EncyrptConnection.ecOptional:
                    connBuilder.SslMode = MySqlSslMode.Preferred; break;
                case EncyrptConnection.ecMandatory:
                    connBuilder.SslMode = MySqlSslMode.Required; break;
                case EncyrptConnection.ecStrict:
                    connBuilder.SslMode = MySqlSslMode.VerifyFull; break;
            }

            var userName = useServiceCredentials
                ? this.ConnectionConfiguration.ServiceUserName ?? this.ConnectionConfiguration.UserName
                : this.ConnectionConfiguration.UserName;
            var password = useServiceCredentials
                ? this.ConnectionConfiguration.ServicePassword ?? this.ConnectionConfiguration.Password
                : this.ConnectionConfiguration.Password;

            if (userName != null)
                connBuilder.UserID = userName;
            if (password != null)
                connBuilder.Password = password;

            return connBuilder.ConnectionString;
        }

        /// <inheritdoc/>
        public IDbConnection CreateConnection()
        {
            var connection = new MySqlConnection(this.buildConnectionString());
            connection.Open();
            return connection;
        }

        /// <inheritdoc/>
        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new MySqlConnection(this.buildConnectionString());
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        /// <inheritdoc/>
        public IDbConnection CreateSecondaryConnection()
        {
            return new MySqlConnection(this.buildConnectionString());
        }

        /// <inheritdoc/>
        public IDbConnection CreateAdministrativeConnection()
        {
            return new MySqlConnection(this.buildConnectionString(AdministrativeDatabaseName, useServiceCredentials: true));
        }
    }
}
