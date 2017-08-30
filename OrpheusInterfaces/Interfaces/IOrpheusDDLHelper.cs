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
        /// <param name="dbName"></param>
        /// <returns></returns>
        bool DatabaseExists(string dbName);

        /// <summary>
        /// Returns true if the schema object exists in the database. A schema object can be a table,view,primary key, stored procedure, etc.
        /// </summary>
        /// <param name="schemaObjectName"></param>
        /// <returns></returns>
        bool SchemaObjectExists(string schemaObjectName);

        /// <summary>
        /// Returns true if a database is successfully created using the underlying db engine settings.
        /// </summary>
        /// <returns></returns>
        bool CreateDatabase();

        /// <summary>
        /// Returns true if a database is successfully created using the underlying db engine settings.
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        bool CreateDatabase(string dbName);

        /// <summary>
        /// Returns true if a database is successfully created using the passed DDL script.
        /// </summary>
        /// <param name="ddlString"></param>
        /// <returns></returns>
        bool CreateDatabaseWithDDL(string ddlString);

        /// <summary>
        /// Database for the DDL helper.
        /// </summary>
        IOrpheusDatabase DB { get; set; }

        /// <summary>
        /// Returns the db engine specific string equivalent, for a .net type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string TypeToString(Type type);

        /// <summary>
        /// Returns the db engine specific string equivalent, for a DbType enumeration.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        string DbTypeToString(DbType dataType);

        /// <summary>
        /// Identifiers that do not comply with all of the rules for identifiers must be delimited in a SQL statement, enclosed in the DelimitedIdentifier char.
        /// </summary>
        char DelimitedIndetifierStart { get; }

        /// <summary>
        /// Identifiers that do not comply with all of the rules for identifiers must be delimited in a SQL statement, enclosed in the DelimitedIdentifier char.
        /// </summary>
        char DelimitedIndetifierEnd { get; }

        /// <summary>
        /// Returns true if the DBEngine supports natively the Guid type.
        /// </summary>
        bool SupportsGuidType { get; }

        /// <summary>
        /// Properly formats a field name, to be used in a SQL statement, in case the field name is a reserved word.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        string SafeFormatField(string fieldName);
    }
}
