using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OrpheusCore.Errors;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;

namespace OrpheusMySQLDDLHelper
{
    /// <summary>
    /// MySQL Server definition of DDL helper.
    /// DDL helper is used to execute DB engine specific DDL commands.
    /// </summary>
    public class OrpheusMySQLServerDDLHelper : IMySQLServerDDLHelper
    {
        private Dictionary<Type, string> typeMap = new Dictionary<Type, string>();
        private Dictionary<int, string> dbTypeMap = new Dictionary<int, string>();
        private IDbCommand selectSchemaObjectTable;
        private IDbCommand selectSchemaObjectConstraint;
        private IDbCommand selectSchemaObjectPrimaryConstraint;
        private MySqlConnection _secondConnection;
        private IOrpheusDatabase db;
        private ILogger<OrpheusMySQLServerDDLHelper> logger;

        private void initializeTypeMap()
        {
            typeMap[typeof(byte)] = "TINYINT";
            typeMap[typeof(sbyte)] = "TINYINT";
            typeMap[typeof(short)] = "SMALLINT";
            typeMap[typeof(ushort)] = "INT";
            typeMap[typeof(int)] = "INT";
            typeMap[typeof(uint)] = "BIGINT";
            typeMap[typeof(long)] = "BIGINT";
            typeMap[typeof(ulong)] = "BIGINT";
            typeMap[typeof(float)] = "FLOAT";
            typeMap[typeof(double)] = "REAL";
            typeMap[typeof(decimal)] = "DECIMAL";
            typeMap[typeof(bool)] = "BOOL";
            typeMap[typeof(string)] = "NVARCHAR";
            typeMap[typeof(char)] = "NCHAR";
            typeMap[typeof(Guid)] = "NVARCHAR(38)";
            typeMap[typeof(DateTime)] = "DATETIME";
            typeMap[typeof(DateTimeOffset)] = "DATETIMEOFFSET";
            typeMap[typeof(byte[])] = "LONGBLOB";

            //nullable types.
            typeMap[typeof(byte?)] = "TINYINT";
            typeMap[typeof(sbyte?)] = "TINYINT";
            typeMap[typeof(short?)] = "SMALLINT";
            typeMap[typeof(ushort?)] = "INT";
            typeMap[typeof(int?)] = "INT";
            typeMap[typeof(uint?)] = "BIGINT";
            typeMap[typeof(long?)] = "BIGINT";
            typeMap[typeof(ulong?)] = "BIGINT";
            typeMap[typeof(float?)] = "FLOAT";
            typeMap[typeof(double?)] = "REAL";
            typeMap[typeof(decimal?)] = "DECIMAL";
            typeMap[typeof(bool?)] = "BOOL";
            typeMap[typeof(char?)] = "NCHAR";
            typeMap[typeof(Guid?)] = "NVARCHAR(38)";
            typeMap[typeof(DateTime?)] = "DATETIME";
            typeMap[typeof(DateTimeOffset?)] = "DATETIMEOFFSET";

            dbTypeMap[(int)DbType.Byte] = "TINYINT";
            dbTypeMap[(int)DbType.Int16] = "SMALLINT";
            dbTypeMap[(int)DbType.Int32] = "INT";
            dbTypeMap[(int)DbType.Int64] = "BIGINT";
            dbTypeMap[(int)DbType.Double] = "FLOAT";
            dbTypeMap[(int)DbType.Single] = "REAL";
            dbTypeMap[(int)DbType.Decimal] = "DECIMAL";
            dbTypeMap[(int)DbType.Boolean] = "BOOL";
            dbTypeMap[(int)DbType.String] = "NVARCHAR";
            dbTypeMap[(int)DbType.AnsiString] = "VARCHAR";
            dbTypeMap[(int)DbType.AnsiStringFixedLength] = "CHAR";
            dbTypeMap[(int)DbType.Guid] = "NVARCHAR(38)";
            dbTypeMap[(int)DbType.DateTime] = "DATETIME";
            dbTypeMap[(int)DbType.DateTimeOffset] = "DATETIMEOFFSET";
            dbTypeMap[(int)DbType.Binary] = "LONGBLOB";
            dbTypeMap[(int)DbType.Time] = "TIME";

            dbTypeMap[(int)ExtendedDbTypes.StringBlob] = "LONGTEXT";
        }

