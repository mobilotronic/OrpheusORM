using Microsoft.Extensions.Logging;
using OrpheusAttributes;
using OrpheusCore.Errors;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

namespace OrpheusCore.SchemaBuilder
{
    internal class OrpheusSchemaInfo
    {
        [Length(60)]
        public string SchemaDescription { get; set; }
        public double Version { get; set; }
        [PrimaryKey]
        public Guid Id { get; set; }
    }

    internal class OrpheusSchemaObject
    {
        [Length(100)]
        [PrimaryKey]
        public string ObjectName { get; set; }

        public Guid ObjectId { get; set; }

        [DefaultValue(null)]
        public int? DbEngineObjectId { get; set; }

        [DataTypeAttribute((int)ExtendedDbTypes.StringBlob)]
        public string DDL { get; set; }

        [DataTypeAttribute((int)ExtendedDbTypes.StringBlob)]
        public string ConstraintsDDL { get; set; }

        public DateTime CreatedOn { get; set; }

        public int ObjectType { get; set; }

        [ForeignKey("OrpheusSchemaInfo", "Id")]
        public Guid SchemaId { get; set; }
    }

    internal class OrpheusSchemaModule
    {
        [PrimaryKey]
        public string ModuleName { get; set; }
        public string ModuleDefinition { get; set; }

        [ForeignKey("OrpheusSchemaInfo", "Id")]
        public Guid SchemaId { get; set; }
    }


    internal class OrpheusSchemaConstants
    {
        public static string SchemaObjectPrefix = "Orpheus";
        public static string SchemaObjectsTable = "SchemaObject";
        public static string SchemaInfoTable = "SchemaInfo";
        public static string SchemaModulesTable = "SchemaModule";
        public static Guid SchemaId = Guid.Parse("249074F8-FB81-4115-BADB-6D425D9BE069");
    }

    /// <summary>
    /// Represents an Orpheus Schema.
    /// </summary>
    public class Schema : ISchema
    {
        #region private properties
        private OrpheusSchema _orpheusSchema;
        private IOrpheusDatabase db;
        private Guid id;
        private List<ISchemaObject> schemaObjectCache;
        private IDbCommand schemaObjectExistsPreparedQuery;
        private ILogger logger;
        private OrpheusSchema orpheusSchema
        {
            get
            {
                if (this._orpheusSchema == null)
                    this._orpheusSchema = new OrpheusSchema(this.db, "Internal Orpheus Schema", 1.0, OrpheusSchemaConstants.SchemaId, null);
                return this._orpheusSchema;
            }
        }

        private string formatLoggerMessage(string message)
        {
            var result = "{0} [{1}]";

            return String.Format(result, this.Name == null ? this.Description : this.Name, message);
        }

        //private IOrpheusTable<OrpheusSchemaInfo> internalSchemaInfo;
        #endregion

        #region public properties
        /// <summary>
        /// Orpheus database.
        /// </summary>
        /// <returns>Instance of the Orpheus Database</returns>
        public IOrpheusDatabase DB { get { return this.db; } }

        /// <value>
        /// List of schema objects. <see cref="ISchemaObject"/>
        /// </value>
        public List<ISchemaObject> SchemaObjects { get; set; }

        /// <value>
        /// List of reference schemas
        /// </value>
        public List<ISchema> ReferencedSchemas { get; set; }

        /// <summary>
        /// Schema name.
        /// </summary>
        /// <returns>Schema description</returns>
        public string Name { get; protected set; }

        /// <summary>
        /// Schema description.
        /// </summary>
        /// <returns>Schema description</returns>
        public string Description { get; protected set; }

        /// <summary>
        /// Schema version.
        /// </summary>
        /// <returns>Schema version</returns>
        public double Version { get; protected set; }

        /// <summary>
        /// Schema Id.
        /// </summary>
        /// <returns>Schema unique id</returns>
        public Guid Id { get { return this.id; } }

        /// <value>
        /// Orpheus schema objects table.
        /// </value>
        public string SchemaObjectsTable
        {
            get
            {
                return OrpheusSchemaConstants.SchemaObjectPrefix + OrpheusSchemaConstants.SchemaObjectsTable;
            }
        }
        /// <value>
        /// Orpheus schema info table.
        /// </value>
        public string SchemaInfoTable
        {
            get
            {
                return OrpheusSchemaConstants.SchemaObjectPrefix + OrpheusSchemaConstants.SchemaInfoTable;
            }
        }

        /// <summary>
        /// Orpheus module definition table.
        /// </summary>
        /// <returns>Table name for the Orpheus schema modules table</returns>
        public string SchemaModulesTable
        {
            get
            {
                return OrpheusSchemaConstants.SchemaObjectPrefix + OrpheusSchemaConstants.SchemaModulesTable;
            }
        }

        #endregion

