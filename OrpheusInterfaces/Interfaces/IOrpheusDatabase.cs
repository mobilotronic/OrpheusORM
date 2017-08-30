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
        bool Connected { get; }
        
        /// <summary>
        /// List of registered Orpheus modules.
        /// </summary>
        List<IOrpheusModule> Modules { get; }
        
        /// <summary>
        /// Register an Orpheus module to the database.
        /// </summary>
        /// <param name="module"></param>
        void RegisterModule(IOrpheusModule module);
        
        /// <summary>
        /// Creates a transaction object.
        /// </summary>
        /// <returns></returns>
        IDbTransaction BeginTransaction();
        
        /// <summary>
        /// /
        /// </summary>
        /// <returns></returns>
        IDbCommand CreateCommand();
        
        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDbCommand CreatePreparedQuery(string SQL,List<string> parameters);

        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        IDbCommand CreatePreparedQuery(string SQL);

        /// <summary>
        /// Mapping dictionary of types to data types.
        /// </summary>
        Dictionary<Type,System.Data.DbType> TypeMap { get; }

        /// <summary>
        /// Returns true if the type is a nullable type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsNullableType(Type type);
        
        /// <summary>
        /// Helps execute DDL specific commands for the underlying db engine.
        /// </summary>
        IOrpheusDDLHelper DDLHelper { get; set; }

        /// <summary>
        /// Gets the underlying IDbConnection connection string.
        /// </summary>
        string ConnectionString { get;}

        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        IOrpheusTable<T> CreateTable<T>(IOrpheusTableOptions options);

        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="keyFields"></param>
        /// <returns></returns>
        IOrpheusTable<T> CreateTable<T>(string tableName,List<IOrpheusTableKeyField> keyFields = null);

        /// <summary>
        /// Creates a schema object and sets it's database.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        ISchema CreateSchema(Guid id, string description, double version);

        /// <summary>
        /// Creates an OrpheusModule.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        IOrpheusModule CreateModule(IOrpheusModuleDefinition definition = null);

        /// <summary>
        /// Creates an OrpheusModuleDefinition.
        /// </summary>
        /// <returns></returns>
        IOrpheusModuleDefinition CreateModuleDefinition();
    }
}
