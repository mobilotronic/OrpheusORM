using Microsoft.Extensions.Logging;
using Microsoft.Practices.Unity;
using OrpheusInterfaces;
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
        private Dictionary<Type, System.Data.DbType> typeMap = new Dictionary<Type, DbType>();
        private List<Type> nullableTypes = new List<Type>();
        private IOrpheusDDLHelper ddlHelper;
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
        /// <summary>
        /// State of the database. Connected or not.
        /// </summary>
        public bool Connected
        {
            get
            {
                return dbConnection.State == ConnectionState.Open;
            }
        }

        /// <summary>
        /// List of registered Orpheus modules.
        /// </summary>
        public List<IOrpheusModule> Modules
        {
            get
            {
                return this.modules;
            }
        }
        
        /// <summary>
        /// Is dictionary map between .net data types and DBTypes.
        /// </summary>
        public Dictionary<Type, System.Data.DbType> TypeMap
        {
            get
            {
                return this.typeMap;
            }
        }
        
        /// <summary>
        /// Returns true if the type is in the list of nullable types.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsNullableType(Type type)
        {
            return this.nullableTypes.Contains(type);
        }

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// Gets the underlying IDbConnection connection string.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return this.dbConnection.ConnectionString;
            }
        }
        #endregion

        public OrpheusDatabase(IDbConnection connection, IOrpheusDDLHelper ddlHelper)
        {
            this.dbConnection = connection;
            this.ddlHelper = ddlHelper;
            this.modules = new List<IOrpheusModule>();
            this.logger = OrpheusIocContainer.Resolve<ILogger>();
            this.initializeTypeMap();
        }

        #region public methods

        /// <summary>
        /// Creates an OrpheusModule.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public IOrpheusModule CreateModule(IOrpheusModuleDefinition definition = null)
        {
            return
                OrpheusIocContainer.Resolve<IOrpheusModule>(new ResolverOverride[] {
                new ParameterOverride("database",this),
                new ParameterOverride("definition",definition)
            });
        }

        /// <summary>
        /// Creates an OrpheusModuleDefinition.
        /// </summary>
        /// <returns></returns>
        public IOrpheusModuleDefinition CreateModuleDefinition()
        {
            var result = OrpheusIocContainer.Resolve<IOrpheusModuleDefinition>();
            result.Database = this;
            return result;
        }

        /// <summary>
        /// Creates an OrpheusTable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public IOrpheusTable<T> CreateTable<T>(IOrpheusTableOptions options)
        {
            if(options != null)
            {
                options.Database = this;
                return OrpheusIocContainer.Resolve<IOrpheusTable<T>>(new ResolverOverride[] {  new ParameterOverride("options",options)});
            }
            return null;
        }

        /// <summary>
        /// Creates an OrpheusTable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public IOrpheusTable<T> CreateTable<T>(string tableName,List<IOrpheusTableKeyField> keyFields = null)
        {
            var options = OrpheusIocContainer.Resolve<IOrpheusTableOptions>();
            options.TableName = tableName;
            options.KeyFields = keyFields == null ? new List<IOrpheusTableKeyField>() : keyFields;
            return this.CreateTable<T>(options);
        }

        /// <summary>
        /// Creates a schema object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="description"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public ISchema CreateSchema(Guid id, string description, double version)
        {
            if (id == Guid.Empty)
                id = Guid.NewGuid();
            return OrpheusIocContainer.Resolve<ISchema>(new ResolverOverride[] {
                new ParameterOverride("db",this),
                new ParameterOverride("description",description),
                new ParameterOverride("version",version),
                new ParameterOverride("id",id)
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDbCommand CreateCommand()
        {
            return this.dbConnection.CreateCommand();
        }

        /// <summary>
        /// Returns a prepared query command.
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IDbCommand CreatePreparedQuery(string SQL, List<string> parameters)
        {
            var result = this.CreateCommand();
            result.CommandText = SQL;
            parameters.ForEach(parameter => {
                var param = result.CreateParameter();
                param.ParameterName = parameter.IndexOf("@") >= 0 ? parameter : "@" + parameter;
                result.Parameters.Add(param);
            });
            return result;
        }

        /// <summary>
        /// Returns a prepared query command.
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
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
        public void Connect(string connectionString = null)
        {
            if (!this.Connected)
            {
                if (connectionString != null)
                    this.dbConnection.ConnectionString = connectionString;
                try
                {
                    if (this.ddlHelper.DB == null)
                        this.ddlHelper.DB = this;
                    this.ddlHelper.CreateDatabase();
                    this.dbConnection.Open();
                }
                catch (Exception e)
                {
                    this.logger.LogError(e.Message);
                    throw e;
                }
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
        /// <param name="module"></param>
        public void RegisterModule(IOrpheusModule module)
        {
            this.modules.Add(module);
        }

        /// <summary>
        /// Creates a transaction.
        /// </summary>
        /// <returns></returns>
        public IDbTransaction BeginTransaction()
        {
            return this.dbConnection.BeginTransaction();
        }
        #endregion
    }
}
