using Npgsql;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OrpheusPostgreSQLDDLHelper
{
    /// <summary>
    /// Creates pooled PostgreSQL connections from an <see cref="IDatabaseConnectionConfiguration"/>.
    /// </summary>
    public class NpgsqlConnectionFactory : IOrpheusConnectionFactory
    {
        /// <summary>
        /// PostgreSQL's administrative/system database, used for database-level operations
        /// (existence checks, CREATE DATABASE) that can't run against the target database itself.
        /// </summary>
        private const string AdministrativeDatabaseName = "postgres";

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

            var connBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = this.ConnectionConfiguration.Server,
                Database = databaseNameOverride ?? this.ConnectionConfiguration.DatabaseName,
                Pooling = this.ConnectionConfiguration.Pooling,
                MinPoolSize = this.ConnectionConfiguration.MinPoolSize,
                MaxPoolSize = this.ConnectionConfiguration.MaxPoolSize,
                ConnectionIdleLifetime = this.ConnectionConfiguration.ConnectionIdleTimeout
            };
            if (this.ConnectionConfiguration.Port > 0)
                connBuilder.Port = this.ConnectionConfiguration.Port;

            var userName = useServiceCredentials
                ? this.ConnectionConfiguration.ServiceUserName ?? this.ConnectionConfiguration.UserName
                : this.ConnectionConfiguration.UserName;
            var password = useServiceCredentials
                ? this.ConnectionConfiguration.ServicePassword ?? this.ConnectionConfiguration.Password
                : this.ConnectionConfiguration.Password;

            if (userName != null)
                connBuilder.Username = userName;
            if (password != null)
                connBuilder.Password = password;

            switch (this.ConnectionConfiguration.EncyrptConnection)
            {
                case EncyrptConnection.ecOptional:
                    connBuilder.SslMode = SslMode.Prefer; break;
                case EncyrptConnection.ecMandatory:
                case EncyrptConnection.ecStrict:
                    connBuilder.SslMode = SslMode.Require; break;
            }

            return connBuilder.ConnectionString;
        }

        /// <inheritdoc/>
        public IDbConnection CreateConnection()
        {
            var connection = new NpgsqlConnection(this.buildConnectionString());
            connection.Open();
            return connection;
        }

        /// <inheritdoc/>
        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(this.buildConnectionString());
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        /// <inheritdoc/>
        public IDbConnection CreateSecondaryConnection()
        {
            return new NpgsqlConnection(this.buildConnectionString());
        }

        /// <inheritdoc/>
        public IDbConnection CreateAdministrativeConnection()
        {
            return new NpgsqlConnection(this.buildConnectionString(AdministrativeDatabaseName, useServiceCredentials: true));
        }
    }
}
