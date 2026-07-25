using Microsoft.Extensions.Logging;
using Npgsql;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OrpheusPostgreSQLDDLHelper
{
    /// <summary>
    /// PostgreSQL DDL helper — generates and executes PostgreSQL-specific DDL commands.
    /// </summary>
    public class OrpheusPostgreSQLDDLHelper : IOrpheusDDLHelper
    {
        #region private fields
        private Dictionary<Type, string> typeMap = new();
        private Dictionary<int, string> dbTypeMap = new();
        private IOrpheusDatabase db;
        private ILogger logger;
        private NpgsqlConnection secondConnection;
        private NpgsqlConnection masterConnection;
        #endregion

        #region auxiliary connections
        private NpgsqlConnection SecondConnection
        {
            get
            {
                if (secondConnection == null)
                    secondConnection = new NpgsqlConnection(ConnectionString);
                return secondConnection;
            }
        }

        private NpgsqlConnection MasterConnection
        {
            get
            {
                if (masterConnection == null)
                {
                    var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
                    builder.Database = "postgres"; // system database for CREATE DATABASE
                    masterConnection = new NpgsqlConnection(builder.ConnectionString);
                }
                return masterConnection;
            }
        }
        #endregion

        #region constructors
        public OrpheusPostgreSQLDDLHelper(ILogger<OrpheusPostgreSQLDDLHelper> logger)
        {
            this.logger = logger;
            initializeTypeMap();
        }
        #endregion

        #region type mapping
        private void initializeTypeMap()
        {
            // .NET type → PostgreSQL type
            typeMap[typeof(byte)] = "SMALLINT";
            typeMap[typeof(sbyte)] = "SMALLINT";
            typeMap[typeof(short)] = "SMALLINT";
            typeMap[typeof(ushort)] = "INTEGER";
            typeMap[typeof(int)] = "INTEGER";
            typeMap[typeof(uint)] = "BIGINT";
            typeMap[typeof(long)] = "BIGINT";
            typeMap[typeof(ulong)] = "NUMERIC(20)";
            typeMap[typeof(float)] = "REAL";
            typeMap[typeof(double)] = "DOUBLE PRECISION";
            typeMap[typeof(decimal)] = "NUMERIC";
            typeMap[typeof(bool)] = "BOOLEAN";
            typeMap[typeof(string)] = "VARCHAR";
            typeMap[typeof(char)] = "CHAR(1)";
            typeMap[typeof(Guid)] = "UUID";
            typeMap[typeof(DateTime)] = "TIMESTAMP";
            typeMap[typeof(DateTimeOffset)] = "TIMESTAMPTZ";
            typeMap[typeof(byte[])] = "BYTEA";

            // Nullable types
            typeMap[typeof(byte?)] = "SMALLINT";
            typeMap[typeof(sbyte?)] = "SMALLINT";
            typeMap[typeof(short?)] = "SMALLINT";
            typeMap[typeof(ushort?)] = "INTEGER";
            typeMap[typeof(int?)] = "INTEGER";
            typeMap[typeof(uint?)] = "BIGINT";
            typeMap[typeof(long?)] = "BIGINT";
            typeMap[typeof(ulong?)] = "NUMERIC(20)";
            typeMap[typeof(float?)] = "REAL";
            typeMap[typeof(double?)] = "DOUBLE PRECISION";
            typeMap[typeof(decimal?)] = "NUMERIC";
            typeMap[typeof(bool?)] = "BOOLEAN";
            typeMap[typeof(char?)] = "CHAR(1)";
            typeMap[typeof(Guid?)] = "UUID";
            typeMap[typeof(DateTime?)] = "TIMESTAMP";
            typeMap[typeof(DateTimeOffset?)] = "TIMESTAMPTZ";

            // DbType mapping
            dbTypeMap[(int)DbType.Byte] = "SMALLINT";
            dbTypeMap[(int)DbType.Int16] = "SMALLINT";
            dbTypeMap[(int)DbType.Int32] = "INTEGER";
            dbTypeMap[(int)DbType.Int64] = "BIGINT";
            dbTypeMap[(int)DbType.Double] = "DOUBLE PRECISION";
            dbTypeMap[(int)DbType.Single] = "REAL";
            dbTypeMap[(int)DbType.Decimal] = "NUMERIC";
            dbTypeMap[(int)DbType.Boolean] = "BOOLEAN";
            dbTypeMap[(int)DbType.String] = "VARCHAR";
            dbTypeMap[(int)DbType.AnsiString] = "VARCHAR";
            dbTypeMap[(int)DbType.AnsiStringFixedLength] = "CHAR";
            dbTypeMap[(int)DbType.StringFixedLength] = "CHAR";
            dbTypeMap[(int)DbType.Guid] = "UUID";
            dbTypeMap[(int)DbType.DateTime] = "TIMESTAMP";
            dbTypeMap[(int)DbType.DateTimeOffset] = "TIMESTAMPTZ";
            dbTypeMap[(int)DbType.Binary] = "BYTEA";
            dbTypeMap[(int)DbType.SByte] = "SMALLINT";
            dbTypeMap[(int)DbType.UInt16] = "INTEGER";
            dbTypeMap[(int)DbType.UInt32] = "BIGINT";
            dbTypeMap[(int)DbType.UInt64] = "NUMERIC(20)";
        }
        #endregion

        #region IOrpheusDDLHelper implementation

        public IOrpheusDatabase DB { get; set; }

        public char DelimitedIdentifierStart => '"';
        public char DelimitedIdentifierEnd => '"';

        public bool SupportsGuidType => true;
        public bool SupportsSchemaNameSpace => true; // PostgreSQL schemas

        public string DatabaseName
        {
            get
            {
                var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
                return builder.Database;
            }
        }

        public DatabaseEngineType DbEngineType => DatabaseEngineType.dbPostgreSQL;

        public string ConnectionString => DB?.ConnectionString ?? string.Empty;

        public string ModifyColumnCommand => "ALTER COLUMN";

        #endregion

        #region type conversion
        public string TypeToString(Type type)
        {
            if (typeMap.TryGetValue(type, out var result))
                return result;
            return "VARCHAR";
        }

        public string DbTypeToString(DbType dataType)
        {
            if (dbTypeMap.TryGetValue((int)dataType, out var result))
                return result;
            return "VARCHAR";
        }
        #endregion

        #region database operations
        public bool DatabaseExists(string dbName)
        {
            try
            {
                using var cmd = MasterConnection.CreateCommand();
                cmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{dbName}'";
                MasterConnection.Open();
                var result = cmd.ExecuteScalar();
                return result != null;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (MasterConnection.State == ConnectionState.Open)
                    MasterConnection.Close();
            }
        }

        public bool CreateDatabase()
        {
            return CreateDatabase(DatabaseName);
        }

        public bool CreateDatabase(string dbName)
        {
            try
            {
                if (DatabaseExists(dbName))
                    return true;

                MasterConnection.Open();
                using var cmd = MasterConnection.CreateCommand();
                cmd.CommandText = $"CREATE DATABASE \"{dbName}\"";
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create database {0}", dbName);
                return false;
            }
            finally
            {
                if (MasterConnection.State == ConnectionState.Open)
                    MasterConnection.Close();
            }
        }

        public bool CreateDatabaseWithDDL(string ddlString)
        {
            try
            {
                using var cmd = SecondConnection.CreateCommand();
                SecondConnection.Open();
                cmd.CommandText = ddlString;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to execute DDL: {0}", ddlString);
                return false;
            }
            finally
            {
                if (SecondConnection.State == ConnectionState.Open)
                    SecondConnection.Close();
            }
        }
        #endregion

        #region schema object operations
        public bool SchemaObjectExists(ISchemaObject schemaObject)
        {
            return SchemaObjectExists(schemaObject.SQLName);
        }

        public bool SchemaObjectExists(string schemaObjectName)
        {
            try
            {
                using var cmd = SecondConnection.CreateCommand();
                SecondConnection.Open();
                // PostgreSQL: check information_schema.tables
                var parts = schemaObjectName.Split('.');
                var schema = parts.Length > 1 ? parts[0] : "public";
                var table = parts.Length > 1 ? parts[1] : parts[0];
                cmd.CommandText = $"SELECT 1 FROM information_schema.tables WHERE table_schema = '{schema}' AND table_name = '{table}'";
                var result = cmd.ExecuteScalar();
                return result != null;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (SecondConnection.State == ConnectionState.Open)
                    SecondConnection.Close();
            }
        }

        public bool SchemaObjectExists(ISchemaConstraint schemaConstraint)
        {
            // Check if a constraint exists
            try
            {
                using var cmd = SecondConnection.CreateCommand();
                SecondConnection.Open();
                cmd.CommandText = $"SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = '{schemaConstraint.Name}'";
                var result = cmd.ExecuteScalar();
                return result != null;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (SecondConnection.State == ConnectionState.Open)
                    SecondConnection.Close();
            }
        }

        public T SchemaObjectId<T>(ISchemaObject schemaObject)
        {
            return default;
        }
        #endregion

        #region field formatting
        public string SafeFormatField(string fieldName)
        {
            return $"\"{fieldName}\"";
        }

        public string SafeFormatAlterTableDropColumn(string tableName, List<string> columnsToDelete)
        {
            var drops = columnsToDelete.Select(c => $"DROP COLUMN \"{c}\"");
            return $"ALTER TABLE \"{tableName}\" {string.Join(", ", drops)}";
        }

        public string SafeFormatAlterTableAddColumn(string tableName, List<string> columnsToAdd)
        {
            var adds = columnsToAdd.Select(c => $"ADD COLUMN {c}");
            return $"ALTER TABLE \"{tableName}\" {string.Join(", ", adds)}";
        }
        #endregion
    }
}
