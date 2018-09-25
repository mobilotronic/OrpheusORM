using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// Extended DbTypes enumeration, for types that are not listed in the generic DbType enumeration.
    /// </summary>
    public enum ExtendedDbTypes
    {
        /// <summary>
        /// A variable-length stream of Unicode data with a maximum length
        /// </summary>
        StringBlob = 999
    }

    /// <summary>
    /// Database engine type.
    /// </summary>
    public enum DatabaseEngineType
    {
        /// <summary>
        /// Unknown - unsupported.
        /// </summary>
        dbUnknown,
        /// <summary>
        /// SQL Server.
        /// </summary>
        dbSQLServer,
        /// <summary>
        /// MySQL.
        /// </summary>
        dbMySQL,
        /// <summary>
        /// SQLite
        /// </summary>
        dbSQLite
    }

    /// <summary>
    /// Abstract definition of DDL helper.
    /// DDL helper is used to execute DB engine specific DDL commands.
    /// </summary>
    public interface IOrpheusDDLHelper
    {
        /// <summary>
        /// Returns true the database exists.
        /// </summary>
        /// <param name="dbName">Database name</param>
        bool DatabaseExists(string dbName);

        /// <summary>
        /// Returns true if the schema object exists in the database. A schema object can be a table,view,primary key, stored procedure, etc.
        /// </summary>
        /// <param name="schemaObject">Schema</param>
        /// <returns>True if the object exists</returns>
        bool SchemaObjectExists(ISchemaObject schemaObject);

        /// <summary>
        /// Returns true if the schema object exists in the database. A schema object can be a table,view,primary key, stored procedure, etc.
        /// </summary>
        /// <param name="schemaObjectName">Schema</param>
        /// <returns>True if the object exists</returns>
        bool SchemaObjectExists(string schemaObjectName);

        /// <summary>
        /// Returns true if the schema object exists in the database.
        /// </summary>
        /// <param name="schemaConstraint"></param>
        /// <returns></returns>
        bool SchemaObjectExists(ISchemaConstraint schemaConstraint);

        /// <summary>
        /// Returns true if a database is successfully created using the underlying db engine settings.
        /// </summary>
        /// <returns>True if database was created successfully</returns>
        bool CreateDatabase();

        /// <summary>
        /// Returns true if a database is successfully created using the underlying db engine settings.
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>True if the database was created successfully</returns>
        bool CreateDatabase(string dbName);

        /// <summary>
        /// Returns true if a database is successfully created using the passed DDL script.
        /// </summary>
        /// <param name="ddlString">DDL command</param>
        /// <returns>True if the database was created successfully</returns>
        bool CreateDatabaseWithDDL(string ddlString);

        /// <summary>
        /// Database for the DDL helper.
        /// </summary>
        /// <returns>Database the helper is associated with</returns>
        IOrpheusDatabase DB { get; set; }

        /// <summary>
        /// Returns the db engine specific string equivalent, for a .net type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>String value for the mapped DbType</returns>
        string TypeToString(Type type);

        /// <summary>
        /// Returns the db engine specific string equivalent, for a DbType enumeration.
        /// </summary>
        /// <param name="dataType">DbType</param>
        /// <returns>String value for the DbType</returns>
        string DbTypeToString(DbType dataType);

        /// <summary>
        /// Identifiers that do not comply with all of the rules for identifiers must be delimited in a SQL statement, enclosed in the DelimitedIdentifier char.
        /// </summary>
        /// <returns>Char</returns>
        char DelimitedIndetifierStart { get; }

        /// <summary>
        /// Identifiers that do not comply with all of the rules for identifiers must be delimited in a SQL statement, enclosed in the DelimitedIdentifier char.
        /// </summary>
        /// <returns>Char</returns>
        char DelimitedIndetifierEnd { get; }

        /// <summary>
        /// Returns true if the DBEngine supports natively the Guid type.
        /// </summary>
        /// <returns>True if the DBEngine supports natively the Guid type</returns>
        bool SupportsGuidType { get; }

        /// <summary>
        /// Returns true if the DBEngine supports having schema name spaces. From the currently supported databases, only SQL has this feature.
        /// </summary>
        bool SupportsSchemaNameSpace { get; }

        /// <summary>
        /// Properly formats a field name, to be used in a SQL statement, in case the field name is a reserved word.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        /// <returns>Safely formated field name</returns>
        string SafeFormatField(string fieldName);

        /// <summary>
        /// Returns the DB specific modify table command.
        /// </summary>
        string ModifyColumnCommand { get; }

        /// <summary>
        /// Properly formats an ALTER TABLE DROP COLUMN command for the underlying database engine.
        /// </summary>
        /// <param name="tableName">Table's name that schema is going to change</param>
        /// <param name="columnsToDelete">Columns for deletion</param>
        /// <returns></returns>
        string SafeFormatAlterTableDropColumn(string tableName, List<string> columnsToDelete);

        /// <summary>
        /// Properly formats an ALTER TABLE ADD COLUMN command for the underlying database engine.
        /// </summary>
        /// <param name="tableName">Table's name that schema is going to change</param>
        /// <param name="columnsToAdd">Columns for creation</param>
        /// <returns></returns>
        string SafeFormatAlterTableAddColumn(string tableName, List<string> columnsToAdd);

        /// <summary>
        /// Gets the database name.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Returns the underlying connection type.
        /// </summary>
        DatabaseEngineType DbEngineType { get; }

        /// <summary>
        /// Builds the connection string.
        /// </summary>
        /// <returns></returns>
        string ConnectionString { get; }
    }

    /// <summary>
    /// SQL Server specific DDL helper definition.
    /// </summary>
    public interface ISQLServerDDLHelper : IOrpheusDDLHelper
    {
        #region schema 
        /// <summary>
        /// Returns true if the schema exists.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        bool SchemaExists(string schemaName);

        /// <summary>
        /// Creates a schema.
        /// </summary>
        /// <param name="schemaName"></param>
        void CreateSchema(string schemaName);

        /// <summary>
        /// Drops a schema.
        /// </summary>
        /// <param name="schemaName"></param>
        void DropSchema(string schemaName);

        /// <summary>
        /// Schema separator. Char that separates the schema name and the schema object. By default in SQL server, the separator is the dot char.
        /// </summary>
        string SchemaSeparator { get;}

        /// <summary>
        /// Returns true if the schema object exists in the database. A schema object can be a table,view,primary key, stored procedure, etc.
        /// </summary>
        /// <param name="schemaObjectName"></param>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        bool SchemaObjectExists(string schemaObjectName, string schemaName = null);
        #endregion

        #region database role
        /// <summary>
        /// Returns true if the database role exists.
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        bool DatabaseRoleExists(string roleName);

        /// <summary>
        /// Creates a database role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="owner"></param>
        void CreateDatabaseRole(string roleName, string owner = null);

        /// <summary>
        /// Drops a database role.
        /// </summary>
        /// <param name="roleName"></param>
        void DropDatabaseRole(string roleName);

        /// <summary>
        /// Adds a database user to the specified role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userName"></param>
        void AddDatabaseRoleMember(string roleName, string userName);

        /// <summary>
        /// Drops a database user to the specified role.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userName"></param>
        void DropDatabaseRoleMember(string roleName, string userName);

        #endregion

        #region user
        /// <summary>
        /// Creates a contained database user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        void CreateDatabaseUser(string userName, string password);

        /// <summary>
        /// Changes the password for an existing user.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        void ChangeDatabaseUserPassword(string userName, string oldPassword, string newPassword);


        /// <summary>
        /// Drops a user.
        /// </summary>
        /// <param name="userName"></param>
        void DropDatabaseUser(string userName);

        /// <summary>
        /// Returns true if a database user exists.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        bool DatabaseUserExists(string userName);
        #endregion

        #region databases
        /// <summary>
        /// Enables/disables the contained database feature on the SQL server instance. A feature supported from SQL server 2012 and later.
        /// </summary>
        /// <param name="enable"></param>
        void EnableContainedDatabases(bool enable);

        /// <summary>
        /// Sets the containment option for a database.
        /// </summary>
        /// <param name="containment">Containment value. NONE or PARTIAL</param>
        /// <param name="databaseName"></param>
        void SetDatabaseContainment(string containment, string databaseName = null);
        #endregion

        #region permissions
        /// <summary>
        /// Grants permission to a database principal.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="databasePrincipal"></param>
        void Grant(string permission, string databasePrincipal);

        /// <summary>
        /// Grants permission to a database principal.
        /// </summary>
        /// <param name="permissions"></param>
        /// <param name="databasePrincipal"></param>
        void Grant(List<string> permissions, string databasePrincipal);

        /// <summary>
        /// Grants permission to a database principal for a specific schema object.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="schemaObject"></param>
        /// <param name="databasePrincipal"></param>
        void Grant(string permission, string schemaObject, string databasePrincipal);

        /// <summary>
        /// Deny permission to a database principal.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="databasePrincipal"></param>
        void Deny(string permission, string databasePrincipal);

        /// <summary>
        /// Deny permissions to a database principal.
        /// </summary>
        /// <param name="permissions"></param>
        /// <param name="databasePrincipal"></param>
        void Deny(List<string> permissions, string databasePrincipal);

        /// <summary>
        /// Denies permission to a database principal for a specific schema object.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="schemaObject"></param>
        /// <param name="databasePrincipal"></param>
        void Deny(string permission, string schemaObject, string databasePrincipal);

        /// <summary>
        /// Revoke permission for a database principal.
        /// </summary>
        /// <param name="permission"></param>
        /// <param name="schemaObject"></param>
        /// <param name="databasePrincipal"></param>
        void Revoke(string permission,string schemaObject, string databasePrincipal);

        /// <summary>
        /// Revoke permissions for a database principal.
        /// </summary>
        /// <param name="permissions"></param>
        /// <param name="schemaObject"></param>
        /// <param name="databasePrincipal"></param>
        void Revoke(List<string> permissions,string schemaObject, string databasePrincipal);
        #endregion

    }

    /// <summary>
    /// MySQL Server specific DDL helper definition.
    /// </summary>
    public interface IMySQLServerDDLHelper : IOrpheusDDLHelper
    {
    }
}
