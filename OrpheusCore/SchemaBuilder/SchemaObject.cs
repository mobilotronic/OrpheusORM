using Microsoft.Extensions.Logging;
using OrpheusCore.Configuration;
using OrpheusCore.Errors;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace OrpheusCore.SchemaBuilder
{
    /// <summary>
    /// Base schema object.
    /// </summary>
    public class SchemaObject : ISchemaObject
    {
        #region private properties
        #endregion

        #region private methods


        #endregion

        #region protected properties
        /// <summary>
        /// 
        /// </summary>
        protected ILogger logger;
        /// <summary>
        /// 
        /// </summary>
        protected string _sqlName;
        #endregion

        #region protected methods
        /// <summary>
        /// Formats the logger's message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected string formatLoggerMessage(string message)
        {
            var result = "{0} [{1}]";
            return String.Format(result, this.SQLName == null ? "" : this.SQLName, message);
        }

        /// <summary>
        /// Creates the DDL string for the schema object.
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> createDDLString()
        {
            return null;
        }

        /// <summary>
        /// Returns the schema type.
        /// </summary>
        /// <returns>Returns <see cref="SchemaObjectType"/></returns>
        protected virtual SchemaObjectType getType() { return SchemaObjectType.sotUnknown; }

        /// <summary>
        /// Returns true if the schema can be executed.
        /// </summary>
        /// <returns></returns>
        protected virtual bool canExecuteSchema()
        {
            //if the schema object has no SQL name or it's already created and the DDLAction is not drop,
            //then we won't do anything.
            if (this.SQLName == null || this.IsCreated)
                return false;
            return true;
        }

        /// <summary>
        /// Registers an Orpheus schema.
        /// </summary>
        /// <param name="transaction"></param>
        protected virtual void registerSchema(IDbTransaction transaction) { }

        /// <summary>
        /// Unregisters an Orpheus schema.
        /// </summary>
        /// <param name="transaction"></param>
        protected virtual void unRegisterSchema(IDbTransaction transaction) { }

        /// <summary>
        /// Returns formatted the SQL name for the object, including schema name and/or alias name, if defined.
        /// </summary>
        /// <returns></returns>
        protected virtual string formatSQLName()
        {
            if (this.Schema.DB.DDLHelper.SupportsSchemaNameSpace && this.Schema.Name != null)
            {
                //if the SQL name of the object already includes the schema name, just return the sqlname.
                //there are cases, specially in views, where all the views can be belong to a schema
                //but the tables that are part of the view, can belong to different schemas.
                var schemaSeparator = this.Schema.DB.DDLHelperAs<ISQLServerDDLHelper>().SchemaSeparator;
                if (this._sqlName.Contains(schemaSeparator))
                {
                    return String.Format("{0} {1}", this._sqlName, this.AliasName);
                }
                else
                    return String.Format("{0}.{1} {2}", this.Schema.Name, this._sqlName, this.AliasName);
            }
            else
                return this.AliasName == null ? this._sqlName : String.Format("{0} {1}", this._sqlName, this.AliasName);
        }

        #endregion

        #region public properties
        /// <value>
        /// The SQL name for the schema. This will be used to actually create the schema object in the database.
        /// </value>
        public string SQLName
        {
            get
            {
                return this.formatSQLName();
            }
            set
            {
                this._sqlName = value;
            }
        }

        /// <value>
        /// Overrides any schema object configuration and executes RawDLL contents.
        /// </value>
        public string RawDDL { get; set; }

        /// <value>
        /// The Schema where the schema object belongs to.
        /// </value>
        public ISchema Schema { get; set; }

        /// <value>
        /// Defines the behavior of execute. <see cref="DDLAction"/>
        /// </value>
        public DDLAction Action { get; set; }

        /// <value>
        /// An Id to uniquely identify this schema object.
        /// </value>
        public Guid UniqueKey { get; set; }

        /// <value>
        /// A list of other schema objects that this instance depends upon. Any schema object in this list
        /// will be executed before, to make sure that on the time of creation they will be available/created.
        /// </value>
        public List<ISchemaObject> SchemaObjectsThatIDepend { get; set; }

        /// <value>
        /// A list of other schema objects that depend on this instance.
        /// </value>
        public List<ISchemaObject> SchemaObjectsThatDependOnMe { get; set; }

        /// <summary>
        /// Schema Type
        /// </summary>
        /// <returns>Returns <see cref="SchemaObjectType"/></returns>
        public SchemaObjectType GetSchemaType() { return this.getType(); }

        /// <value>
        /// Flag to indicate if the schema object has actually been created in the database.
        /// </value>
        public bool IsCreated { get; protected set; }

        /// <value>
        /// The schema object alias name.
        /// </value>
        public string AliasName { get; set; }

        #endregion

        #region public methods
        /// <summary>
        /// Generate the schema DDL string.
        /// </summary>
        /// <returns>Returns the DDL string ready to be executed.</returns>
        public List<string> GetDDLString() { return this.createDDLString(); }

        /// <summary>
        /// Adds a dependency to a schema object.
        /// </summary>
        /// <param name="schemaObject">The schema object.</param>
        public void AddDependency(ISchemaObject schemaObject)
        {
            if (schemaObject != null)
            {
                this.SchemaObjectsThatIDepend.Add(schemaObject);
                schemaObject.SchemaObjectsThatDependOnMe.Add(this);
            }
        }

        /// <summary>
        /// Adds a dependency to a schema object based on the model type.
        /// </summary>
        /// <param name="modelType">The model type.</param>
        /// <exception cref="Exception">Model [model name] has been registered in more than schema's</exception>
        public void AddDependency(Type modelType)
        {
            if (modelType != null)
            {
                try
                {
                    var dependenedObjects = new List<ISchemaObject>();
                    var schemaObject = this.Schema.SchemaObjects.Where(obj =>
                       obj.Schema.Name == null ? obj.SQLName.ToLower() == modelType.Name.ToLower() : obj.SQLName.Split(".")[1].Trim().ToLower() == modelType.Name.ToLower()
                    ).FirstOrDefault();
                    //if the schema object that this object is depended upon, is not part of the same schema, then search for that object in other schema's 
                    if (schemaObject == null)
                    {
                        if (this.Schema.ReferencedSchemas.Count > 0)
                        {
                            foreach (var s in this.Schema.ReferencedSchemas)
                            {
                                var schObj = s.SchemaObjects.Where(obj => obj.SQLName.ToLower() == modelType.Name.ToLower()).FirstOrDefault();
                                if (schObj != null)
                                    dependenedObjects.Add(schObj);
                            }
                        }
                    }
                    else
                        dependenedObjects.Add(schemaObject);

                    //if the same type is registered in two different schema's then, throw an error.
                    if (dependenedObjects.Count > 1)
                        throw new Exception(String.Format("Model {0} has been registered in more than schema's", modelType.Name));

                    this.AddDependency(schemaObject);
                }
                catch (Exception e)
                {
                    this.logger.LogError(ErrorCodes.ERR_SCHEMA_ADD_DEPENDENCY, e,ErrorDictionary.GetError(ErrorCodes.ERR_SCHEMA_ADD_DEPENDENCY));
                    if (e is NullReferenceException)
                    {
                        this.logger.LogError("Table with name {0} not found", modelType.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a dependency to a schema object based on the model type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddDependency<T>() where T : class
        {
            this.AddDependency(typeof(T));
        }

        /// <summary>
        /// Creates schema object.
        /// </summary>
        public virtual void Execute() { }

        /// <summary>
        /// Drops the schema object.
        /// </summary>
        public virtual void Drop() { }
        #endregion

        #region constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaObject"/> class.
        /// </summary>
        public SchemaObject()
        {
            this.logger = ConfigurationManager.LoggerFactory.CreateLogger<SchemaObject>();
            this.IsCreated = false;
            this.SchemaObjectsThatDependOnMe = new List<ISchemaObject>();
            this.SchemaObjectsThatIDepend = new List<ISchemaObject>();
        }
        #endregion
    }

    /// <summary>
    /// Base schema data object.
    /// </summary>
    public class SchemaDataObject : SchemaObject, ISchemaDataObject
    {
        #region private fields
        private IOrpheusTable<OrpheusSchemaObject> schemaObjectsTable;
        private object dataToSeed;
        private bool objectExistsInDatabase;
        /// <summary>
        /// Schema model helper.
        /// </summary>
        protected OrpheusModelHelper modelHelper;
        #endregion

        #region protected methods
        /// <summary>
        /// Table that holds the initial seed data for the schema.
        /// </summary>
        protected IOrpheusTable seedDataTable;

        /// <summary>
        /// Returns the schema constraints SQL.
        /// </summary>
        /// <returns></returns>
        protected string getConstraintsDDL()
        {
            var sBuilder = new StringBuilder();
            this.Constraints.ForEach(constraint => {
                sBuilder.AppendLine(String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL()));
            });
            return sBuilder.ToString();
        }

        /// <summary>
        /// Applies schema constraints.
        /// </summary>
        /// <param name="cmd" type="IDbCommand"></param>
        protected virtual void applyConstraints(IDbCommand cmd) { }

        /// <summary>
        /// Inserts data to the DB engine, if the schema is a Table.
        /// </summary>
        /// <param name="cmd"></param>
        protected virtual void seedData(IDbCommand cmd) { }

        /// <summary>
        /// Registers an Orpheus schema.
        /// </summary>
        /// <param name="transaction"></param>
        protected override void registerSchema(IDbTransaction transaction = null)
        {
            if (this.Schema.SchemaObjectExists(this) == Guid.Empty && this.SQLName != this.Schema.SchemaInfoTable)
            {

                if(this.schemaObjectsTable == null)
                {
                    this.schemaObjectsTable = this.DB.CreateTable<OrpheusSchemaObject>();
                }

                this.schemaObjectsTable.Add(new OrpheusSchemaObject() {
                    ObjectId = this.UniqueKey,
                    ObjectName = this.SQLName,
                    DDL = string.Join(",", this.GetDDLString().ToArray()),
                    ConstraintsDDL = string.Join(",", this.GetConstraintsDDL().ToArray()),
                    CreatedOn = DateTime.Now,
                    ObjectType = (int)this.GetSchemaType(),
                    SchemaId = this.Schema.Id,
                    DbEngineObjectId = this.DB.DDLHelper.SchemaObjectId<int?>(this)
                });

                this.schemaObjectsTable.Save(transaction,transaction == null ? true : false);
            }

        }

        /// <summary>
        /// Unregisters an Orpheus schema.
        /// </summary>
        /// <param name="transaction"></param>
        protected override void unRegisterSchema(IDbTransaction transaction = null)
        {
            if (this.Schema.SchemaObjectExists(this) != Guid.Empty)
            {

                if (this.schemaObjectsTable == null)
                {
                    this.schemaObjectsTable = this.DB.CreateTable<OrpheusSchemaObject>();
                }

                this.schemaObjectsTable.Delete(new OrpheusSchemaObject()
                {
                    ObjectId = this.UniqueKey,
                    ObjectName = this.SQLName,
                    DDL = string.Join(",", this.GetDDLString().ToArray()),
                    ConstraintsDDL = string.Join(",", this.GetConstraintsDDL().ToArray()),
                    CreatedOn = DateTime.Now,
                    ObjectType = (int)this.GetSchemaType(),
                    SchemaId = this.Schema.Id
                });

                this.schemaObjectsTable.Save(transaction, transaction == null ? true : false);
            }
        }

        /// <summary>
        /// Returns true if the schema can be executed.
        /// </summary>
        /// <returns></returns>
        protected override bool canExecuteSchema()
        {
            //if the schema object has no SQL name or it's already created and the DDLAction is not drop,
            //then we won't do anything.
            if (!base.canExecuteSchema())
                return false;

            //does the schema object actually exist in the database?
            this.objectExistsInDatabase = this.DB.DDLHelper.SchemaObjectExists(this);

            if(!this.objectExistsInDatabase && this.Action == DDLAction.ddlDrop)
            {
                //if the schema object does not actually exist but it still is set a created, then log a warning.
                if (this.IsCreated)
                    this.logger.LogWarning(this.formatLoggerMessage("Not found in the database"));
                return false;
            }

            if(this.Action != DDLAction.ddlDrop)
            {
                if (this.objectExistsInDatabase)
                    this.Action = DDLAction.ddlAlter;
                else
                    this.Action = DDLAction.ddlCreate;
                //this.Action = this.objectExistsInDatabase ? DDLAction.ddlAlter : DDLAction.ddlCreate;
            }
            return true;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Database that the schema object belongs to.
        /// Taken from the <see cref="ISchema"/> property.
        /// </summary>
        public IOrpheusDatabase DB { get { return this.Schema.DB; } }

        /// <summary>
        /// The list of fields of the schema object.
        /// </summary>
        public List<ISchemaField> Fields { get; set; }

        /// <summary>
        /// The list of constraints of the schema object.
        /// </summary>
        public List<ISchemaConstraint> Constraints { get; set; }

        #endregion

        #region schema fields and constraints                
        /// <summary>
        /// Creates fields from a given model.
        /// Supports OrpheusAttributes attributes
        /// <param name="model">Instance of model</param>
        /// </summary>
        public void CreateFieldsFromModel(object model)
        {
            this.CreateFieldsFromModel(model.GetType());
        }

        /// <summary>
        /// Creates fields from a given model.
        /// Supports OrpheusAttributes attributes
        /// </summary>
        /// <typeparam name="T">Model type</typeparam>
        public void CreateFieldsFromModel<T>()
        {
            this.CreateFieldsFromModel(typeof(T));
        }

        /// <summary>
        /// Creates fields from a given model.
        /// Supports OrpheusAttributes attributes
        /// <param name="modelType">Model type</param>
        /// </summary>
        public void CreateFieldsFromModel(Type modelType)
        {
            this.modelHelper = new OrpheusModelHelper(modelType);
            //if the schema has no name, set it to the type name.
            if (this.SQLName == null)
                this.SQLName = modelType.Name;
            this.modelHelper.CreateSchemaFields(this);

            if (this.SQLName != null && (this.SQLName.ToLower() != this.modelHelper.SQLName.ToLower()))
                this.logger.LogWarning($"The table name {this.SQLName} passed into the constructor is not the same as the associated model's {this.modelHelper.SQLName}");
            ////if there was a [TableName] attribute set, then it takes precedence.
            //if (this.modelHelper.SQLName != null)
            //    this.SQLName = this.modelHelper.SQLName;
        }


        /// <summary>
        /// Creates and adds a field to the field list.
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="dataType">Field data type</param>
        /// <param name="nullable">Nullable</param>
        /// <param name="defaultValue">Field default value</param>
        /// <param name="size">Field size</param>
        /// <param name="alias">Field alias</param>
        /// <returns></returns>
        public ISchemaField AddField(string name, string dataType, bool nullable = true, string defaultValue = null, string size = null, string alias = null)
        {
            var schemaField = new SchemaField(this);
            schemaField.Name = name;
            schemaField.DataType = dataType;
            schemaField.Nullable = nullable;
            if (defaultValue != null)
                schemaField.DefaultValue = defaultValue;
            if (size != null)
                schemaField.Size = size;
            if (alias != null)
                schemaField.Alias = alias;
            this.Fields.Add(schemaField);
            return schemaField;
        }

        /// <summary>
        /// Adds a primary key constraint.
        /// </summary>
        /// <param name="name">Constraint name</param>
        /// <param name="fields">Constraint fields</param>
        /// <param name="sort">Constraint sort</param>
        /// <returns></returns>
        public IPrimaryKeySchemaConstraint AddPrimaryKeyConstraint(string name, List<string> fields, SchemaSort sort = SchemaSort.ssAsc)
        {
            var pkConstraint = new PrimaryKeySchemaConstraint(this);
            pkConstraint.Name = name;
            pkConstraint.Fields = fields;
            pkConstraint.Sort = sort;
            this.Constraints.Add(pkConstraint);
            return pkConstraint;
        }

        /// <summary>
        /// Adds the foreign key constraint.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="foreignKeySchemaObject">The foreign key schema object.</param>
        /// <param name="foreignKeySchemaFields">The foreign key schema fields.</param>
        /// <param name="onCascadeDelete">if set to <c>true</c> [on cascade delete].</param>
        /// <param name="onUpdateCascade">if set to <c>true</c> [on update cascade].</param>
        /// <returns></returns>
        public IForeignKeySchemaConstraint AddForeignKeyConstraint(string name, List<string> fields, string foreignKeySchemaObject, List<string> foreignKeySchemaFields,
            bool onCascadeDelete = true, bool onUpdateCascade = true)
        {
            var fkConstraint = new ForeignKeySchemaConstraint(this);
            fkConstraint.Name = name;
            fkConstraint.Fields = fields;
            fkConstraint.ForeignKeySchemaObject = foreignKeySchemaObject;
            fkConstraint.ForeignKeyFields = foreignKeySchemaFields;
            fkConstraint.OnDeleteCascade = onCascadeDelete;
            fkConstraint.OnUpdateCascade = onUpdateCascade;
            this.Constraints.Add(fkConstraint);
            return fkConstraint;
        }

        /// <summary>
        /// Adds a unique key constraint.
        /// </summary>
        /// <param name="name">Constraint name</param>
        /// <param name="fields">Constraint fields</param>
        /// <returns>
        /// An IUniqueKeySchemaConstraint
        /// </returns>
        public IUniqueKeySchemaConstraint AddUniqueKeyConstraint(string name, List<string> fields)
        {
            var ukConstraint = new UniqueKeySchemaConstraint(this);
            ukConstraint.Name = name;
            ukConstraint.Fields = fields;
            this.Constraints.Add(ukConstraint);
            return ukConstraint;
        }



        #endregion

        #region data related methods        
        /// <summary>
        /// Optional data to initialize a schema object. Practically applicable only to a table.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="data">The data.</param>
        public void SetData<T>(List<T> data)
        {
            var tablePrimaryKeyConstraints = this.Constraints.FindAll(constraint =>
            {
                return constraint.GetType().IsAssignableFrom(typeof(IPrimaryKeySchemaConstraint));
            });

            List<IOrpheusTableKeyField> tableKeys = new List<IOrpheusTableKeyField>();

            tablePrimaryKeyConstraints.ForEach(constraint =>
            {
                constraint.Fields.ForEach(field =>
                {
                    tableKeys.Add(new OrpheusTableKeyField() { Name = field, IsDBGenerated = false });
                });
            });

            this.seedDataTable = new OrpheusTable<T>(this.DB,tableKeys, this.SQLName);
            //
            //if (this.seedDataTable.Name != this.SQLName)
            //    this.seedDataTable.Name = this.SQLName;
            data.ForEach(dataRow =>
            {
                ((IOrpheusTable<T>)this.seedDataTable).Add(dataRow);
            });
            this.dataToSeed = data;
        }

        /// <summary>
        /// Returns the seed data for the table if defined.
        /// </summary>
        /// <typeparam name="T">Schema object model type</typeparam>
        /// <returns>
        /// Schema object's data
        /// </returns>
        public List<T> GetData<T>()
        {
            return (List<T>)this.dataToSeed;
        }
        #endregion;

        #region public methods

        /// <summary>
        /// Generate the schema's constraints DDL string.
        /// </summary>
        /// <returns>Returns the DDL string ready to be executed.</returns>
        public List<string> GetConstraintsDDL()
        {
            var list = new List<string>();
            this.Constraints.ForEach(constraint => {
                list.Add(String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL()));
            });
            return list;
        }

        /// <summary>
        /// Creates the schema object in the database.
        /// </summary>
        public override void Execute()
        {
            if (this.canExecuteSchema())
            {
                //first running any dependencies.
                switch (this.Action)
                {
                    case DDLAction.ddlCreate:
                        {
                            if (this.SchemaObjectsThatIDepend.Count > 0)
                            {
                                this.logger.LogTrace(this.formatLoggerMessage("Begin creating dependencies"));
                                this.SchemaObjectsThatIDepend.ForEach(dep => dep.Execute());
                                this.logger.LogTrace(this.formatLoggerMessage("End creating dependencies"));
                            }

                            break;
                        }
                    case DDLAction.ddlDrop:
                        {
                            if (this.SchemaObjectsThatDependOnMe.Count > 0)
                            {
                                this.logger.LogTrace(this.formatLoggerMessage("Begin dropping dependencies"));
                                this.SchemaObjectsThatDependOnMe.ForEach(dep => dep.Drop());
                                this.logger.LogTrace(this.formatLoggerMessage("End dropping dependencies"));
                            }
                            break;
                        }
                    //case DDLAction.ddlAlter:
                    //    {
                    //        if (this.SchemaObjectsThatDependOnMe.Count > 0)
                    //        {
                    //            this.logger.LogDebug(this.formatLoggerMessage("Begin altering dependencies"));
                    //            this.SchemaObjectsThatDependOnMe.ForEach(dep => dep.Execute());
                    //            this.logger.LogDebug(this.formatLoggerMessage("End altering dependencies"));
                    //        }
                    //        break;
                    //    }
                }
                //if there was a direct DDL SQL then ignore any other configuration and run it.
                var DDLString = this.RawDDL == null ? this.createDDLString() : new List<string>() { this.RawDDL };
                if (DDLString.Count > 0)
                {
                    //creating the schema and registering it in the schema objects table.
                    using (var transaction = this.DB.BeginTransaction())
                    {
                        //actually creating the new schema object.
                        var cmd = this.DB.CreateCommand();
                        cmd.Transaction = transaction;
                        try
                        {
                            foreach (var ddlCommand in DDLString)
                            {
                                try
                                {
                                    cmd.CommandText = ddlCommand;
                                    cmd.ExecuteNonQuery();
                                    switch (this.Action)
                                    {
                                        case DDLAction.ddlCreate:
                                            {
                                                if (this.UniqueKey == Guid.Empty)
                                                    this.UniqueKey = Guid.NewGuid();
                                                //apply constraints primary,foreign keys
                                                this.applyConstraints(cmd);
                                                //seeding any data if set.
                                                this.seedData(cmd);

                                                this.IsCreated = true;
                                                this.logger.LogTrace(this.formatLoggerMessage("Schema created"));
                                                break;
                                            }
                                        case DDLAction.ddlDrop:
                                            {
                                                this.IsCreated = false;
                                                this.logger.LogTrace(this.formatLoggerMessage("Schema dropped"));
                                                break;
                                            }
                                        case DDLAction.ddlAlter:
                                            {
                                                //apply constraints primary,foreign keys
                                                this.applyConstraints(cmd);
                                                this.IsCreated = true;
                                                this.seedData(cmd);
                                                this.logger.LogTrace(this.formatLoggerMessage("Schema altered"));
                                                break;
                                            }
                                    }
                                }
                                catch (Exception e)
                                {
                                    this.DB.RollbackTransaction(transaction);
                                    this.logger.LogError(ErrorCodes.ERR_SCHEMA_EXECUTE, this.formatLoggerMessage(ErrorDictionary.GetError(ErrorCodes.ERR_SCHEMA_EXECUTE)));
                                    this.logger.LogDebug(ddlCommand);
                                    this.logger.LogDebug(this.getConstraintsDDL());
                                    var ex = e;
                                    while (ex != null)
                                    {
                                        this.logger.LogError(ErrorCodes.ERR_SCHEMA_EXECUTE,ex,"");
                                        ex = ex.InnerException;
                                    }
                                    throw;
                                }
                            }
                            this.DB.CommitTransaction(transaction);
                            switch (Action)
                            {
                                //registering the newly created schema object.
                                case DDLAction.ddlCreate: { this.registerSchema(); break; }
                                case DDLAction.ddlDrop:
                                    {
                                        if (this.SQLName != this.Schema.SchemaObjectsTable)
                                            this.unRegisterSchema();
                                        this.SchemaObjectsThatDependOnMe.ForEach((obj) => {
                                            obj.SchemaObjectsThatIDepend.Remove(this);
                                        });
                                        break;
                                    }
                            }
                        }
                        finally
                        {
                            //transaction.Dispose();
                            cmd.Dispose();
                        }
                    }
                }
                else
                    this.logger.LogWarning(this.formatLoggerMessage("Did not create any DDL string"));

            }
        }

        /// <summary>
        /// Drops the schema from the database.
        /// </summary>
        public override void Drop()
        {
            DDLAction oldAction = this.Action;
            try
            {
                this.Action = DDLAction.ddlDrop;
                this.Execute();
            }
            finally
            {
                this.Action = oldAction;
            }
        }


        #endregion

        #region constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDataObject"/> class.
        /// </summary>
        public SchemaDataObject()
        {
            this.SchemaObjectsThatIDepend = new List<ISchemaObject>();
            this.SchemaObjectsThatDependOnMe = new List<ISchemaObject>();
            this.Fields = new List<ISchemaField>();
            this.Constraints = new List<ISchemaConstraint>();
       }
        #endregion
    }

    /// <summary>
    /// Derived class to specifically handle TABLE type schema objects.
    /// </summary>
    public class SchemaObjectTable: SchemaDataObject, ISchemaTable
    {
        /// <summary>
        /// Gets the <see cref="SchemaObjectType"/>.
        /// </summary>
        /// <returns></returns>
        protected override SchemaObjectType getType() { return SchemaObjectType.sotTable; }

        /// <summary>
        /// Creates the DDL string for the schema object.
        /// </summary>
        /// <returns></returns>
        protected override List<string> createDDLString()
        {
            
            var result = new List<string>();
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        var fields = new List<string>();
                        this.Fields.ForEach(fld => fields.Add(fld.SQL()));
                        result.Add(fields.Count > 0 ? String.Format("CREATE TABLE {0} ({1})", this.SQLName, string.Join(",", fields.ToArray())) : "");
                        break;
                    }
                case DDLAction.ddlDrop:
                    {
                        result.Add(String.Format("DROP TABLE {0}", this.SQLName)); 
                        break;
                    }
                case DDLAction.ddlAlter:
                    {
                        result.AddRange(this.modelHelper.GetAlterDDLCommands(this,this.DB.DDLHelper));
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// Applies schema's constraints.
        /// </summary>
        /// <param name="cmd"></param>
        protected override void applyConstraints(IDbCommand cmd)
        {
            //first run the primary key constraints.
            this.Constraints.Where(ct => ct.GetType() == typeof(PrimaryKeySchemaConstraint)).ToList().ForEach(constraint => {
                if(this.Action == DDLAction.ddlAlter)
                {
                    if (this.DB.DDLHelper.SchemaObjectExists(constraint))
                    {
                        //MySQL has a bit different SQL commands. TODO:Factor this out to the DDLHelper
                        if (this.DB.DDLHelper.DbEngineType == DatabaseEngineType.dbMySQL)
                        {
                            cmd.CommandText = String.Format("ALTER TABLE {0} DROP PRIMARY KEY", this.DB.DDLHelper.SafeFormatField(this.SQLName));
                        }
                        else 
                            cmd.CommandText = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}",this.SQLName, constraint.Name);
                        this.logger.LogDebug(cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.CommandText = String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL());
                this.logger.LogDebug(cmd.CommandText);
                cmd.ExecuteNonQuery();
            });

            //then the unique key constraints.
            this.Constraints.Where(ct => ct.GetType() == typeof(UniqueKeySchemaConstraint)).ToList().ForEach(constraint => {
                if (this.Action == DDLAction.ddlAlter)
                {
                    if (this.DB.DDLHelper.SchemaObjectExists(constraint.Name))
                    {
                        //MySQL has a bit different SQL commands. TODO:Factor this out to the DDLHelper
                        if (this.DB.DDLHelper.DbEngineType == DatabaseEngineType.dbMySQL)
                            cmd.CommandText = String.Format("ALTER TABLE {0} DROP INDEX {1}", this.DB.DDLHelper.SafeFormatField(this.SQLName), constraint.Name);
                        else
                            cmd.CommandText = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", this.SQLName, constraint.Name);
                        this.logger.LogDebug(cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.CommandText = String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL());
                this.logger.LogDebug(cmd.CommandText);
                cmd.ExecuteNonQuery();
            });

            //finally the foreign key constraints
            this.Constraints.Where(ct => ct.GetType() == typeof(ForeignKeySchemaConstraint)).ToList().ForEach(constraint => {
                if (this.Action == DDLAction.ddlAlter)
                {
                    if (this.DB.DDLHelper.SchemaObjectExists(constraint.Name))
                    {
                        //MySQL has a bit different SQL commands. TODO:Factor this out to the DDLHelper
                        if (this.DB.DDLHelper.DbEngineType == DatabaseEngineType.dbMySQL)
                            cmd.CommandText = String.Format("ALTER TABLE {0} DROP FOREIGN KEY {1}", this.DB.DDLHelper.SafeFormatField(this.SQLName), constraint.Name);
                        else
                            cmd.CommandText = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", this.SQLName, constraint.Name);
                        this.logger.LogDebug(cmd.CommandText);
                        cmd.ExecuteNonQuery();
                    }
                }

                cmd.CommandText = String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL());
                this.logger.LogDebug(cmd.CommandText);
                cmd.ExecuteNonQuery();
            });
        }

        /// <summary>
        /// Seeds the table with initial data.
        /// </summary>
        /// <param name="cmd"></param>
        protected override void seedData(IDbCommand cmd)
        {
            base.seedData(cmd);
            if(this.seedDataTable != null)
                this.seedDataTable.ExecuteInserts(cmd.Transaction);
        }

        /// <value>
        /// Table's Join definition. How and if this table is connected to other tables.
        /// </value>
        public ISchemaJoinDefinition JoinDefinition { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaObjectTable"/> class.
        /// </summary>
        public SchemaObjectTable() { }
    }

    /// <summary>
    /// Derived class to specifically handle data tables that are part of a view.
    /// </summary>
    public class SchemaObjectViewTable: SchemaObjectTable, ISchemaViewTable
    {
        /// <value>
        /// Override for the schema name.
        /// </value>
        public string SchemaName { get; set; }

        /// <summary>
        /// Overriding the default behavior, for view objects.
        /// </summary>
        /// <returns></returns>
        protected override string formatSQLName()
        {
            if (this.SchemaName == null)
                return base.formatSQLName();
            else
            {
                return String.Format("{0}.{1} {2}", this.SchemaName, this._sqlName, this.AliasName);
            }
        }

    }

    /// <summary>
    /// Derived class to specifically handle VIEW type schema objects.
    /// </summary>
    public class SchemaObjectView: SchemaDataObject, ISchemaView
    {
        /// <value>
        /// View's Join definition. How and if this table is connected to other tables.
        /// </value>
        public ISchemaJoinDefinition JoinDefinition { get; set; }

        /// <value>
        /// List of schema table to be included in the view.
        /// </value>
        public List<ISchemaTable> JoinSchemaObjects { get; set; }

        /// <value>
        /// The main table around which the view will be built.
        /// </value>
        public string TableName { get; set; }

        /// <value>
        /// SQL server specific option, to create views with schema binding, in order to be able to create indexes on the view itself.
        /// </value>
        public bool WithSchemaBinding { get; set; }

        /// <summary>
        /// Gets the <see cref="SchemaObjectType"/>.
        /// </summary>
        /// <returns></returns>
        protected override SchemaObjectType getType() { return SchemaObjectType.sotView; }

        ///// <summary>
        ///// Overriding the default behavior, for view objects.
        ///// </summary>
        ///// <returns></returns>
        //protected override string formatSQLName()
        //{
        //    return this._sqlName;
        //}

        /// <summary>
        /// Creates the DDL string for the schema object.
        /// </summary>
        /// <returns></returns>
        protected override List<string> createDDLString()
        {
            if (this.TableName == null)
                return null;
            var result = new List<string>();
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        var fields = new List<string>();
                        List<string> joins = new List<string>();
                        this.Fields.ForEach(fld => fields.Add(fld.FullFieldName));
                        this.JoinSchemaObjects.ForEach(joinObject => {
                            var joinType = "";
                            switch (joinObject.JoinDefinition.JoinType)
                            {
                                case SchemaJoinType.jtInner: { joinType = "INNER JOIN"; break; }
                                case SchemaJoinType.jtLeftInner: { joinType = "LEFT INNER JOIN"; break; }
                                case SchemaJoinType.jtLeftOuter: { joinType = "LEFT OUTER JOIN"; break; }
                                case SchemaJoinType.jtRightInner: { joinType = "RIGHT INNER JOIN"; break; }
                                case SchemaJoinType.jtRightOuter: { joinType = "RIGHT OUTER JOIN"; break; }
                                default: { joinType = "LEFT INNER JOIN"; break; }
                            }
                            var joinOperator = joinObject.JoinDefinition.JoinOperator == SchemaJoinOperator.joEquals ? "=" : "!=";
                            var joinString = String.Format(" {0} {1} ON {2} {3} {4} ",
                                joinType,
                                joinObject.SQLName,
                                joinObject.JoinDefinition.KeyField,
                                joinOperator,  
                                joinObject.JoinDefinition.JoinKeyField);
                            joins.Add(joinString);
                        });
                        if(this.WithSchemaBinding)
                            result.Add(String.Format("CREATE VIEW {0} WITH SCHEMABINDING AS SELECT {1} FROM {2} {3}",
                                this.SQLName,
                                string.Join(",", fields.ToArray()),
                                this.FormattedTableName(),
                                string.Join(Environment.NewLine, joins.ToArray()))
                                );
                        else
                        result.Add(String.Format("CREATE VIEW {0} AS SELECT {1} FROM {2} {3}",
                            this.SQLName,
                            string.Join(",", fields.ToArray()),
                            this.FormattedTableName(),
                            string.Join(Environment.NewLine, joins.ToArray()))
                            );
                        break;
                    }
                case DDLAction.ddlDrop:
                    {
                        result.Add(String.Format("DROP VIEW {0}", this.SQLName));
                        break;
                    }
                case DDLAction.ddlAlter:
                    {
                        throw new Exception("If you want to alter a SQL view, drop it and re-create it.");
                    }
            }
            return result;
        }


        /// <summary>
        /// Returns the main table name, SQL formatted, with a schema name, if the underlying db engine supports it, and with the table alias, if defined.
        /// </summary>
        /// <returns></returns>
        public string FormattedTableName()
        {
            if (this.Schema.Name == null)
                return String.Format("{0} {1}", this.TableName, this.AliasName);
            else
            {
                var schemaSeparator = this.Schema.DB.DDLHelperAs<ISQLServerDDLHelper>().SchemaSeparator;
                if(this.TableName.Contains(schemaSeparator))
                    return String.Format("{0} {1}", this.TableName, this.AliasName);
                else
                    return String.Format("{0}.{1} {2}", this.Schema.Name, this.TableName, this.AliasName);
            }
             
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaObjectView"/> class.
        /// </summary>
        public SchemaObjectView(){ }
    }
}