        #region constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        public Schema()
        {
            this.logger = ServiceManager.CreateLogger<Schema>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="description">The description.</param>
        /// <param name="version">The version.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="name">Schema name. If the DB engine is SQL server, and the name value is set, it will be used as SCHEMA name.</param>
        public Schema(IOrpheusDatabase db, string description, double version, Guid id, string name = null) : this(db, description, version, id)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="description">The description.</param>
        /// <param name="version">The version.</param>
        /// <param name="id">The identifier.</param>
        public Schema(IOrpheusDatabase db, string description, double version, Guid id)
        {
            this.db = db;
            this.Description = description;
            this.Version = version;
            this.id = id;
            this.logger = ServiceManager.CreateLogger<Schema>();
            this.SchemaObjects = new List<ISchemaObject>();
            this.ReferencedSchemas = new List<ISchema>();
        }
        #endregion

        #region public methods        
        /// <summary>
        /// Adds a schema object to the list.
        /// </summary>
        /// <param name="schemaObject"></param>
        /// <returns>
        /// The schema object that was added
        /// </returns>
        public ISchemaObject AddSchemaObject(ISchemaObject schemaObject)
        {
            if (schemaObject != null)
            {
                schemaObject.Schema = this;
                this.SchemaObjects.Add(schemaObject);
                schemaObject.AddDependency(this.orpheusSchema.SchemaObjectTable);
            }
            return schemaObject;
        }

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="dependencies">List of schema objects, that this objects depends upon</param>
        /// <param name="model">Model will be used to auto-generate fields, primary keys etc, for the schema object</param>
        /// <returns></returns>
        public ISchemaTable AddSchemaTable(string tableName, List<ISchemaObject> dependencies = null, object model = null)
        {
            var result = new SchemaObjectTable();
            result.SQLName = tableName;
            if (dependencies != null)
            {
                foreach (var dep in dependencies)
                    result.AddDependency(dep);
            }
            this.AddSchemaObject(result);
            if (model != null)
                result.CreateFieldsFromModel(model);

            return result;
        }

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="model">Model will be used to auto-generate fields, primary keys etc, for the schema object</param>
        /// <param name="dependencies">List of schema objects, that this objects depends upon</param>
        /// <returns></returns>
        public ISchemaTable AddSchemaTable(object model, List<ISchemaObject> dependencies = null)
        {
            return this.AddSchemaTable(model.GetType().Name, dependencies, model);
        }

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <param name="modelType">Model type will be used to auto-generate fields, primary keys etc, for the schema object</param>
        /// <param name="dependencies">List of schema objects, that this objects depends upon</param>
        /// <returns></returns>
        public ISchemaTable AddSchemaTable(Type modelType, List<ISchemaObject> dependencies = null)
        {
            var modelInstance = Activator.CreateInstance(modelType);
            return this.AddSchemaTable(modelInstance, dependencies);
        }


        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <typeparam name="T">Schema table type</typeparam>
        /// <param name="dependencies">The dependencies.</param>
        /// <returns></returns>
        public ISchemaTable AddSchemaTable<T>(List<ISchemaObject> dependencies = null) where T : class
        {
            var modelInstance = Activator.CreateInstance(typeof(T));
            return this.AddSchemaTable(modelInstance, dependencies);
        }

        /// <summary>
        /// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        /// <typeparam name="D">Dependency model type</typeparam>
        /// <returns></returns>
        public ISchemaTable AddSchemaTable<T, D>()
        {
            var modelInstance = Activator.CreateInstance(typeof(T));
            //var dependencies = this.SchemaObjects.Where(obj => obj.SQLName.ToLower() == typeof(D).Name.ToLower()).ToList();
            var dependencyTypeName = typeof(D).Name;
            var dependencies = this.SchemaObjects.Where(obj =>
                      obj.Schema.Name == null ? obj.SQLName.ToLower() == dependencyTypeName.ToLower() : obj.SQLName.Split(".")[1].Trim().ToLower() == dependencyTypeName.ToLower()
                   ).ToList();
            return this.AddSchemaTable(modelInstance, dependencies);
        }

        /// <summary>
        /// Creates a view schema object.
        /// </summary>
        /// <returns></returns>
        public ISchemaView CreateSchemaView()
        {
            var result = ServiceManager.Resolve<ISchemaView>();
            result.Schema = this;
            return result;
        }

        /// <summary>
        /// Creates a view table schema object.
        /// </summary>
        /// <returns></returns>
        public ISchemaViewTable CreateSchemaViewTable()
        {
            var result = ServiceManager.Resolve<ISchemaViewTable>();
            result.Schema = this;
            return result;
        }

        /// <summary>
        /// Creates a table schema object.
        /// </summary>
        /// <returns></returns>
        public ISchemaTable CreateSchemaTable()
        {
            var result = ServiceManager.Resolve<ISchemaTable>();
            result.Schema = this;
            return result;
        }

        /// <summary>
        /// Creates a schema object.
        /// </summary>
        /// <returns></returns>
        public ISchemaObject CreateSchemaObject()
        {
            var result = ServiceManager.Resolve<ISchemaObject>();
            result.Schema = this;
            return result;
        }

        /// <summary>
        /// Creates a join schema definition.
        /// </summary>
        /// <returns></returns>
        public ISchemaJoinDefinition CreateSchemaJoinDefinition()
        {
            return ServiceManager.Resolve<ISchemaJoinDefinition>();
        }

        /// <summary>
        /// Removes a schema object from the schema list.
        /// </summary>
        /// <param name="schemaObject"></param>
        public void RemoveSchemaObject(ISchemaObject schemaObject)
        {
            this.SchemaObjects.Remove(schemaObject);
        }

        /// <summary>
        /// Iterates through registered schema objects and executes them.
        /// </summary>
        public void Execute()
        {
            this.orpheusSchema.CreateSchema();

            this.RegisterSchema();

            this.logger.LogDebug(this.formatLoggerMessage("Begin creating schema"));
            this.SchemaObjects.ForEach(schObj =>
            {
                schObj.Execute();
            });
            this.logger.LogDebug(this.formatLoggerMessage("End creating schema"));
        }

        /// <summary>
        /// Drops schema. Removes all schema objects from the database.
        /// </summary>
        public void Drop()
        {
            this.logger.LogDebug(this.formatLoggerMessage("Begin dropping schema"));
            //this.orpheusSchema.DropSchema();

            this.SchemaObjects.ForEach(schObj =>
            {
                schObj.Drop();
            });

            //if the schema is dropped make sure to clear the cache.
            if (this.schemaObjectCache != null)
            {
                this.schemaObjectCache.Clear();
                this.schemaObjectCache = null;
            }
            this.logger.LogDebug(this.formatLoggerMessage("End dropping schema"));
        }

        /// <summary>
        /// Loads schema definition from a file.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        public void LoadFromFile(string fileName)
        {
            XDocument xDoc = XDocument.Load(fileName);
            var schema = from node in xDoc.Descendants("OrpheusSchema")
                         select new
                         {
                             Description = node.Element("Description").Value,
                             id = node.Element("Id").Value,
                             Version = Convert.ToDouble(node.Element("Version").Value),
                             SchemaObjects = (from schemaNode in node.Element("SchemaObject").Elements()
                                              select new
                                              {

                                              }).ToList()
                         };

        }
        /// <summary>
        /// Saves schema definition to a file. If the file exists it will overwrite it.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        public void SaveToFile(string fileName)
        {
            XDocument xDoc = null;
            try
            {
                xDoc = new XDocument(new XElement("OrpheusSchema",
                new XElement("Description", this.Description)),
                new XElement("Id", this.Id.ToString()),
                new XElement("Version", this.Version),
                new XElement("SchemaObject",
                    from schemaObject in this.SchemaObjects
                    select new XElement(schemaObject.SQLName,
                        new XElement("DDL", schemaObject.GetDDLString())))
                );
                xDoc.Save(fileName);
            }
            finally
            {
                xDoc = null;
            }
        }

        /// <summary>
        /// Returns the guid of the schema object it is created.
        /// </summary>
        /// <param name="schemaObject">Schema object to be checked if it exists</param>
        /// <returns>
        /// The schema object unique id
        /// </returns>
        public Guid SchemaObjectExists(ISchemaObject schemaObject)
        {
            var result = Guid.Empty;
            //creating the schema cache and storing any schema object that is not in it.
            //caching the result to avoid hitting the database every time.
            if (this.schemaObjectCache == null)
                this.schemaObjectCache = new List<ISchemaObject>();

            if (this.schemaObjectExistsPreparedQuery == null)
            {
                this.schemaObjectExistsPreparedQuery = this.db.CreatePreparedQuery(String.Format("SELECT ObjectId,ObjectName FROM {0} WHERE ObjectName = @OBJECT_NAME", this.SchemaObjectsTable), new List<string>() { "@OBJECT_NAME" });
            }
            var schemaInCache = this.schemaObjectCache.Find(cacheObject => { return cacheObject.SQLName.ToLowerInvariant() == schemaObject.SQLName.ToLowerInvariant(); });

            if (this.db.LastActiveTransaction != null)
                this.schemaObjectExistsPreparedQuery.Transaction = this.db.LastActiveTransaction;

            if (schemaInCache != null)
            {
                //if the item does not exist in the database but exists in object cache, then update the cache.
                if (!this.db.DDLHelper.SchemaObjectExists(schemaObject))
                {
                    this.schemaObjectCache.Remove(schemaInCache);
                    return result;
                }
                return schemaInCache.UniqueKey;
            }
            ((IDataParameter)this.schemaObjectExistsPreparedQuery.Parameters["@OBJECT_NAME"]).Value = schemaObject.SQLName;
            IDataReader reader = null;
            try
            {
                reader = this.schemaObjectExistsPreparedQuery.ExecuteReader();
                if (reader.Read())
                {
                    var id = reader.GetGuid(0);
                    if (id != Guid.Empty)
                    {
                        result = new Guid(id.ToString());
                        var newSchemaObject = new SchemaObject();
                        newSchemaObject.Schema = this;
                        newSchemaObject.SQLName = reader.GetString(1);
                        newSchemaObject.UniqueKey = result;
                        this.schemaObjectCache.Add(newSchemaObject);
                    }
                }
            }
            catch (Exception e)
            {
                result = Guid.Empty;
                this.logger.LogError(ErrorCodes.ERR_SCHEMA_OBJECT_EXISTS, e, ErrorDictionary.GetError(ErrorCodes.ERR_SCHEMA_OBJECT_EXISTS));
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// Registers schema information, in the schema information table.
        /// </summary>
        public void RegisterSchema()
        {
            var schemaInfoTable = this.DB.CreateTable<OrpheusSchemaInfo>();
            schemaInfoTable.Load(new List<object>() { this.Id });
            if (schemaInfoTable.Data.Count == 0)
            {
                schemaInfoTable.Add(new OrpheusSchemaInfo()
                {
                    Id = this.id,
                    SchemaDescription = this.Description,
                    Version = this.Version
                });
                schemaInfoTable.Save();
            }
        }
        #endregion

    }

    internal class OrpheusSchema : Schema
    {
        #region private properties
        private ISchemaTable internalSchemaInfo;
        private ISchemaTable internalSchemaObject;
        private ISchemaTable internalSchemaModules;
        #endregion

        #region private methods
        private void initializeOrpheusSchema()
        {
            this.internalSchemaObject = new SchemaObjectTable();
            this.internalSchemaObject.Schema = this;
            this.internalSchemaObject.CreateFieldsFromModel<OrpheusSchemaObject>();


            this.internalSchemaInfo = new SchemaObjectTable();
            this.internalSchemaInfo.Schema = this;
            this.internalSchemaInfo.CreateFieldsFromModel<OrpheusSchemaInfo>();

            var data = new List<OrpheusSchemaInfo>();
            data.Add(new OrpheusSchemaInfo() { SchemaDescription = this.Description, Version = this.Version, Id = OrpheusSchemaConstants.SchemaId });
            this.internalSchemaInfo.SetData<OrpheusSchemaInfo>(data);


            this.internalSchemaModules = new SchemaObjectTable();
            this.internalSchemaModules.Schema = this;
            this.internalSchemaModules.CreateFieldsFromModel<OrpheusSchemaModule>();

            this.SchemaObjects.Add(this.internalSchemaObject);
        }

        private void executeOrpheusInternalSchema()
        {
            if (!this.DB.DDLHelper.SchemaObjectExists("OrpheusSchemaObject"))
            {
                this.internalSchemaInfo.Execute();
                this.internalSchemaObject.Execute();
                this.internalSchemaModules.Execute();
            }
        }

        private void dropOrpheusInternalSchema()
        {
            this.internalSchemaObject.Drop();
            this.internalSchemaModules.Drop();
            this.internalSchemaInfo.Drop();
        }
        #endregion

        #region public properties
        public ISchemaTable SchemaObjectTable { get { return this.internalSchemaObject; } }
        #endregion

        #region public methods
        public void CreateSchema()
        {
            this.executeOrpheusInternalSchema();
        }

        public void DropSchema()
        {
            this.dropOrpheusInternalSchema();
        }
        #endregion

        public OrpheusSchema()
        {
            this.Name = OrpheusSchemaConstants.SchemaObjectPrefix;
            this.initializeOrpheusSchema();
        }

        /// <summary>
        /// Creates an Orpheus schema.
        /// </summary>
        /// <param name="db">Database where the schema belongs</param>
        /// <param name="description">Schema description</param>
        /// <param name="version">Schema version</param>
        /// <param name="id">Schema unique id</param>
        /// <param name="name">Schema name</param>
        public OrpheusSchema(IOrpheusDatabase db, string description, double version, Guid id, string name) : base(db, description, version, id, name)
        {
            this.initializeOrpheusSchema();
        }

        /// <summary>
        /// Creates an Orpheus schema.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="description"></param>
        /// <param name="version"></param>
        /// <param name="id"></param>
        public OrpheusSchema(IOrpheusDatabase db, string description, double version, Guid id) : base(db, description, version, id)
        {
            this.initializeOrpheusSchema();
        }

    }
}
