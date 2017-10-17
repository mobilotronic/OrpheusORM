using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
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
    /// Abstract definition of DDL helper.
    /// DDL helper is used to execute DB engine specific DDL commands.
    /// </summary>
    public interface IOrpheusDDLHelper
    {
        /// <summary>
        /// Returns true the database exists.
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>True if the database exists</returns>
        bool DatabaseExists(string dbName);

        /// <summary>
        /// Returns true if the schema object exists in the database. A schema object can be a table,view,primary key, stored procedure, etc.
        /// </summary>
        /// <param name="schemaObjectName">Schema object name</param>
        /// <returns>True if the object exists</returns>
        bool SchemaObjectExists(string schemaObjectName);

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
    }
}
