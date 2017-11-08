using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
{
    /// <summary>
    /// Orpheus database access component.
    /// </summary>
    public interface IOrpheusDatabase
    {
        /// <summary>
        /// Connects to the database engine defined in the connection string.
        /// </summary>
        void Connect(string connectionString = null);
        
        /// <summary>
        /// Disconnects from the database engine.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// State of the database. Connected or not.
        /// </summary>
        /// <returns>True if database is connected</returns>
        bool Connected { get; }

        /// <summary>
        /// List of registered Orpheus modules.
        /// </summary>
        /// <returns>Modules that are part in the database</returns>
        List<IOrpheusModule> Modules { get; }
        
        /// <summary>
        /// Register an Orpheus module to the database.
        /// </summary>
        /// <param name="module">Module to be registered</param>
        void RegisterModule(IOrpheusModule module);
        
        /// <summary>
        /// Creates a transaction object.
        /// </summary>
        /// <returns>Returns a transaction instance</returns>
        IDbTransaction BeginTransaction();
        
        /// <summary>
        /// Create a DbCommand
        /// </summary>
        /// <returns>A DbCommand instance</returns>
        IDbCommand CreateCommand();
        
        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL">SQL for the prepared query</param>
        /// <param name="parameters">SQL parameters</param>
        /// <returns></returns>
        IDbCommand CreatePreparedQuery(string SQL,List<string> parameters);

        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL">SQL for the prepared query</param>
        /// <returns></returns>
        IDbCommand CreatePreparedQuery(string SQL);

        /// <summary>
        /// Mapping dictionary of types to data types.
        /// </summary>
        /// <returns>Type map dictionary between types and DbType</returns>
        Dictionary<Type,System.Data.DbType> TypeMap { get; }

        /// <summary>
        /// Returns true if the type is a nullable type.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>True if type is nullable</returns>
        bool IsNullableType(Type type);

        /// <summary>
        /// Helps execute DDL specific commands for the underlying db engine.
        /// </summary>
        /// <returns>A OrpheusDDLHelper instance</returns>
        IOrpheusDDLHelper DDLHelper { get; set; }

        /// <summary>
        /// Gets the underlying IDbConnection connection string.
        /// </summary>
        /// <returns>The database connection string</returns>
        string ConnectionString { get;}

        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T">Model type for table</typeparam>
        /// <param name="options">Table options</param>
        /// <returns>An Orpheus table instance</returns>
        IOrpheusTable<T> CreateTable<T>(IOrpheusTableOptions options);

        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T">Model type for the table</typeparam>
        /// <param name="tableName">Table name</param>
        /// <param name="keyFields">Table key fields</param>
        /// <returns>An Orpheus table instance</returns>
        IOrpheusTable<T> CreateTable<T>(string tableName,List<IOrpheusTableKeyField> keyFields = null);

        /// <summary>
        /// Creates a table and sets its database,using the type name as the table name.
        /// </summary>
        /// <returns>An Orpheus table instance</returns>
        IOrpheusTable<T> CreateTable<T>();

        /// <summary>
        /// Creates a schema object and sets it's database.
        /// </summary>
        /// <param name="id">Schema id</param>
        /// <param name="description">Schema description</param>
        /// <param name="version">Schema version</param>
        /// <returns>An ISchema instance</returns>
        ISchema CreateSchema(Guid id, string description, double version);

        /// <summary>
        /// Creates an OrpheusModule.
        /// </summary>
        /// <param name="definition">Module definition</param>
        /// <returns>An IOrpheusModule instance</returns>
        IOrpheusModule CreateModule(IOrpheusModuleDefinition definition = null);

        /// <summary>
        /// Creates an OrpheusModuleDefinition.
        /// </summary>
        /// <returns>An IOrpheusModuleDefinition instance</returns>
        IOrpheusModuleDefinition CreateModuleDefinition();

        /// <summary>
        /// Creates an OrpheusTableOptions.
        /// </summary>
        /// <returns></returns>
        IOrpheusTableOptions CreateTableOptions();

        /// <summary>
        /// Creates an OrpheusTableKeyField.
        /// </summary>
        /// <returns></returns>
        IOrpheusTableKeyField CreateTableKeyField();

        /// <summary>
        /// Executes a SQL statement and returns it as specific model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL"></param>
        /// <returns></returns>
        List<T> SQL<T>(string SQL, string tableName = null);
    }
}
