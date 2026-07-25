using MySql.Data.MySqlClient;
using OrpheusCore;
using OrpheusInterfaces.Core;

namespace OrpheusMySQLDDLHelper
{
    /// <summary>
    /// Factory for creating an OrpheusDatabase backed by a MySQL connection.
    /// </summary>
    public static class OrpheusMySQLServerDatabase
    {
        /// <summary>
        /// Creates an <see cref="IOrpheusDatabase"/> with a MySqlConnection and
        /// MySQL DDL helper.
        /// </summary>
        public static IOrpheusDatabase CreateDatabase()
        {
            var helper = new OrpheusMySQLServerDDLHelper(
                ServiceManager.CreateLogger<OrpheusMySQLServerDDLHelper>());
            return new OrpheusDatabase(
                new MySqlConnection(), helper,
                ServiceManager.CreateLogger<IOrpheusDatabase>());
        }
    }
}
