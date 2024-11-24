using Microsoft.Data.SqlClient;
using OrpheusCore;
using OrpheusInterfaces.Core;

namespace OrpheusSQLDDLHelper
{
    /// <summary>
    /// 
    /// </summary>
    public static class OrpheusSQLServerDatabase
    {
        /// <summary>
        /// Creates an OrpheusDatabase with the underlying connection of SQL server.
        /// </summary>
        /// <returns></returns>
        public static IOrpheusDatabase CreateDatabase()
        {
            var helper = new OrpheusSQLServerDDLHelper(ServiceManager.CreateLogger<OrpheusSQLServerDDLHelper>());
            return new OrpheusDatabase(new SqlConnection(), helper, ServiceManager.CreateLogger<IOrpheusDatabase>());
        }
    }
}