        private MySqlConnection secondConnection
        {
            get
            {
                if (this._secondConnection == null)
                {
                    var sysConnectionConfiguration = this.db.DatabaseConnectionConfiguration;
                    if (sysConnectionConfiguration == null)
                        throw new ArgumentNullException("Missing database configuration from the configuration file.\r\nThis is required so Orpheus can perform database schema related actions.");
                    var connBuilder = new MySqlConnectionStringBuilder();
                    connBuilder.Server = sysConnectionConfiguration.Server;
                    connBuilder.Database = sysConnectionConfiguration.DatabaseName;
                    MySqlSslMode sslMode;
                    if (Enum.TryParse(this.SSLMode, out sslMode))
                    {
                        connBuilder.SslMode = sslMode;
                    }
                    if (sysConnectionConfiguration.ServiceUserName != null)
                        connBuilder.UserID = sysConnectionConfiguration.ServiceUserName;
                    if (sysConnectionConfiguration.ServicePassword != null)
                        connBuilder.Password = sysConnectionConfiguration.ServicePassword;
                    connBuilder.Database = "sys";
                    this._secondConnection = new MySqlConnection(connBuilder.ConnectionString);
                }
                return this._secondConnection;
            }
        }

        /// <summary>
        /// Returns true if the DBEngine supports natively the Guid type.
        /// </summary>
        /// <returns>True if the DBEngine supports natively the Guid type</returns>
        public bool SupportsGuidType { get; private set; }

        /// <summary>
        /// Returns true if the DBEngine supports having schema name spaces. From the currently supported databases, only SQL has this feature.
        /// </summary>
        public bool SupportsSchemaNameSpace { get; private set; }

