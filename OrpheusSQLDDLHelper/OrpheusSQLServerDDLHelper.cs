﻿using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace OrpheusSQLDDLHelper
{
    public class OrpheusSQLServerDDLHelper : IOrpheusDDLHelper
    {
        private Dictionary<Type,string> typeMap = new Dictionary<Type,string>();
        private Dictionary<int, string> dbTypeMap = new Dictionary<int, string>();

        private IDbCommand selectSchemaObjectQuery;
        private SqlConnection secondConnection;
        private IOrpheusDatabase db;

        private void initializeTypeMap()
        {
            typeMap[typeof(byte)] = "TINYINT";
            typeMap[typeof(sbyte)] = "SMALLINT";
            typeMap[typeof(short)] = "SMALLINT";
            typeMap[typeof(ushort)] = "INT";
            typeMap[typeof(int)] = "INT";
            typeMap[typeof(uint)] = "BIGINT";
            typeMap[typeof(long)] = "BIGINT";
            typeMap[typeof(ulong)] = "BIGINT";
            typeMap[typeof(float)] = "FLOAT";
            typeMap[typeof(double)] = "REAL";
            typeMap[typeof(decimal)] = "DECIMAL";
            typeMap[typeof(bool)] = "BIT";
            typeMap[typeof(string)] = "NVARCHAR";
            typeMap[typeof(char)] = "NCHAR";
            typeMap[typeof(Guid)] = "UNIQUEIDENTIFIER";
            typeMap[typeof(DateTime)] = "DATETIME";
            typeMap[typeof(DateTimeOffset)] = "DATETIMEOFFSET";
            typeMap[typeof(byte[])] = "VARBINARY(MAX)";

            //nullable types.
            typeMap[typeof(byte?)] = "TINYINT";
            typeMap[typeof(sbyte?)] = "SMALLINT";
            typeMap[typeof(short?)] = "SMALLINT";
            typeMap[typeof(ushort?)] = "INT";
            typeMap[typeof(int?)] = "INT";
            typeMap[typeof(uint?)] = "BIGINT";
            typeMap[typeof(long?)] = "BIGINT";
            typeMap[typeof(ulong?)] = "BIGINT";
            typeMap[typeof(float?)] = "FLOAT";
            typeMap[typeof(double?)] = "REAL";
            typeMap[typeof(decimal?)] = "DECIMAL";
            typeMap[typeof(bool?)] = "BIT";
            typeMap[typeof(char?)] = "NCHAR";
            typeMap[typeof(Guid?)] = "UNIQUEIDENTIFIER";
            typeMap[typeof(DateTime?)] = "DATETIME";
            typeMap[typeof(DateTimeOffset?)] = "DATETIMEOFFSET";

            dbTypeMap[(int)DbType.Byte] = "TINYINT";
            dbTypeMap[(int)DbType.Int16] = "SMALLINT";
            dbTypeMap[(int)DbType.Int32] = "INT";
            dbTypeMap[(int)DbType.Int64] = "BIGINT";
            dbTypeMap[(int)DbType.Double] = "FLOAT";
            dbTypeMap[(int)DbType.Single] = "REAL";
            dbTypeMap[(int)DbType.Decimal] = "DECIMAL";
            dbTypeMap[(int)DbType.Boolean] = "BIT";
            dbTypeMap[(int)DbType.String] = "NVARCHAR";
            dbTypeMap[(int)DbType.AnsiString] = "VARCHAR";
            dbTypeMap[(int)DbType.AnsiStringFixedLength] = "CHAR";
            dbTypeMap[(int)DbType.Guid] = "UNIQUEIDENTIFIER";
            dbTypeMap[(int)DbType.DateTime] = "DATETIME";
            dbTypeMap[(int)DbType.DateTimeOffset] = "DATETIMEOFFSET";
            dbTypeMap[(int)DbType.Binary] = "VARBINARY(MAX)";
            dbTypeMap[(int)DbType.Time] = "TIME";

            dbTypeMap[(int)ExtendedDbTypes.StringBlob] = "NTEXT";
        }

        public bool SupportsGuidType { get; private set; }

        public bool CreateDatabase()
        {
            var result = false;
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(this.db.ConnectionString);
            var dbName = connStringBuilder.InitialCatalog;
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
                try {
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
                try {
                    using (var cmd = this.secondConnection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT DATABASE_ID FROM SYS.DATABASES WHERE NAME ='{0}'", dbName);
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
            var result = false;
            if (this.DB.Connected)
            {
                if (this.selectSchemaObjectQuery == null)
                    this.selectSchemaObjectQuery = this.DB.CreatePreparedQuery("SELECT OBJECT_ID FROM SYS.OBJECTS WHERE NAME = @NAME");
                IDataReader reader = null;
                try
                {
                    ((IDataParameter)this.selectSchemaObjectQuery.Parameters["@NAME"]).Value = schemaObjectName;
                    reader = this.selectSchemaObjectQuery.ExecuteReader();
                    if (reader.Read())
                    {
                        result = reader.GetValue(0) != null;
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
                }
            }
            return result;
        }

        public IOrpheusDatabase DB
        {
            get { return this.db; }
            set
            {
                this.db = value;
                if(this.db != null && this.secondConnection == null)
                {
                    SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(this.db.ConnectionString);
                    connStringBuilder.InitialCatalog = "master";
                    this.secondConnection = new SqlConnection(connStringBuilder.ConnectionString);
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

        public char DelimitedIndetifierStart { get { return '['; } }

        public char DelimitedIndetifierEnd { get { return ']'; } }

        /// <summary>
        /// Properly formats a field name, to be used in a SQL statement, in case the field name is a reserved word.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string SafeFormatField(string fieldName) {  return String.Format("{0}{1}{2}",this.DelimitedIndetifierStart,fieldName,this.DelimitedIndetifierEnd);}

        public OrpheusSQLServerDDLHelper()
        {
            this.initializeTypeMap();
            this.SupportsGuidType = true;
        }
    }
}
