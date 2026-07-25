using System;
using System.Data;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// Factory that creates database connections from the ADO.NET connection pool.
    /// Implementations are specific to each database engine (SQL Server, MySQL, PostgreSQL).
    /// </summary>
    public interface IOrpheusConnectionFactory
    {
        /// <summary>
        /// Creates and opens a new <see cref="IDbConnection"/> from the connection pool.
        /// Callers must dispose the returned connection when done.
        /// </summary>
        IDbConnection CreateConnection();
    }
}
