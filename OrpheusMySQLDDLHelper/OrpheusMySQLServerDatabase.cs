using Microsoft.Data.SqlClient;
using OrpheusCore;
using OrpheusInterfaces.Core;

namespace OrpheusMySQLDDLHelper
{
    /// <summary>
    /// 
    /// </summary>
    public static class OrpheusMySQLServerDatabase
    {
        /// <summary>
        /// Creates an OrpheusDatabase with the underlying connection of MySQL server.
        /// </summary>
        /// <returns></returns>
        public static IOrpheusDatabase CreateDatabase()
        {
            var helper = new OrpheusMySQLServerDDLHelper(ServiceManager.CreateLogger<OrpheusMySQLServerDDLHelper>());
            return new OrpheusDatabase(new SqlConnection(), helper, ServiceManager.CreateLogger<IOrpheusDatabase>());
        }
    }
}
