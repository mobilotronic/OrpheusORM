﻿using MySql.Data.MySqlClient;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace OrpheusMySQLDDLHelper
{
    public class OrpheusMySQLServerDDLHelper : IOrpheusDDLHelper
    {
        private Dictionary<Type, string> typeMap = new Dictionary<Type, string>();
        private Dictionary<int, string> dbTypeMap = new Dictionary<int, string>();
        private string databaseName;
        private IDbCommand selectSchemaObjectTable;
        private IDbCommand selectSchemaObjectConstraint;
        private MySqlConnection secondConnection;
        private IOrpheusDatabase db;

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

        public bool SupportsGuidType { get; private set; }

        public bool CreateDatabase()
        {
            var result = false;
            MySqlConnectionStringBuilder connStringBuilder = new MySqlConnectionStringBuilder(this.db.ConnectionString);
            var dbName = connStringBuilder.Database;
            if (this.secondConnection != null && !this.DatabaseExists(dbName))
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
                        catch
                        {
                            result = false;
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

        public bool CreateDatabase(string dbName)
        {
            var result = false;
            if (this.secondConnection != null && !this.DatabaseExists(dbName))
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
                        catch
                        {
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

        public bool CreateDatabaseWithDDL(string ddlString)
        {
            var result = false;
            if (this.secondConnection != null)
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
                        catch
                        {
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
                            if (reader.Read())
                            {
                                result = reader.GetValue(0) != null;
                            }
                        }
                        catch
                        {
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
                    this.selectSchemaObjectConstraint.CommandText = "SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = @NAME";
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
                    reader = this.selectSchemaObjectTable.ExecuteReader();
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
                        reader.Close();
                    this.secondConnection.Close();
                }
            }
            return false;
        }

        public IOrpheusDatabase DB
        {
            get { return this.db; }
            set
            {
                this.db = value;
                if (this.db != null && this.secondConnection == null)
                {
                    MySqlConnectionStringBuilder connStringBuilder = new MySqlConnectionStringBuilder(this.db.ConnectionString);
                    connStringBuilder.Database = "sys";
                    this.secondConnection = new MySqlConnection(connStringBuilder.ConnectionString);
                    try
                    {
                        this.secondConnection.Open();
                        this.secondConnection.Close();
                    }
                    catch
                    {
                        this.secondConnection.Dispose();
                        this.secondConnection = null;
                    }
                }
            }
        }

        public string TypeToString(Type type)
        {
            if (this.typeMap.ContainsKey(type))
            {
                return this.typeMap[type];
            }

            return null;
        }

        public string DbTypeToString(DbType dataType)
        {
            if (this.dbTypeMap.ContainsKey((int)dataType))
            {
                return this.dbTypeMap[(int)dataType];
            }

            return null;
        }

        public char DelimitedIndetifierStart { get { return '`'; } }

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
        public string ModifyColumnCommand { get{ return " MODIFY COLUMN "; } }

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
                if (databaseName == null)
                {
                    var builder = new MySqlConnectionStringBuilder(this.DB.ConnectionString);
                    databaseName =  builder.Database;
                }
                return databaseName;
            }
        }

        public OrpheusMySQLServerDDLHelper()
        {
            this.initializeTypeMap();
            this.SupportsGuidType = false;
        }
    }
}