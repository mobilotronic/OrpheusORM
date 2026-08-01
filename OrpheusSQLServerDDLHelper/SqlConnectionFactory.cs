using Microsoft.Data.SqlClient;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OrpheusSQLDDLHelper
{
    /// <summary>
    /// Creates pooled SQL Server connections from an <see cref="IDatabaseConnectionConfiguration"/>.
    /// </summary>
    public class SqlConnectionFactory : IOrpheusConnectionFactory
    {
        /// <summary>
        /// SQL Server's administrative/system database, used for database-level operations
        /// (existence checks, CREATE DATABASE) that can't run against the target database itself.
        /// </summary>
        private const string AdministrativeDatabaseName = "master";

        /// <inheritdoc/>
        public IDatabaseConnectionConfiguration ConnectionConfiguration { get; set; }

        /// <param name="databaseNameOverride">
        /// When set, connects to this database instead of <see cref="IDatabaseConnectionConfiguration.DatabaseName"/>.
        /// </param>
        /// <param name="useServiceCredentials">
        /// When true, uses UseIntegratedSecurityForServiceConnection/ServiceUserName/ServicePassword,
        /// falling back to the main credentials when the service-specific ones aren't configured.
        /// </param>
        private string buildConnectionString(string databaseNameOverride = null, bool useServiceCredentials = false)
        {
            if (this.ConnectionConfiguration == null)
                throw new ArgumentNullException(nameof(ConnectionConfiguration), "Missing database configuration. This is required so Orpheus can connect to the database.");

            var integratedSecurity = useServiceCredentials
                ? this.ConnectionConfiguration.UseIntegratedSecurityForServiceConnection
                : this.ConnectionConfiguration.UseIntegratedSecurity;

            var connBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = this.ConnectionConfiguration.Server,
                InitialCatalog = databaseNameOverride ?? this.ConnectionConfiguration.DatabaseName,
                TrustServerCertificate = this.ConnectionConfiguration.TrustServerCertificate,
                IntegratedSecurity = integratedSecurity,
                Pooling = this.ConnectionConfiguration.Pooling,
                MinPoolSize = this.ConnectionConfiguration.MinPoolSize,
                MaxPoolSize = this.ConnectionConfiguration.MaxPoolSize,
                LoadBalanceTimeout = this.ConnectionConfiguration.ConnectionIdleTimeout
            };

            if (useServiceCredentials)
            {
                // Matches the main connection's IntegratedSecurity semantics: only set credentials
                // when not using integrated security, falling back to the main credentials when no
                // separate service credentials are configured.
                if (!integratedSecurity)
                {
                    var userName = this.ConnectionConfiguration.ServiceUserName ?? this.ConnectionConfiguration.UserName;
                    var password = this.ConnectionConfiguration.ServicePassword ?? this.ConnectionConfiguration.Password;
                    if (userName != null)
                        connBuilder.UserID = userName;
                    if (password != null)
                        connBuilder.Password = password;
                }
            }
            else
            {
                if (this.ConnectionConfiguration.UserName != null)
                    connBuilder.UserID = this.ConnectionConfiguration.UserName;
                if (this.ConnectionConfiguration.Password != null)
                    connBuilder.Password = this.ConnectionConfiguration.Password;
            }

            switch (this.ConnectionConfiguration.EncyrptConnection)
            {
                case EncyrptConnection.ecOptional:
                    connBuilder["Encrypt"] = "false"; break;
                case EncyrptConnection.ecMandatory:
                    connBuilder.Encrypt = SqlConnectionEncryptOption.Mandatory; break;
                case EncyrptConnection.ecStrict:
                    connBuilder.Encrypt = SqlConnectionEncryptOption.Strict; break;
            }

            return connBuilder.ConnectionString;
        }

        /// <inheritdoc/>
        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(this.buildConnectionString());
            connection.Open();
            return connection;
        }

        /// <inheritdoc/>
        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(this.buildConnectionString());
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        /// <inheritdoc/>
        public IDbConnection CreateSecondaryConnection()
        {
            return new SqlConnection(this.buildConnectionString());
        }

        /// <inheritdoc/>
        public IDbConnection CreateAdministrativeConnection()
        {
            return new SqlConnection(this.buildConnectionString(AdministrativeDatabaseName, useServiceCredentials: true));
        }
    }
}