        /// <summary>
        /// Returns true if a database is successfully created using the underlying db engine settings.
        /// </summary>
        /// <returns>True if database was created successfully</returns>
        public bool CreateDatabase()
        {
            var result = false;
            MySql.Data.MySqlClient.MySqlConnectionStringBuilder connStringBuilder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(this.ConnectionString);
            var dbName = connStringBuilder.Database;
            if (!this.DatabaseExists(dbName))
            {
                this.secondConnection.Open();
                try
                {
                    using (var cmd = this.secondConnection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("CREATE DATABASE {0}", dbName);
                        try
                        {
                            cmd.ExecuteNonQuery();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            result = false;
                            this.logger.LogError(ErrorCodes.ERR_CANNOT_CREATE_DB, e, $"{ErrorDictionary.GetError(ErrorCodes.ERR_CANNOT_CREATE_DB)} {dbName}");
                            throw;
                        }
                    }
                }
                finally
                {
                    this.secondConnection.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if a database is successfully created using the underlying db engine settings.
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>True if the database was created successfully</returns>
        public bool CreateDatabase(string dbName)
        {
            var result = false;
            if (!this.DatabaseExists(dbName))
            {
                this.secondConnection.Open();
                try
                {
                    using (var cmd = this.secondConnection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("CREATE DATABASE {0}", dbName);
                        try
                        {
                            result = cmd.ExecuteNonQuery() > 0;
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(ErrorCodes.ERR_CANNOT_CREATE_DB, e, $"{ErrorDictionary.GetError(ErrorCodes.ERR_CANNOT_CREATE_DB)} {dbName}");
                            throw;
                        }
                    }
                }
                finally
                {
                    this.secondConnection.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if a database is successfully created using the passed DDL script.
        /// </summary>
        /// <param name="ddlString">DDL command</param>
        /// <returns>True if the database was created successfully</returns>
        public bool CreateDatabaseWithDDL(string ddlString)
        {
            var result = false;
            try
            {
                this.secondConnection.Open();
                try
                {
                    using (var cmd = this.secondConnection.CreateCommand())
                    {
                        cmd.CommandText = ddlString;
                        try
                        {
                            result = cmd.ExecuteNonQuery() > 0;
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(ErrorCodes.ERR_CANNOT_CREATE_DB, e, ErrorDictionary.GetError(ErrorCodes.ERR_CANNOT_CREATE_DB));
                            throw;
                        }
                    }
                }
                finally
                {
                    this.secondConnection.Close();
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(ErrorCodes.ERR_CANNOT_CONNECT_TO_DB, e, ErrorDictionary.GetError(ErrorCodes.ERR_CANNOT_CONNECT_TO_DB));
                throw;
            }

            return result;
        }

        /// <summary>
        /// Returns true the database exists.
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>True if the database exists</returns>
        public bool DatabaseExists(string dbName)
        {
            var result = false;
            if (this.secondConnection != null)
            {
                this.secondConnection.Open();
                try
                {
                    using (var cmd = this.secondConnection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME ='{0}'", dbName);
                        try
                        {
                            IDataReader reader = cmd.ExecuteReader();
                            try
                            {
                                if (reader.Read())
                                {
                                    result = reader.GetValue(0) != null;
                                }
                            }
                            finally
                            {
                                if (reader != null)
                                {
                                    reader.Close();
                                    reader.Dispose();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(e, "");
                            throw;
                        }
                    }
                }
                finally
                {
                    this.secondConnection.Close();
                }
            }
            return result;
        }

        /// <summary>
        ///  Returns true if the schema object exists in the database.
        /// </summary>
        /// <param name="schemaConstraint"></param>
        /// <returns></returns>
        public bool SchemaObjectExists(ISchemaConstraint schemaConstraint)
        {
            if (this.secondConnection != null)
            {
                if (this.selectSchemaObjectPrimaryConstraint == null)
                {
                    this.selectSchemaObjectPrimaryConstraint = this.secondConnection.CreateCommand();
                    this.selectSchemaObjectPrimaryConstraint.CommandText = "SELECT DISTINCT TABLE_NAME,COLUMN_NAME FROM INFORMATION_SCHEMA.STATISTICS WHERE INDEX_SCHEMA = @DBNAME AND TABLE_NAME = @TABLE_NAME AND COLUMN_NAME IN (@COLUMN_NAME)";
                    var param = this.selectSchemaObjectPrimaryConstraint.CreateParameter();
                    param.ParameterName = "@DBNAME";
                    this.selectSchemaObjectPrimaryConstraint.Parameters.Add(param);

                    param = this.selectSchemaObjectPrimaryConstraint.CreateParameter();
                    param.ParameterName = "@TABLE_NAME";
                    this.selectSchemaObjectPrimaryConstraint.Parameters.Add(param);

                    param = this.selectSchemaObjectPrimaryConstraint.CreateParameter();
                    param.ParameterName = "@COLUMN_NAME";
                    this.selectSchemaObjectPrimaryConstraint.Parameters.Add(param);
                }

                ((IDataParameter)this.selectSchemaObjectPrimaryConstraint.Parameters["@DBNAME"]).Value = this.DatabaseName;
                ((IDataParameter)this.selectSchemaObjectPrimaryConstraint.Parameters["@TABLE_NAME"]).Value = schemaConstraint.SchemaObject.SQLName;
                ((IDataParameter)this.selectSchemaObjectPrimaryConstraint.Parameters["@COLUMN_NAME"]).Value = string.Join(",", schemaConstraint.Fields.ToArray());
                this.secondConnection.Open();
                IDataReader reader = null;
                try
                {

                    reader = this.selectSchemaObjectPrimaryConstraint.ExecuteReader();
                    if (reader.Read())
                    {
                        return reader.GetValue(0) != null;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    this.secondConnection.Close();
                }
            }

            return false;
        }

        /// <summary>
        ///  Returns true if the schema object exists in the database. A schema object can be a table,view,primary key, stored procedure, etc.
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <returns></returns>
        public bool SchemaObjectExists(ISchemaObject schemaObject)
        {
            return this.SchemaObjectExists(schemaObject.SQLName);
        }

        /// <summary>
        /// Returns true if the schema object exists in the database. A schema object can be a table,view,primary key, stored procedure, etc.
        /// </summary>
        /// <param name="schemaObjectName">Schema object name</param>
        /// <returns>True if the object exists</returns>
        public bool SchemaObjectExists(string schemaObjectName)
        {
            if (this.secondConnection != null)
            {
                if (this.selectSchemaObjectTable == null)
                {
                    this.selectSchemaObjectTable = this.secondConnection.CreateCommand();
                    this.selectSchemaObjectTable.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @NAME";
                    var param = this.selectSchemaObjectTable.CreateParameter();
                    param.ParameterName = "@NAME";
                    this.selectSchemaObjectTable.Parameters.Add(param);
                }
                if (this.selectSchemaObjectConstraint == null)
                {
                    this.selectSchemaObjectConstraint = this.secondConnection.CreateCommand();
                    this.selectSchemaObjectConstraint.CommandText = "SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = @NAME";
                    var param = this.selectSchemaObjectConstraint.CreateParameter();
                    param.ParameterName = "@NAME";
                    this.selectSchemaObjectConstraint.Parameters.Add(param);
                }
                this.secondConnection.Open();
                IDataReader reader = null;
                try
                {
                    ((IDataParameter)this.selectSchemaObjectTable.Parameters["@NAME"]).Value = schemaObjectName;
                    reader = this.selectSchemaObjectTable.ExecuteReader();
                    if (reader.Read())
                    {
                        return reader.GetValue(0) != null;
                    }
                    reader.Close();
                    ((IDataParameter)this.selectSchemaObjectConstraint.Parameters["@NAME"]).Value = schemaObjectName;
                    reader = this.selectSchemaObjectConstraint.ExecuteReader();
                    if (reader.Read())
                    {
                        return reader.GetValue(0) != null;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader.Dispose();
                    }
                    this.secondConnection.Close();
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the schema object, db engine assigned/generated, identifier.
        /// </summary>
        /// <param name="schemaObject">The schema object.</param>
        /// <returns></returns>
        public T SchemaObjectId<T>(ISchemaObject schemaObject)
        {
            return default(T);
        }

        /// <summary>
        /// Database for the DDL helper.
        /// </summary>
        /// <returns>Database the helper is associated with</returns>
        public IOrpheusDatabase DB
        {
            get { return this.db; }
            set
            {
                this.db = value;
            }
        }

        /// <summary>
        /// Returns the db engine specific string equivalent, for a .net type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>String value for the mapped DbType</returns>
        public string TypeToString(Type type)
        {
            if (this.typeMap.ContainsKey(type))
            {
                return this.typeMap[type];
            }

            return null;
        }

        /// <summary>
        /// Returns the db engine specific string equivalent, for a DbType enumeration.
        /// </summary>
        /// <param name="dataType">DbType</param>
        /// <returns>String value for the DbType</returns>
        public string DbTypeToString(DbType dataType)
        {
            if (this.dbTypeMap.ContainsKey((int)dataType))
            {
                return this.dbTypeMap[(int)dataType];
            }

            return null;
        }

        /// <summary>
        /// Identifiers that do not comply with all of the rules for identifiers must be delimited in a SQL statement, enclosed in the DelimitedIdentifier char.
        /// </summary>
        /// <returns>Char</returns>
        public char DelimitedIndetifierStart { get { return '`'; } }

        /// <summary>
        /// Identifiers that do not comply with all of the rules for identifiers must be delimited in a SQL statement, enclosed in the DelimitedIdentifier char.
        /// </summary>
        /// <returns>Char</returns>
        public char DelimitedIndetifierEnd { get { return '`'; } }

        /// <summary>
        /// Properly formats a field name, to be used in a SQL statement, in case the field name is a reserved word.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string SafeFormatField(string fieldName) { return String.Format("{0}{1}{2}", this.DelimitedIndetifierStart, fieldName, this.DelimitedIndetifierEnd); }

        /// <summary>
        /// Returns the DB specific modify table command.
        /// </summary>
        public string ModifyColumnCommand { get { return " MODIFY COLUMN "; } }

        /// <summary>
        /// Returns the underlying database engine type.
        /// </summary>
        public DatabaseEngineType DbEngineType { get; private set; }

        /// <summary>
        /// Properly formats an ALTER TABLE DROP COLUMN command for the underlying database engine.
        /// </summary>
        /// <param name="tableName">Table's name that schema is going to change</param>
        /// <param name="columnsToDelete">Columns for deletion</param>
        /// <returns></returns>
        public string SafeFormatAlterTableDropColumn(string tableName, List<string> columnsToDelete)
        {
            return String.Format("ALTER TABLE {0} DROP COLUMN {1}", tableName, string.Join(", DROP ", columnsToDelete.ToArray()));
        }

        /// <summary>
        /// Properly formats an ALTER TABLE ADD COLUMN command for the underlying database engine.
        /// </summary>
        /// <param name="tableName">Table's name that schema is going to change</param>
        /// <param name="columnsToAdd">Columns for creation</param>
        /// <returns></returns>
        public string SafeFormatAlterTableAddColumn(string tableName, List<string> columnsToAdd)
        {
            return String.Format("ALTER TABLE {0} ADD {1}", tableName, string.Join(", ADD ", columnsToAdd.ToArray()));
        }

        /// <summary>
        /// Gets the database name.
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return this.db.DatabaseConnectionConfiguration.DatabaseName;
            }
        }


        /// <summary>
        /// Builds the connection string.
        /// </summary>
        /// <returns></returns>
        public string ConnectionString
        {
            get
            {
                var dataConnectionConfiguration = this.db.DatabaseConnectionConfiguration;
                if (dataConnectionConfiguration == null)
                    throw new ArgumentNullException("Missing database configuration.\r\nThis is required so Orpheus can connect to the database.");

                var connBuilder = new MySqlConnectionStringBuilder();
                connBuilder.Server = dataConnectionConfiguration.Server;
                connBuilder.Database = dataConnectionConfiguration.DatabaseName;

                MySqlSslMode sslMode;
                switch (dataConnectionConfiguration.EncyrptConnection)
                {
                    case OrpheusInterfaces.Configuration.EncyrptConnection.ecOptional:
                        {
                            connBuilder.SslMode = MySqlSslMode.Preferred; break;
                        }
                    case OrpheusInterfaces.Configuration.EncyrptConnection.ecMandatory:
                        {
                            connBuilder.SslMode = MySqlSslMode.Required; break;
                        }
                    case OrpheusInterfaces.Configuration.EncyrptConnection.ecStrict:
                        {
                            connBuilder.SslMode = MySqlSslMode.VerifyFull; break;
                        }
                    default:
                        {
                            if (Enum.TryParse(this.SSLMode, out sslMode))
                            {
                                connBuilder.SslMode = sslMode;
                            }
                            break;
                        }
                }


                if (dataConnectionConfiguration.UserName != null)
                    connBuilder.UserID = dataConnectionConfiguration.UserName;
                if (dataConnectionConfiguration.Password != null)
                    connBuilder.Password = dataConnectionConfiguration.Password;

                return connBuilder.ConnectionString;
            }
        }

        /// <summary>
        /// SSL connection mode.
        /// </summary>
        public string SSLMode { get; set; }

        /// <summary>
        /// MySQL Server DDL helper constructor.
        /// </summary>
        public OrpheusMySQLServerDDLHelper(ILogger<OrpheusMySQLServerDDLHelper> logger)
        {
            this.initializeTypeMap();
            this.SupportsGuidType = false;
            this.SupportsSchemaNameSpace = false;
            this.DbEngineType = DatabaseEngineType.dbMySQL;
            this.SSLMode = MySqlSslMode.Required.ToString();
            this.logger = logger;
        }
    }
}
