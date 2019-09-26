using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// Orpheus database access component.
    /// </summary>
    public interface IOrpheusDatabase
    {
        /// <summary>
        /// Connects to the database engine defined in the connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        void Connect(string connectionString = null);

        /// <summary>
        /// Connects to the database engine defined in the configuration object.
        /// </summary>
        /// <param name="databaseConnectionConfiguration">The database connection configuration.</param>
        void Connect(IDatabaseConnectionConfiguration databaseConnectionConfiguration);

        /// <summary>
        /// Disconnects from the database engine.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// State of the database. Connected or not.
        /// </summary>
        /// <returns>True if database is connected</returns>
        bool Connected { get; }

        /// <value>
        /// Last active transaction.
        /// </value>
        IDbTransaction LastActiveTransaction { get; }

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
        /// Commits a transaction.
        /// </summary>
        /// <param name="transaction">Transaction to be committed.</param>
        void CommitTransaction(IDbTransaction transaction);

        /// <summary>
        /// Rolls back a transaction.
        /// </summary>
        /// <param name="transaction">Transaction to be rolled-back.</param>
        void RollbackTransaction(IDbTransaction transaction);

        /// <summary>
        /// Create a DbCommand.
        /// </summary>
        /// <returns>A DbCommand instance.</returns>
        IDbCommand CreateCommand();

        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL">SQL for the prepared query</param>
        /// <param name="parameters">SQL parameters</param>
        /// <param name="parameterValues">SQL parameter values</param>
        /// <returns>A DbCommand instance.</returns>
        IDbCommand CreatePreparedQuery(string SQL, List<string> parameters, List<object> parameterValues = null);

        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL">SQL for the prepared query</param>
        /// <returns>A DbCommand instance.</returns>
        IDbCommand CreatePreparedQuery(string SQL);

        /// <summary>
        /// Mapping dictionary of types to data types.
        /// </summary>
        /// <returns>Type map dictionary between types and DbType.</returns>
        Dictionary<Type, System.Data.DbType> TypeMap { get; }

        /// <summary>
        /// Returns true if the type is a nullable type.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>True if type is nullable</returns>
        bool IsNullableType(Type type);

        /// <summary>
        /// Helps execute DDL specific commands for the underlying db engine.
        /// </summary>
        /// <returns>An OrpheusDDLHelper instance.</returns>
        IOrpheusDDLHelper DDLHelper { get; set; }

        /// <summary>
        /// Casts the DDL helper to the specified type.
        /// </summary>
        /// <typeparam name="T">Type in which to cast the DDLHelper. Must be a descendant of IOrpheusDDLHelper.</typeparam>
        /// <returns>An instance of T.</returns>
        T DDLHelperAs<T>();

        /// <summary>
        /// Gets the underlying IDbConnection connection string.
        /// </summary>
        /// <returns>The database connection string.</returns>
        string ConnectionString { get; }

        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T">Model type for table</typeparam>
        /// <param name="options">Table options</param>
        /// <returns>An Orpheus table instance.</returns>
        IOrpheusTable<T> CreateTable<T>(IOrpheusTableOptions options);

        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T">Model type for the table</typeparam>
        /// <param name="tableName">Table name</param>
        /// <param name="keyFields">Table key fields</param>
        /// <returns>An Orpheus table instance.</returns>
        IOrpheusTable<T> CreateTable<T>(string tableName, List<IOrpheusTableKeyField> keyFields = null);

        /// <summary>
        /// Creates a table and sets its database,using the type name as the table name.
        /// </summary>
        /// <typeparam name="T">Table model type.</typeparam>
        /// <returns></returns>
        IOrpheusTable<T> CreateTable<T>();

        /// <summary>
        /// Creates a schema object and sets it's database.
        /// </summary>
        /// <param name="id">Schema id</param>
        /// <param name="description">Schema description</param>
        /// <param name="version">Schema version</param>
        /// <param name="name">Schema name.From the supported db engines, only SQL server has support for named schema's.</param>
        /// <returns>An ISchema instance.</returns>
        ISchema CreateSchema(Guid id, string description, double version, string name = null);

        /// <summary>
        /// Creates an OrpheusModule.
        /// </summary>
        /// <param name="definition">Module definition</param>
        /// <returns>An IOrpheusModule instance</returns>
        IOrpheusModule CreateModule(IOrpheusModuleDefinition definition = null);

        /// <summary>
        /// Creates an OrpheusModuleDefinition.
        /// </summary>
        /// <returns>An IOrpheusModuleDefinition instance.</returns>
        IOrpheusModuleDefinition CreateModuleDefinition();

        /// <summary>
        /// Creates an OrpheusTableOptions.
        /// </summary>
        /// <returns>An IOrpheusTableOptions instance.</returns>
        IOrpheusTableOptions CreateTableOptions();

        /// <summary>
        /// Creates an OrpheusTableKeyField.
        /// </summary>
        /// <returns>An IOrpheusTableKeyField instance.</returns>
        IOrpheusTableKeyField CreateTableKeyField();

        /// <summary>
        /// Executes a SQL statement and returns it as specific model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL">SQL command to execute.</param>
        /// <param name="tableName">Table name.</param>
        /// <returns>A list of 'T'</returns>
        List<T> SQL<T>(string SQL, string tableName = null);

        /// <summary>
        /// Executes a db command and returns it as specific model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbCommand">DbCommand to run.</param>
        /// <param name="tableName">Optionally set the table name, for which the query will run.</param>
        /// <returns>A list of 'T'</returns>
        List<T> SQL<T>(IDbCommand dbCommand, string tableName = null);

        /// <summary>
        /// Executes a DDL command.
        /// </summary>
        /// <param name="DDLCommand">DbCommand to run.</param>
        /// <returns>True if command was successfully executed.</returns>
        bool ExecuteDDL(string DDLCommand);

        /// <value>
        /// Exposing the underlying IDbConnection instance.
        /// </value>
        IDbConnection DbConnection { get; }

        /// <value>
        /// Returns the row count of a table.
        /// </value>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        long GetTableCount(string tableName);

        /// <value>
        /// Returns the row count of a table.
        /// </value>
        /// <typeparam name="T">The table type name, will be used as the table name.</typeparam>
        /// <returns></returns>
        long GetTableCount<T>();

        /// <value>
        /// Database connection configuration.
        /// </value>
        IDatabaseConnectionConfiguration DatabaseConnectionConfiguration { get; set; }
    }
}
