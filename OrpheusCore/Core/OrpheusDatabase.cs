using Microsoft.Extensions.Logging;
using OrpheusCore.Configuration;
using OrpheusCore.Errors;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;


namespace OrpheusCore
{
    /// <summary> 
    /// Orpheus database.
    /// </summary>
    public class OrpheusDatabase : IOrpheusDatabase
    {
        #region private properties
        private IDbConnection dbConnection;
        private List<IOrpheusModule> modules;
        private ILogger logger;
        private ILoggerFactory loggerFactory;
        private Dictionary<Type, System.Data.DbType> typeMap = new Dictionary<Type, DbType>();
        private List<Type> nullableTypes = new List<Type>();
        private IOrpheusDDLHelper ddlHelper;
        private IDbCommand rowCountCommand;
        private IDatabaseConnectionConfiguration databaseConnectionConfiguration;
        #endregion

        #region private methods
        private void initializeTypeMap()
        {
            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(byte[])] = DbType.Binary;
            //nullable types
            typeMap[typeof(byte?)] = DbType.Byte;
            nullableTypes.Add(typeof(byte?));
            typeMap[typeof(sbyte?)] = DbType.SByte;
            nullableTypes.Add(typeof(sbyte?));
            typeMap[typeof(short?)] = DbType.Int16;
            nullableTypes.Add(typeof(short?));
            typeMap[typeof(ushort?)] = DbType.UInt16;
            nullableTypes.Add(typeof(ushort?));
            typeMap[typeof(int?)] = DbType.Int32;
            nullableTypes.Add(typeof(int?));
            typeMap[typeof(uint?)] = DbType.UInt32;
            nullableTypes.Add(typeof(uint?));
            typeMap[typeof(long?)] = DbType.Int64;
            nullableTypes.Add(typeof(long?));
            typeMap[typeof(ulong?)] = DbType.UInt64;
            nullableTypes.Add(typeof(ulong?));
            typeMap[typeof(float?)] = DbType.Single;
            nullableTypes.Add(typeof(float?));
            typeMap[typeof(double?)] = DbType.Double;
            nullableTypes.Add(typeof(double?));
            typeMap[typeof(decimal?)] = DbType.Decimal;
            nullableTypes.Add(typeof(decimal?));
            typeMap[typeof(bool?)] = DbType.Boolean;
            nullableTypes.Add(typeof(bool?));
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            nullableTypes.Add(typeof(char?));
            typeMap[typeof(Guid?)] = DbType.Guid;
            nullableTypes.Add(typeof(Guid?));
            typeMap[typeof(DateTime?)] = DbType.DateTime;
            nullableTypes.Add(typeof(DateTime?));
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            nullableTypes.Add(typeof(DateTimeOffset?));
            //typeMap[typeof(System.Data.Linq.Binary)] = DbType.Binary;
        }

