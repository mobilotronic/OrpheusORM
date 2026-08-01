using OrpheusInterfaces.Configuration;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// Factory that creates database connections from the ADO.NET connection pool.
    /// Implementations are specific to each database engine (SQL Server, MySQL, PostgreSQL).
    /// </summary>
    public interface IOrpheusConnectionFactory
    {
        /// <summary>
        /// The database connection configuration used to build connection strings.
        /// Must be set before calling CreateConnection()/CreateConnectionAsync().
        /// </summary>
        IDatabaseConnectionConfiguration ConnectionConfiguration { get; set; }

        /// <summary>
        /// Creates and opens a new <see cref="IDbConnection"/> from the connection pool.
        /// Callers must dispose the returned connection when done.
        /// </summary>
        IDbConnection CreateConnection();

        /// <summary>
        /// Asynchronously creates and opens a new <see cref="IDbConnection"/> from the connection pool.
        /// Callers must dispose the returned connection when done.
        /// </summary>
        Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates (but does not open) a new pooled connection to the same target database as
        /// <see cref="CreateConnection"/>, independent of the main database's connection/transaction
        /// state — e.g. for schema introspection while the main connection may be mid-transaction.
        /// Unlike CreateConnection(), the connection is left closed: callers that hold onto this
        /// connection across multiple operations manage its open/close state themselves.
        /// Callers must dispose the returned connection when done.
        /// </summary>
        IDbConnection CreateSecondaryConnection();

        /// <summary>
        /// Creates (but does not open) a new pooled connection to the engine's administrative/system
        /// database (SQL Server: master, MySQL: sys, PostgreSQL: postgres), using ServiceUserName/
        /// ServicePassword/UseIntegratedSecurityForServiceConnection when configured, falling back to
        /// the main credentials otherwise. Used for database-level operations — existence checks,
        /// CREATE DATABASE — that can't run against the target database itself. Unlike
        /// CreateConnection(), the connection is left closed: callers that hold onto this connection
        /// across multiple operations manage its open/close state themselves.
        /// Callers must dispose the returned connection when done.
        /// </summary>
        IDbConnection CreateAdministrativeConnection();
    }
}
