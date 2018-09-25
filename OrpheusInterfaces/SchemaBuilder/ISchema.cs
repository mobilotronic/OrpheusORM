using OrpheusInterfaces.Core;
using System;
using System.Collections.Generic;

namespace OrpheusInterfaces.Schema
{
    /// <summary>
    /// Represents an Orpheus Schema.
    /// </summary>
    public interface ISchema
    {
        /// <summary>
        /// List of other schemas.
        /// </summary>
        List<ISchema> ReferencedSchemas { get; }

        /// <summary>
        /// List of schema objects. <see cref="ISchemaObject"/>
        /// </summary>
        List<ISchemaObject> SchemaObjects { get; }

        /// <summary>
        /// Adds a schema object to the list.
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <returns>The schema object that was added</returns>
        ISchemaObject AddSchemaObject(ISchemaObject schemaObject);

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="dependencies">List of schema objects, that this objects depends upon</param>
        /// <param name="model">Model will be used to auto-generate fields, primary keys etc, for the schema object</param>
        /// <returns></returns>
        ISchemaTable AddSchemaTable(string tableName,List<ISchemaObject> dependencies = null,object model=null);

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="dependencies">List of schema objects, that this objects depends upon</param>
        /// <param name="model">Model will be used to auto-generate fields, primary keys etc, for the schema object</param>
        /// <returns></returns>
        ISchemaTable AddSchemaTable(object model, List<ISchemaObject> dependencies = null);

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="dependencies">List of schema objects, that this objects depends upon</param>
        /// <param name="modelType">Model type will be used to auto-generate fields, primary keys etc, for the schema object</param>
        /// <returns></returns>
        ISchemaTable AddSchemaTable(Type modelType, List<ISchemaObject> dependencies = null);

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        ISchemaTable AddSchemaTable<T>(List<ISchemaObject> dependencies = null) where T : class;

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <typeparam name="D">Dependency model type</typeparam>
        /// <returns></returns>
        ISchemaTable AddSchemaTable<T, D>();

        /// <summary>
        /// Creates a view schema object.
        /// </summary>
        /// <returns></returns>
        ISchemaView CreateSchemaView();

        /// <summary>
        /// Creates a view table schema object.
        /// </summary>
        /// <returns></returns>
        ISchemaViewTable CreateSchemaViewTable();

        /// <summary>
        /// Creates a table schema object.
        /// </summary>
        /// <returns></returns>
        ISchemaTable CreateSchemaTable();

        /// <summary>
        /// Creates a schema object.
        /// </summary>
        /// <returns></returns>
        ISchemaObject CreateSchemaObject();

        /// <summary>
        /// Creates a join schema definition.
        /// </summary>
        /// <returns></returns>
        ISchemaJoinDefinition CreateSchemaJoinDefinition();

        /// <summary>
        /// DB SQL schema name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Schema description.
        /// </summary>
        /// <returns>Schema description</returns>
        string Description { get; }

        /// <summary>
        /// Schema version.
        /// </summary>
        /// <returns>Schema version</returns>
        double Version { get; }

        /// <summary>
        /// Iterate through the schema objects and executes them.
        /// </summary>
        void Execute();

        /// <summary>
        /// Drops schema. Removes all schema objects from the database.
        /// </summary>
        void Drop();

        /// <summary>
        /// Saves schema to an xml file.
        /// </summary>
        /// <param name="fileName"></param>
        void SaveToFile(string fileName);

        /// <summary>
        /// Loads schema from an xml file.
        /// </summary>
        /// <param name="fileName"></param>
        void LoadFromFile(string fileName);

        /// <summary>
        /// Orpheus database.
        /// </summary>
        /// <returns>Instance of the Orpheus Database</returns>
        IOrpheusDatabase DB { get;}

        /// <summary>
        /// Schema Id.
        /// </summary>
        /// <returns>Schema unique id</returns>
        Guid Id { get; }

        /// <summary>
        /// Removes from the schema list
        /// </summary>
        /// <param name="schemaObject">Schema object to remove</param>
        void RemoveSchemaObject(ISchemaObject schemaObject);

        /// <summary>
        /// Orpheus schema objects table.
        /// </summary>
        /// <returns>Table name for the Orpheus schema objects table</returns>
        string SchemaObjectsTable { get; }

        /// <summary>
        /// Orpheus schema info table.
        /// </summary>
        /// <returns>Table name for the Orpheus schema information table</returns>
        string SchemaInfoTable { get; }

        /// <summary>
        /// Orpheus module definition table.
        /// </summary>
        /// <returns>Table name for the Orpheus schema modules table</returns>
        string SchemaModulesTable { get; }

        /// <summary>
        /// Returns the guid of the schema object it is created.
        /// </summary>
        /// <param name="schemaObject">Schema object to be checked if it exists</param>
        /// <returns>The schema object unique id</returns>
        Guid SchemaObjectExists(ISchemaObject schemaObject);

        /// <summary>
        /// Registers schema information, in the schema information table.
        /// </summary>
        void RegisterSchema();

    }
}
