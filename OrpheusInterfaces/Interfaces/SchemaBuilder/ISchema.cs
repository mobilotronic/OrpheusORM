using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
{
    /// <summary>
    /// Represents an Orpheus Schema.
    /// </summary>
    public interface ISchema
    {
        /// <summary>
        /// List of schema objects. <see cref="ISchemaObject"/>
        /// </summary>
        List<ISchemaObject> SchemaObjects { get; }

        /// <summary>
        /// Adds a schema object to the list.
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <returns></returns>
        ISchemaObject AddSchemaObject(ISchemaObject schemaObject);

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dependencies"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        ISchemaTable AddSchemaTable(string tableName,List<ISchemaObject> dependencies = null,object model=null);

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        ISchemaTable AddSchemaTable(object model, List<ISchemaObject> dependencies = null);

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="dependencies"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        ISchemaTable AddSchemaTable(Type modelType, List<ISchemaObject> dependencies = null);

        /// <summary>
        /// Schema description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Schema version.
        /// </summary>
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
        IOrpheusDatabase DB { get;}

        /// <summary>
        /// Schema Id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Removes from the schema list
        /// </summary>
        /// <param name="schemaObject"></param>
        void RemoveSchemaObject(ISchemaObject schemaObject);

        /// <summary>
        /// Orpheus schema objects table.
        /// </summary>
        string SchemaObjectsTable { get; }

        /// <summary>
        /// Orpheus schema info table.
        /// </summary>
        string SchemaInfoTable { get; }

        /// <summary>
        /// Orpheus module definition table.
        /// </summary>
        string SchemaModulesTable { get; }

        /// <summary>
        /// Returns the guid of the schema object it is created.
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <returns></returns>
        Guid SchemaObjectExists(ISchemaObject schemaObject);

    }
}
