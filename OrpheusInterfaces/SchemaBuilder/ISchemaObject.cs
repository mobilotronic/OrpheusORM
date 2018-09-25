using OrpheusInterfaces.Core;
using System;
using System.Collections.Generic;

namespace OrpheusInterfaces.Schema
{

    /// <summary>
    /// Type of a schema object.
    /// </summary>
    public enum SchemaObjectType
    {
        /// <summary>
        /// Unknown type. This is the default.
        /// </summary>
        sotUnknown,

        /// <summary>
        /// Table.
        /// </summary>
        sotTable,
      
        /// <summary>
        /// View.
        /// </summary>
        sotView,

        /// <summary>
        /// Unique index
        /// </summary>
        sotUniqueIndex
    }

    /// <summary>
    /// DDL action.
    /// </summary>
    public enum DDLAction
    {
        /// <summary>
        /// DDL create.
        /// </summary>
        ddlCreate,

        /// <summary>
        /// DDL alter.
        /// </summary>
        ddlAlter,

        /// <summary>
        /// DDL drop.
        /// </summary>
        ddlDrop
    }

    /// <summary>
    /// Base schema object interface.
    /// </summary>
    public interface ISchemaObject
    {
        /// <summary>
        /// The name of the schema object. Could be the name of a table or a view or a stored procedure.
        /// </summary>
        /// <returns>SQL name of the schema object</returns>
        string SQLName { get; set; }

        /// <summary>
        /// If DDL is set all other fields and join schema objects are ignored. Dependencies still apply.
        /// </summary>
        /// <returns>Set raw DDL for the schema object</returns>
        string RawDDL { get; set; }

        /// <summary>
        /// Returns the DDL string to be executed.
        /// </summary>
        /// <returns>Get the generated DDL string for the schema object</returns>
        List<string> GetDDLString();

        /// <summary>
        /// Schema where the schema object belongs to.
        /// </summary>
        /// <returns>Schema where the schema object exists</returns>
        ISchema Schema { get; set; }

        /// <summary>
        /// Adds a dependency to a schema object.
        /// </summary>
        /// <param name="schemaObject"></param>
        void AddDependency(ISchemaObject schemaObject);

        /// <summary>
        /// Adds a dependency to a schema object based on the model type.
        /// </summary>
        /// <param name="modelType"></param>
        void AddDependency(Type modelType);

        /// <summary>
        /// Adds a dependency to a schema object based on the model type.
        /// </summary>
        void AddDependency<T>() where T : class;

        /// <summary>
        /// Executes schema object.
        /// </summary>
        void Execute();

        /// <summary>
        /// Drops the schema object.
        /// </summary>
        void Drop();

        /// <summary>
        /// Defines the DDL action to be taken when schema objects are executed.
        /// </summary>
        /// <returns>Defines the DDL action to be taken when schema objects are executed</returns>
        DDLAction Action { get; set; }

        /// <summary>
        /// Unique generated when the object is created and saved in the DB.
        /// </summary>
        /// <returns>Schema object unique key</returns>
        Guid UniqueKey { get; set; }

        /// <summary>
        /// Other schema objects that this object depends upon. First it will iterate through the dependency list and run any schema object that is not yet created.
        /// </summary>
        /// <returns>Schema object that this object depends upon</returns>
        List<ISchemaObject> SchemaObjectsThatIDepend { get; set; }

        /// <summary>
        /// Other schema objects that depend on this object. First it will iterate through the dependency list and run any schema object that is not yet destroyed.
        /// </summary>
        /// <returns>Schema objects that depend on this object</returns>
        List<ISchemaObject> SchemaObjectsThatDependOnMe { get; set; }

        /// <summary>
        /// Gets the schema type.
        /// </summary>
        /// <returns>Schema type</returns>
        SchemaObjectType GetSchemaType();

        /// <summary>
        /// True if the schema object is created in the DB.
        /// </summary>
        /// <returns>True if the schema object is created in the DB</returns>
        bool IsCreated { get; }

        /// <summary>
        /// The schema object alias name.
        /// </summary>
        string AliasName { get; set; }
    }


    /// <summary>
    /// Base schema data object interface.
    /// </summary>
    public interface ISchemaDataObject : ISchemaObject
    {

        /// <summary>
        /// Returns the DDL constraints string to be executed. 
        /// </summary>
        /// <returns>Get the generated DDL string for the schema constraints</returns>
        List<string> GetConstraintsDDL();

        /// <summary>
        /// Fields for the schema object. Applicable mostly when schema object is a table or a view.
        /// </summary>
        /// <returns>Fields in the schema object</returns>
        List<ISchemaField> Fields { get; set; }

