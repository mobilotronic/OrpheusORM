using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
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
        /// Create table.
        /// </summary>
        sotCreateTable,
        
        /// <summary>
        /// Alter table.
        /// </summary>
        sotAlterTable,

        /// <summary>
        /// Drop table.
        /// </summary>
        sotDropTable,
        
        /// <summary>
        /// Create view.
        /// </summary>
        sotCreateView,

        /// <summary>
        /// Drop view.
        /// </summary>
        sotDropView,

        /// <summary>
        /// Alter column.
        /// </summary>
        sotAlterColumn
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
        /// DB Schema name. Any DDL/SQL statement executed will have pre-pent the SchemaName if set.
        /// </summary>
        string SchemaName { get; set; }

        /// <summary>
        /// The name of the schema object. Could be the name of a table or a view or a stored procedure.
        /// </summary>
        string SQLName { get; set; }

        /// <summary>
        /// Unique generated when the object is created and saved in the DB.
        /// </summary>
        Guid UniqueKey { get; set; }

        /// <summary>
        /// If DDL is set all other fields and join schema objects are ignored. Dependencies still apply.
        /// </summary>
        string RawDDL { get; set; }

        /// <summary>
        /// Returns the DDL string to be executed.
        /// </summary>
        string GetDDLString();

        /// <summary>
        /// Returns the DDL constraints string to be executed. 
        /// </summary>
        /// <returns></returns>
        string GetConstraintsDDL();

        /// <summary>
        /// Fields for the schema object. Applicable mostly when schema object is a table or a view.
        /// </summary>
        List<ISchemaField> Fields { get; set; }

        /// <summary>
        /// List of schema object constraints. Primary,foreign or any type of constraint.
        /// </summary>
        List<ISchemaConstraint> Constraints { get; set; }

        /// <summary>
        /// Other schema objects that this object depends upon. First it will iterate through the dependency list and run any schema object that is not yet created.
        /// </summary>
        List<ISchemaObject> SchemaObjectsThatIDepend { get; set; }

        /// <summary>
        /// Other schema objects that depend on this object. First it will iterate through the dependency list and run any schema object that is not yet destroyed.
        /// </summary>
        List<ISchemaObject> SchemaObjectsThatDependOnMe { get; set; }

        /// <summary>
        /// Optional data to initialize a schema object. Practically applicable only to a table.
        /// </summary>
        void SetData<T>(List<T> data);

        /// <summary>
        /// Returns the seed data for the table if defined.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> GetData<T>();

        /// <summary>
        /// True if the schema object is created in the DB.
        /// </summary>
        bool IsCreated { get; }

        /// <summary>
        /// Executes schema object.
        /// </summary>
        void Execute();

        /// <summary>
        /// Drops the schema object.
        /// </summary>
        void Drop();

        /// <summary>
        /// Orpheus database.
        /// </summary>
        IOrpheusDatabase DB { get; }

        /// <summary>
        /// Gets the schema type.
        /// </summary>
        /// <returns></returns>
        SchemaObjectType GetSchemaType();

        /// <summary>
        /// Schema where the schema object belongs to.
        /// </summary>
        ISchema Schema { get; set; }

        /// <summary>
        /// Creates fields from a given model.
        /// Supports <see cref="OrpheusAttributes"/> attributes
        /// <param name="model"></param>
        /// </summary>
        void CreateFieldsFromModel(object model);

        /// <summary>
        /// Creates and adds a field to the field list.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="defaultValue"></param>
        /// <param name="size"></param>
        /// <param name="nullable"></param>
        ISchemaField AddField(string name, string dataType, bool nullable = true, string defaultValue = null, string size = null, string alias = null);

        /// <summary>
        /// Adds a primary key constraint.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        IPrimaryKeySchemaConstraint AddPrimaryKeyConstraint(string name, List<string> fields, SchemaSort sort = SchemaSort.ssAsc);

        /// <summary>
        /// Adds a foreign key constraint.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <param name="foreignKeySchemaObject"></param>
        /// <param name="foreignKeySchemaFields"></param>
        /// <param name="onCascadeDelete"></param>
        /// <param name="onUpdateDelete"></param>
        /// <returns></returns>
        IForeignKeySchemaConstraint AddForeignKeyConstraint(string name, List<string> fields, string foreignKeySchemaObject, List<string> foreignKeySchemaFields, bool onCascadeDelete = true, bool onUpdateCascade = true);

        /// <summary>
        /// Adds a unique key constraint.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        IUniqueKeySchemaConstraint AddUniqueKeyConstraint(string name, List<string> fields);

        /// <summary>
        /// Defines the DDL action to be taken when schema objects are executed.
        /// </summary>
        DDLAction Action { get; set; }

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
    }

    /// <summary>
    /// Create table schema interface.
    /// </summary>
    public interface ISchemaTable : ISchemaObject
    {
        /// <summary>
        /// Join definition. Defines how schema objects can be joined.
        /// </summary>
        ISchemaJoinDefinition JoinDefinition { get; set; }
    }

    /// <summary>
    /// Create view schema object.
    /// </summary>
    public interface ISchemaView : ISchemaObject
    {
        /// <summary>
        /// Join schema objects. Applicable mostly when schema object is a table or a view.
        /// </summary>
        List<ISchemaTable> JoinSchemaObjects { get; set; }
        /// <summary>
        /// Applicable only when DDLCommand = ddcCreateView.
        /// </summary>
        string TableName { get; set; }
    }
}
