using Npgsql;
using OrpheusCore;
using OrpheusInterfaces.Core;

namespace OrpheusPostgreSQLDDLHelper
{
    /// <summary>
    /// Factory for creating an OrpheusDatabase backed by a PostgreSQL connection.
    /// </summary>
    public static class OrpheusPostgreSQLServerDatabase
    {
        /// <summary>
        /// Creates an <see cref="IOrpheusDatabase"/> with an Npgsql connection and
        /// PostgreSQL DDL helper.
        /// </summary>
        public static IOrpheusDatabase CreateDatabase()
        {
            var helper = new OrpheusPostgreSQLDDLHelper(
                ServiceManager.CreateLogger<OrpheusPostgreSQLDDLHelper>());
            return new OrpheusDatabase(
                new NpgsqlConnection(), helper,
                ServiceManager.CreateLogger<IOrpheusDatabase>());
        }
    }
}
