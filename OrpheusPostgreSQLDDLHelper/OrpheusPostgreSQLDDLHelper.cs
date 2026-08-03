using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OrpheusPostgreSQLDDLHelper
{
    /// <summary>
    /// PostgreSQL DDL helper — generates and executes PostgreSQL-specific DDL commands.
    /// </summary>
    public class OrpheusPostgreSQLDDLHelper : IOrpheusDDLHelper, IDisposable
    {
        #region private fields
        private Dictionary<Type, string> typeMap = new();
        private Dictionary<int, string> dbTypeMap = new();
        private readonly ILogger logger;
        private NpgsqlConnection secondConnection;
        private NpgsqlConnection masterConnection;
        #endregion

        #region auxiliary connections
        /// <summary>
        /// The connection factory backing this database, required to build the auxiliary connections
        /// below. Only null if the database was constructed from a raw IDbConnection instead of a
        /// connection factory, which isn't supported for schema/DDL operations.
        /// </summary>
        private IOrpheusConnectionFactory connectionFactory
        {
            get
            {
                if (DB?.ConnectionFactory == null)
                    throw new InvalidOperationException("Schema/DDL operations require a database constructed with an IOrpheusConnectionFactory.");
                return DB.ConnectionFactory;
            }
        }

        private NpgsqlConnection SecondConnection
        {
            get
            {
                if (secondConnection == null)
                    secondConnection = (NpgsqlConnection)connectionFactory.CreateSecondaryConnection();
                return secondConnection;
            }
        }

        private NpgsqlConnection MasterConnection
        {
            get
            {
                if (masterConnection == null)
                    masterConnection = (NpgsqlConnection)connectionFactory.CreateAdministrativeConnection();
                return masterConnection;
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new PostgreSQL DDL helper.
        /// </summary>
        /// <param name="logger">Logger used for database-creation and DDL-execution failures.</param>
        public OrpheusPostgreSQLDDLHelper(ILogger<OrpheusPostgreSQLDDLHelper> logger)
        {
            this.logger = logger ?? NullLogger<OrpheusPostgreSQLDDLHelper>.Instance;
            initializeTypeMap();
        }

        /// <summary>
        /// Disposes the auxiliary secondary/administrative connections, if created.
        /// </summary>
        public void Dispose()
        {
            secondConnection?.Dispose();
            secondConnection = null;
            masterConnection?.Dispose();
            masterConnection = null;
            GC.SuppressFinalize(this);
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

        /// <inheritdoc/>
        public IOrpheusDatabase DB { get; set; }

        /// <inheritdoc/>
        public char DelimitedIdentifierStart => '"';
        /// <inheritdoc/>
        public char DelimitedIdentifierEnd => '"';

        /// <inheritdoc/>
        public bool SupportsGuidType => true;
        /// <inheritdoc/>
        public bool SupportsSchemaNameSpace => true; // PostgreSQL schemas

        /// <inheritdoc/>
        public string DatabaseName
        {
            get
            {
                // Not ConnectionString/DB.ConnectionString: that's only populated once a connection
                // has actually been leased, but CreateDatabase() needs the target name *before* that
                // connection exists (chicken-and-egg — this runs during Connect() itself). The
                // connection configuration, unlike the live connection string, is available from the
                // start. Matches OrpheusSQLServerDDLHelper/OrpheusMySQLServerDDLHelper's DatabaseName.
                return DB?.DatabaseConnectionConfiguration?.DatabaseName;
            }
        }

        /// <inheritdoc/>
        public DatabaseEngineType DbEngineType => DatabaseEngineType.dbPostgreSQL;

        /// <inheritdoc/>
        public string ConnectionString => DB?.ConnectionString ?? string.Empty;

        /// <inheritdoc/>
        public string ModifyColumnCommand => "ALTER COLUMN";

        #endregion

        #region type conversion
        /// <inheritdoc/>
        public string TypeToString(Type type)
        {
            if (typeMap.TryGetValue(type, out var result))
                return result;
            return "VARCHAR";
        }

        /// <inheritdoc/>
        public string DbTypeToString(DbType dataType)
        {
            if (dbTypeMap.TryGetValue((int)dataType, out var result))
                return result;
            return "VARCHAR";
        }
        #endregion

        #region database operations
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool CreateDatabase()
        {
            return CreateDatabase(DatabaseName);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public bool SchemaObjectExists(ISchemaObject schemaObject)
        {
            return SchemaObjectExists(schemaObject.SQLName);
        }

        /// <inheritdoc/>
        public bool SchemaObjectExists(string schemaObjectName)
        {
            try
            {
                using var cmd = SecondConnection.CreateCommand();
                SecondConnection.Open();
                // PostgreSQL folds unquoted identifiers to lowercase, and every CREATE TABLE this
                // codebase emits is unquoted — so information_schema.tables.table_name is always
                // lowercase regardless of the case callers pass in here. Compare case-insensitively
                // rather than assuming callers already pass the folded form.
                var parts = schemaObjectName.Split('.');
                var schema = parts.Length > 1 ? parts[0] : "public";
                var table = parts.Length > 1 ? parts[1] : parts[0];
                cmd.CommandText = $"SELECT 1 FROM information_schema.tables WHERE LOWER(table_schema) = LOWER('{schema}') AND LOWER(table_name) = LOWER('{table}')";
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

        /// <inheritdoc/>
        public bool SchemaObjectExists(ISchemaConstraint schemaConstraint)
        {
            // Check if a constraint exists
            try
            {
                using var cmd = SecondConnection.CreateCommand();
                SecondConnection.Open();
                cmd.CommandText = $"SELECT 1 FROM information_schema.table_constraints WHERE LOWER(constraint_name) = LOWER('{schemaConstraint.Name}')";
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

        /// <inheritdoc/>
        /// <remarks>
        /// Not implemented for PostgreSQL — always returns <c>default</c>. Nothing in this codebase
        /// currently calls it for the PostgreSQL engine, but implement this before relying on it.
        /// </remarks>
        public T SchemaObjectId<T>(ISchemaObject schemaObject)
        {
            return default;
        }
        #endregion

        #region field formatting
        /// <inheritdoc/>
        public string SafeFormatField(string fieldName)
        {
            return $"\"{fieldName}\"";
        }

        /// <inheritdoc/>
        public string SafeFormatAlterTableDropColumn(string tableName, List<string> columnsToDelete)
        {
            // columnsToDelete arrives already delimited (the caller runs each name through
            // SafeFormatField first, same contract as the other DDL helpers) — wrapping again here
            // would double-quote it (e.g. ""Code"" instead of "Code"), which PostgreSQL rejects as a
            // zero-length delimited identifier.
            var drops = columnsToDelete.Select(c => $"DROP COLUMN {c}");
            return $"ALTER TABLE \"{tableName}\" {string.Join(", ", drops)}";
        }

        /// <inheritdoc/>
        public string SafeFormatAlterTableAddColumn(string tableName, List<string> columnsToAdd)
        {
            var adds = columnsToAdd.Select(c => $"ADD COLUMN {c}");
            return $"ALTER TABLE \"{tableName}\" {string.Join(", ", adds)}";
        }
        #endregion

        #region batched insert with key retrieval
        /// <inheritdoc/>
        /// <remarks>
        /// PostgreSQL implementation note: INSERT...RETURNING preserves input VALUES row order for a
        /// single statement, so generated keys map back to rows positionally — no correlation column
        /// needed.
        /// <para>
        /// NOT VERIFIED against a real PostgreSQL instance: no reachable PostgreSQL server was
        /// available in the environment this was implemented in (only SQL Server was reachable, and
        /// that path IS verified — see SQLServerBatchedInsertKeyRetrievalTests). Verify this against a
        /// real instance before relying on it in production.
        /// </para>
        /// </remarks>
        public List<object> ExecuteBatchedInsertWithKeyRetrieval(string tableName, List<string> columns, string keyColumnName, List<List<object>> rows, IDbTransaction transaction)
        {
            using var cmd = this.buildBatchedInsertWithKeyRetrievalCommand(tableName, columns, keyColumnName, rows, transaction);
            var result = new List<object>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetValue(0));
            return result;
        }

        /// <inheritdoc/>
        public async Task<List<object>> ExecuteBatchedInsertWithKeyRetrievalAsync(string tableName, List<string> columns, string keyColumnName, List<List<object>> rows, IDbTransaction transaction, CancellationToken cancellationToken = default)
        {
            using var cmd = (System.Data.Common.DbCommand)this.buildBatchedInsertWithKeyRetrievalCommand(tableName, columns, keyColumnName, rows, transaction);
            var result = new List<object>();
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                result.Add(reader.GetValue(0));
            return result;
        }

        private IDbCommand buildBatchedInsertWithKeyRetrievalCommand(string tableName, List<string> columns, string keyColumnName, List<List<object>> rows, IDbTransaction transaction)
        {
            var cmd = this.DB.CreateCommand();
            cmd.Transaction = transaction;
            var columnList = string.Join(",", columns.Select(c => this.SafeFormatField(c)));
            var rowValueLists = new List<string>();
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var placeholders = new List<string>();
                for (var colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    var paramName = $"@p_{rowIndex}_{colIndex}";
                    placeholders.Add(paramName);
                    var param = cmd.CreateParameter();
                    param.ParameterName = paramName;
                    param.Value = rows[rowIndex][colIndex] ?? DBNull.Value;
                    cmd.Parameters.Add(param);
                }
                rowValueLists.Add($"({string.Join(",", placeholders)})");
            }
            cmd.CommandText = $"INSERT INTO {tableName} ({columnList}) VALUES {string.Join(",", rowValueLists)} RETURNING {this.SafeFormatField(keyColumnName)}";
            return cmd;
        }
        #endregion
    }
}