        /// <summary>
        /// List of schema object constraints. Primary,foreign or any type of constraint.
        /// </summary>
        /// <returns>Constraints in the schema object</returns>
        List<ISchemaConstraint> Constraints { get; set; }

        /// <summary>
        /// Optional data to initialize a schema object. Practically applicable only to a table.
        /// </summary>
        void SetData<T>(List<T> data);

        /// <summary>
        /// Returns the seed data for the table if defined.
        /// </summary>
        /// <typeparam name="T">Schema object model type</typeparam>
        /// <returns>Schema object's data</returns>
        List<T> GetData<T>();

        /// <summary>
        /// Orpheus database.
        /// </summary>
        /// <returns>Database where the schema object exists</returns>>
        IOrpheusDatabase DB { get; }

        /// <summary>
        /// Creates fields from a given model.
        /// Supports OrpheusAttributes attributes
        /// <param name="model">Instance of model</param>
        /// </summary>
        void CreateFieldsFromModel(object model);

        /// <summary>
        /// Creates fields from a given model.
        /// Supports OrpheusAttributes attributes
        /// <param name="modelType">Model type</param>
        /// </summary>
        void CreateFieldsFromModel(Type modelType);

        /// <summary>
        /// Creates fields from a given model.
        /// Supports OrpheusAttributes attributes
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        void CreateFieldsFromModel<T>();

        /// <summary>
        /// Creates and adds a field to the field list.
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="dataType">Field data type</param>
        /// <param name="defaultValue">Field default value</param>
        /// <param name="size">Field size</param>
        /// <param name="nullable">Nullable</param>
        /// <param name="alias">Field alias</param>
        ISchemaField AddField(string name, string dataType, bool nullable = true, string defaultValue = null, string size = null, string alias = null);

        /// <summary>
        /// Adds a primary key constraint.
        /// </summary>
        /// <param name="name">Constraint name</param>
        /// <param name="fields">Constraint fields</param>
        /// <param name="sort">Constraint sort</param>
        /// <returns></returns>
        IPrimaryKeySchemaConstraint AddPrimaryKeyConstraint(string name, List<string> fields, SchemaSort sort = SchemaSort.ssAsc);

        /// <summary>
        /// Adds a foreign key constraint.
        /// </summary>
        /// <param name="name">Constraint name</param>
        /// <param name="fields">Constraint fields</param>
        /// <param name="foreignKeySchemaObject">Reference table name</param>
        /// <param name="foreignKeySchemaFields">Reference table fields</param>
        /// <param name="onCascadeDelete">Cascade on delete</param>
        /// <param name="onUpdateDelete">Cascade on update</param>
        /// <returns></returns>
        IForeignKeySchemaConstraint AddForeignKeyConstraint(string name, List<string> fields, string foreignKeySchemaObject, List<string> foreignKeySchemaFields, bool onCascadeDelete = true, bool onUpdateDelete = true);

        /// <summary>
        /// Adds a unique key constraint.
        /// </summary>
        /// <param name="name">Constraint name</param>
        /// <param name="fields">Constraint fields</param>
        /// <returns>An IUniqueKeySchemaConstraint</returns>
        IUniqueKeySchemaConstraint AddUniqueKeyConstraint(string name, List<string> fields);

    }

    /// <summary>
    /// Create table schema interface.
    /// </summary>
    public interface ISchemaTable : ISchemaDataObject
    {
        /// <summary>
        /// Join definition. Defines how schema objects can be joined.
        /// </summary>
        /// <returns>Join definition</returns>
        ISchemaJoinDefinition JoinDefinition { get; set; }
    }

    /// <summary>
    /// A data table that is used in a View.
    /// </summary>
    public interface ISchemaViewTable: ISchemaTable
    {
        /// <summary>
        /// Override for a table's schema name.
        /// </summary>
        string SchemaName { get; set; }
    }

    /// <summary>
    /// Create view schema object.
    /// </summary>
    public interface ISchemaView : ISchemaDataObject
    {
        /// <summary>
        /// Join schema objects. Applicable mostly when schema object is a table or a view.
        /// </summary>
        /// <returns>Schema objects in the schema view</returns>
        List<ISchemaTable> JoinSchemaObjects { get; set; }
        /// <summary>
        /// Applicable only when DDLCommand = ddcCreateView.
        /// </summary>
        /// <returns>Table name</returns>
        string TableName { get; set; }

        /// <summary>
        /// Returns the main table name, SQL formatted, with a schema name, if the underlying db engine supports it, and with the table alias, if defined.
        /// </summary>
        /// <returns></returns>
        string FormattedTableName();

        /// <summary>
        /// SQL server specific option, to create views with schema binding, in order to be able to create indexes on the view itself.
        /// </summary>
        bool WithSchemaBinding { get; set; }
    }
}