        private List<string> createParametersList(string SQL)
        {
            //http://stackoverflow.com/questions/5877084/regex-to-match-whole-words-that-begin-with
            var result = new List<string>();
            Regex reg = new Regex(@"\@(\w+)",RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            foreach(Match match in reg.Matches(SQL))
            {
                result.Add(match.Value);
            }
            return result;
        }
        #endregion

        #region public properties                
        /// <value>
        /// State of the database. Connected or not.
        /// </value>
        public bool Connected
        {
            get
            {
                return dbConnection.State == ConnectionState.Open;
            }
        }

        /// <value>
        /// List of registered Orpheus modules.
        /// </value>
        public List<IOrpheusModule> Modules
        {
            get
            {
                return this.modules;
            }
        }

        /// <value>
        /// Mapping dictionary of types to data types.
        /// </value>
        public Dictionary<Type, System.Data.DbType> TypeMap
        {
            get
            {
                return this.typeMap;
            }
        }

        /// <summary>
        /// Returns true if the type is a nullable type.
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>
        /// True if type is nullable
        /// </returns>
        public bool IsNullableType(Type type)
        {
            return this.nullableTypes.Contains(type);
        }

        /// <value>
        /// Helps execute DDL specific commands for the underlying db engine.
        /// </value>
        public IOrpheusDDLHelper DDLHelper
        {
            get
            {
                return this.ddlHelper;
            }
            set
            {
                this.ddlHelper = value;
            }
        }

        /// <value>
        /// Gets the underlying IDbConnection connection string.
        /// </value>
        public string ConnectionString
        {
            get
            {
                return this.dbConnection.ConnectionString;
            }
        }

        /// <value>
        /// Last active transaction.
        /// </value>
        public IDbTransaction LastActiveTransaction { get; private set; }

        /// <value>
        /// Exposing the underlying IDbConnection instance.
        /// </value>
        public IDbConnection DbConnection { get { return this.dbConnection; } }

        /// <value>
        /// Database connection configuration.
        /// </value>
        /// <exception cref="ArgumentNullException">The database connection configuration cannot be null.</exception>
        public IDatabaseConnectionConfiguration DatabaseConnectionConfiguration
        {
            get
            {
                return this.databaseConnectionConfiguration;
            }
            set
            {
                this.databaseConnectionConfiguration = value;
                if (this.databaseConnectionConfiguration == null)
                    throw new ArgumentNullException("The database connection configuration cannot be null.");
            }
        }

        /// <value>
        /// Logger instance.
        /// </value>
        public ILogger Logger => this.logger;
        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        /// <value>
        /// The logger factory.
        /// </value>
        public ILoggerFactory LoggerFactory => this.loggerFactory;
        #endregion

        #region constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrpheusDatabase"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="ddlHelper">The DDL helper.</param>
        /// <param name="loggerFactory">The logger.</param>
        public OrpheusDatabase(IDbConnection connection, IOrpheusDDLHelper ddlHelper, ILoggerFactory loggerFactory)
        {
            this.dbConnection = connection;
            this.ddlHelper = ddlHelper;
            this.ddlHelper.DB = this;
            this.modules = new List<IOrpheusModule>();
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<OrpheusDatabase>();
            this.initializeTypeMap();
        }
        #endregion

        #region public methods

        /// <summary>
        /// Creates an OrpheusModule.
        /// </summary>
        /// <param name="definition">Module definition</param>
        /// <returns>
        /// An IOrpheusModule instance
        /// </returns>
        public IOrpheusModule CreateModule(IOrpheusModuleDefinition definition = null)
        {
            return ConfigurationManager.Resolve<IOrpheusModule>(new object[] { this, definition }); // new OrpheusModule(this, definition);
        }

        /// <summary>
        /// Creates an OrpheusTableOptions.
        /// </summary>
        /// <returns>
        /// An IOrpheusTableOptions instance.
        /// </returns>
        public IOrpheusTableOptions CreateTableOptions()
        {
            return ConfigurationManager.Resolve<IOrpheusTableOptions>();
        }

        /// <summary>
        /// Creates an OrpheusTableKeyField.
        /// </summary>
        /// <returns>
        /// An IOrpheusTableKeyField instance.
        /// </returns>
        public IOrpheusTableKeyField CreateTableKeyField()
        {
            return ConfigurationManager.Resolve<IOrpheusTableKeyField>();
        }

        /// <summary>
        /// Creates an OrpheusModuleDefinition.
        /// </summary>
        /// <returns>
        /// An IOrpheusModuleDefinition instance.
        /// </returns>
        public IOrpheusModuleDefinition CreateModuleDefinition()
        {
            var result = ConfigurationManager.Resolve<IOrpheusModuleDefinition>();
            result.Database = this;
            return result;
        }


        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T">Model type for table</typeparam>
        /// <param name="options">Table options</param>
        /// <returns>
        /// An Orpheus table instance.
        /// </returns>
        public IOrpheusTable<T> CreateTable<T>(IOrpheusTableOptions options)
        {
            if(options != null)
            {
                options.Database = this;
                return new OrpheusTable<T>(options);
            }
            return null;
        }


        /// <summary>
        /// Creates a table and sets its database.
        /// </summary>
        /// <typeparam name="T">Model type for the table</typeparam>
        /// <param name="tableName">Table name</param>
        /// <param name="keyFields">Table key fields</param>
        /// <returns>
        /// An Orpheus table instance.
        /// </returns>
        public IOrpheusTable<T> CreateTable<T>(string tableName,List<IOrpheusTableKeyField> keyFields = null)
        {
            var options = this.CreateTableOptions();
            options.TableName = tableName;
            options.KeyFields = keyFields == null ? new List<IOrpheusTableKeyField>() : keyFields;
            return this.CreateTable<T>(options);
        }

        /// <summary>
        /// Creates a table and sets its database,using the type name as the table name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// An Orpheus table instance.
        /// </returns>
        public IOrpheusTable<T> CreateTable<T>()
        {
            var tableName = typeof(T).Name;
            return this.CreateTable<T>(tableName);
        }

        /// <summary>
        /// Creates a schema object and sets it's database.
        /// </summary>
        /// <param name="id">Schema id</param>
        /// <param name="description">Schema description</param>
        /// <param name="version">Schema version</param>
        /// <param name="name">Schema name.From the supported db engines, only SQL server has support for named schemas.</param>
        /// <returns>
        /// An ISchema instance.
        /// </returns>
        public ISchema CreateSchema(Guid id, string description, double version, string name = null)
        {
            if (id == Guid.Empty)
                id = Guid.NewGuid();
            //return new SchemaBuilder.Schema(this,description, version, id, name);
            return ConfigurationManager.Resolve<ISchema>(new object[] { this, description, version, id, name });
        }

        /// <summary>
        /// Create a DbCommand.
        /// </summary>
        /// <returns>
        /// A DbCommand instance.
        /// </returns>
        public IDbCommand CreateCommand()
        {
            return this.dbConnection.CreateCommand();
        }

        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL">SQL for the prepared query</param>
        /// <param name="parameters">SQL parameters</param>
        /// <param name="parameterValues">SQL parameter values</param>
        /// <returns>
        /// A DbCommand instance.
        /// </returns>
        public IDbCommand CreatePreparedQuery(string SQL, List<string> parameters, List<object> parameterValues = null)
        {
            var result = this.CreateCommand();
            result.CommandText = SQL;
            for(var i=0;i<=parameters.Count - 1; i++)
            {
                var parameter = parameters[i];

                var param = result.CreateParameter();
                param.ParameterName = parameter.IndexOf("@") >= 0 ? parameter : "@" + parameter;
                if(parameterValues != null)
                {
                    param.Value = parameterValues[i];
                }
                result.Parameters.Add(param);
            }
            return result;
        }

        /// <summary>
        /// Returns a prepared query with parameters created.
        /// </summary>
        /// <param name="SQL">SQL for the prepared query</param>
        /// <returns>
        /// A DbCommand instance.
        /// </returns>
        public IDbCommand CreatePreparedQuery(string SQL)
        {
            var result = this.CreateCommand();
            var parameters = this.createParametersList(SQL);
            result.CommandText = SQL;
            parameters.ForEach(parameter => {
                var param = result.CreateParameter();
                param.ParameterName = parameter.IndexOf("@") >= 0 ? parameter : "@" + parameter;
                result.Parameters.Add(param);
            });
            return result;
        }

        /// <summary>
        /// Connects to the database engine defined in the connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public void Connect(string connectionString = null)
        {
            if (!this.Connected)
            {
                try
                {
                    if (this.ddlHelper.DB == null)
                        this.ddlHelper.DB = this;
                    try
                    {
                        this.ddlHelper.CreateDatabase();
                    }
                    catch(Exception e)
                    {
                        this.logger.LogError(ErrorCodes.ERR_CANNOT_CREATE_DB,e, ErrorDictionary.GetError(ErrorCodes.ERR_CANNOT_CREATE_DB));
                        throw;
                    }
                    if (!String.IsNullOrEmpty(connectionString))
                        this.dbConnection.ConnectionString = connectionString;
                    else
                        this.dbConnection.ConnectionString = this.ddlHelper.ConnectionString;
                    this.dbConnection.Open();
                }
                catch (Exception e)
                {
                    this.logger.LogError(ErrorCodes.ERR_CANNOT_CONNECT_TO_DB,e, ErrorDictionary.GetError(ErrorCodes.ERR_CANNOT_CONNECT_TO_DB));
                    throw;
                }
            }
        }

        /// <summary>
        /// Connects to the database engine defined in the configuration object.
        /// </summary>
        /// <param name="databaseConnectionConfiguration"></param>
        public void Connect(IDatabaseConnectionConfiguration databaseConnectionConfiguration)
        {
            if (!this.Connected)
            {
                this.DatabaseConnectionConfiguration = databaseConnectionConfiguration;
                this.Connect();
            }
        }

        /// <summary>
        /// Disconnects from the database engine.
        /// </summary>
        public void Disconnect()
        {
            this.dbConnection.Close();
        }

        /// <summary>
        /// Register an Orpheus module to the database.
        /// </summary>
        /// <param name="module">Module to be registered</param>
        public void RegisterModule(IOrpheusModule module)
        {
            this.modules.Add(module);
        }

        /// <summary>
        /// Creates a transaction object.
        /// </summary>
        /// <returns>
        /// Returns a transaction instance
        /// </returns>
        public IDbTransaction BeginTransaction()
        {
            this.LastActiveTransaction = this.dbConnection.BeginTransaction();
            return this.LastActiveTransaction;
        }

        /// <summary>
        /// Commits a transaction.
        /// </summary>
        /// <param name="transaction">Transaction to be committed.</param>
        public void CommitTransaction(IDbTransaction transaction)
        {
            transaction.Commit();
            if(transaction == this.LastActiveTransaction)
            {
                this.LastActiveTransaction = null;
                transaction.Dispose();
            }
        }

        /// <summary>
        /// Rolls back a transaction.
        /// </summary>
        /// <param name="transaction">Transaction to be rolled-back.</param>
        public void RollbackTransaction(IDbTransaction transaction)
        {
            transaction.Rollback();
            if (transaction == this.LastActiveTransaction)
            {
                this.LastActiveTransaction = null;
                transaction.Dispose();
            }
        }

        /// <summary>
        /// Executes a SQL statement and returns it as specific model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL">SQL command to execute.</param>
        /// <param name="tableName">Table name.</param>
        /// <returns>
        /// A list of 'T'
        /// </returns>
        public List<T> SQL<T>(string SQL,string tableName = null)
        {
            tableName = tableName == null ? typeof(T).Name : tableName;
            var table = this.CreateTable<T>(tableName);
            table.Load(SQL);
            return table.Data;
        }

        /// <summary>
        /// Executes a db command and returns it as specific model.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbCommand">DbCommand to run.</param>
        /// <param name="tableName">Optionally set the table name, for which the query will run.</param>
        /// <returns>
        /// A list of 'T'
        /// </returns>
        public List<T> SQL<T>(IDbCommand dbCommand, string tableName = null)
        {
            tableName = tableName == null ? typeof(T).Name : tableName;
            var table = this.CreateTable<T>(tableName);
            table.Load(dbCommand);
            return table.Data;
        }

        /// <summary>
        /// Casts the DDL helper to the specified type.
        /// </summary>
        /// <typeparam name="T">Type in which to cast the DDLHelper. Must be a descendant of IOrpheusDDLHelper.</typeparam>
        /// <returns>
        /// An instance of T.
        /// </returns>
        public T DDLHelperAs<T>()
        {
            return (T)this.ddlHelper;
        }

        /// <summary>
        /// Executes a DDL command.
        /// </summary>
        /// <param name="DDLCommand">DbCommand to run.</param>
        /// <returns>
        /// True if command was successfully executed.
        /// </returns>
        public bool ExecuteDDL(string DDLCommand)
        {
            var result = false;
            var cmd = this.CreateCommand();
            cmd.CommandText = DDLCommand;
            try
            {
                cmd.ExecuteNonQuery();
                result = true;
            }
            catch(Exception e)
            {
                this.logger.LogError(ErrorCodes.ERR_CANNOT_RUN_DDL,e, $"{ErrorDictionary.GetError(ErrorCodes.ERR_CANNOT_RUN_DDL)} | {DDLCommand}");
            }
            finally
            {
                cmd.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Returns the row count of a table.
        /// </summary>
        /// <param name="tableName">The table name.</param>
        /// <returns></returns>
        public long GetTableCount(string tableName)
        {
            if(this.rowCountCommand == null)
            {
                this.rowCountCommand = this.CreateCommand();
            }
            this.rowCountCommand.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            var reader = this.rowCountCommand.ExecuteReader();
            try
            {
                if (reader.Read())
                    return Convert.ToInt64(reader.GetValue(0));
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }
            return 0;
        }

        /// <summary>
        /// Returns the row count of a table.
        /// </summary>
        /// <typeparam name="T">The table type.</typeparam>
        /// <returns></returns>
        public long GetTableCount<T>()
        {
            return this.GetTableCount(typeof(T).Name);
        }
        #endregion
    }
}
