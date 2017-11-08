using OrpheusCore;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using OrpheusAttributes;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace OrpheusCore.SchemaBuilder
{
    /// <summary>
    /// Base schema object.
    /// </summary>
    public class SchemaObject : ISchemaObject
    {
        #region private fields
        private IDbCommand registerSchemaPreparedQuery;
        private IDbCommand unregisterSchemaPreparedQuery;
        private ILogger logger;
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
        /// Creates the DDL string for the schema object.
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> createDDLString()
        {
            return null;
        }

        /// <summary>
        /// Applies schema constraints.
        /// </summary>
        /// <param name="cmd" type="IDbCommand"></param>
        protected virtual void applyConstraints(IDbCommand cmd)
        {

        }

        /// <summary>
        /// Inserts data to the DB engine, if the schema is a Table.
        /// </summary>
        /// <param name="cmd"></param>
        protected virtual void seedData(IDbCommand cmd)
        {

        }

        /// <summary>
        /// Registers an Orpheus schema.
        /// </summary>
        /// <param name="transaction"></param>
        protected virtual void registerSchema(IDbTransaction transaction)
        {
            if(this.Schema.SchemaObjectExists(this) == Guid.Empty && this.SQLName != this.Schema.SchemaInfoTable)
            {
                if (this.registerSchemaPreparedQuery == null)
                {
                    this.registerSchemaPreparedQuery = this.DB.CreatePreparedQuery(String.Format("INSERT INTO {0} (ObjectName,ObjectId,DDL,ConstraintsDDL,CreatedOn,ObjectType,SchemaId) values (@ObjectName,@ObjectId,@DDL,@ConstraintsDDL,@CreatedOn,@ObjectType,@SchemaId)", this.Schema.SchemaObjectsTable));
                }

                this.registerSchemaPreparedQuery.Transaction = transaction;
                ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ObjectId"]).Value = this.UniqueKey;
                ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ObjectName"]).Value = this.SQLName;
                ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@DDL"]).Value = string.Join(",",this.GetDDLString().ToArray());
                ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ConstraintsDDL"]).Value = string.Join(",", this.GetConstraintsDDL().ToArray());
                ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@CreatedOn"]).Value = DateTime.Now;
                ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ObjectType"]).Value = (int)this.GetSchemaType();
                ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@SchemaId"]).Value = this.Schema.Id;
                this.registerSchemaPreparedQuery.ExecuteNonQuery();
            }

        }

        /// <summary>
        /// Unregisters an Orpheus schema.
        /// </summary>
        /// <param name="transaction"></param>
        protected virtual void unRegisterSchema(IDbTransaction transaction)
        {
            if(this.Schema.SchemaObjectExists(this) != Guid.Empty)
            {
                if (this.unregisterSchemaPreparedQuery == null)
                {
                    this.unregisterSchemaPreparedQuery = this.DB.CreatePreparedQuery(String.Format("DELETE FROM {0} WHERE ObjectId=@ObjectId", this.Schema.SchemaObjectsTable));
                }
            ((IDataParameter)this.unregisterSchemaPreparedQuery.Parameters["@ObjectId"]).Value = this.UniqueKey;
                this.unregisterSchemaPreparedQuery.Transaction = transaction;
                this.unregisterSchemaPreparedQuery.ExecuteNonQuery();
            }
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

            //does the schema object actually exist in the database?
            this.objectExistsInDatabase = this.DB.DDLHelper.SchemaObjectExists(this.SQLName);

            if(!this.objectExistsInDatabase && this.Action == DDLAction.ddlDrop)
            {
                this.logger.LogWarning("Object {0} not found in the database.", this.SQLName);
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
        /// Schema Type
        /// </summary>
        /// <returns>Returns <see cref="SchemaObjectType"/></returns>
        public SchemaObjectType GetSchemaType() { return this.getType(); }

        /// <summary>
        /// Database that the schema object belongs to.
        /// Taken from the <see cref="ISchema"/> property.
        /// </summary>
        public IOrpheusDatabase DB { get { return this.Schema.DB; } }

        /// <summary>
        /// The Schema where the schema object belongs to.
        /// </summary>
        public ISchema Schema { get; set; }

        /// <summary>
        /// Overrides any schema object configuration and executes RawDLL contents.
        /// </summary>
        public string RawDDL { get; set; }

        /// <summary>
        /// A list of other schema objects that this instance depends upon. Any schema object in this list
        /// will be executed before, to make sure that on the time of creation they will be available/created.
        /// </summary>
        public List<ISchemaObject> SchemaObjectsThatIDepend { get; set; }

        /// <summary>
        /// A list of other schema objects that depend on this instance.
        /// </summary>
        public List<ISchemaObject> SchemaObjectsThatDependOnMe { get; set; }

        /// <summary>
        /// The list of fields of the schema object.
        /// </summary>
        public List<ISchemaField> Fields { get; set; }

        /// <summary>
        /// The list of constraints of the schema object.
        /// </summary>
        public List<ISchemaConstraint> Constraints { get; set; }

        /// <summary>
        /// Flag to indicate if the schema object has actually been created in the database.
        /// </summary>
        public bool IsCreated { get; private set; }

        /// <summary>
        /// A descriptive name for the schema object.
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        /// The SQL name for the schema. This will be used to actually create the schema object in the database.
        /// </summary>
        public string SQLName { get; set; }

        /// <summary>
        /// An Id to uniquely identify this schema object.
        /// </summary>
        public Guid UniqueKey { get; set; }

        /// <summary>
        /// Defines the behavior of execute. <see cref="DDLAction"/>
        /// </summary>
        public DDLAction Action { get; set; }
        #endregion

        #region schema fields and constraints
        /// <summary>
        /// Creates fields from a given model.
        /// Supports <see cref="System.ComponentModel.DataAnnotations"/> attributes
        /// </summary>
        /// <param name="model"></param>
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
            //if there was a [TableName] attribute set, then it takes precedence.
            if (this.modelHelper.SQLName != null)
                this.SQLName = this.modelHelper.SQLName;
        }


        /// <summary>
        /// Adds a field.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        /// <param name="nullable"></param>
        /// <param name="defaultValue"></param>
        /// <param name="size"></param>
        /// <param name="alias"></param>
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
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <param name="sort"></param>
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
        /// Adds a foreign key constraint.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <param name="foreignKeySchemaObject"></param>
        /// <param name="foreignKeySchemaFields"></param>
        /// <param name="onCascadeDelete"></param>
        /// <param name="onUpdateCascade"></param>
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
        /// <param name="name"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IUniqueKeySchemaConstraint AddUniqueKeyConstraint(string name, List<string> fields)
        {
            var ukConstraint = new UniqueKeySchemaConstraint(this);
            ukConstraint.Name = name;
            ukConstraint.Fields = fields;
            this.Constraints.Add(ukConstraint);
            return ukConstraint;
        }

        /// <summary>
        /// Adds a dependency to a schema object.
        /// </summary>
        /// <param name="schemaObject"></param>
        public void AddDependency(ISchemaObject schemaObject)
        {
            if (schemaObject != null)
            {
                this.SchemaObjectsThatIDepend.Add(schemaObject);
                schemaObject.SchemaObjectsThatDependOnMe.Add(this);
            }
        }

        /// <summary>
        /// Adds a dependency to a schema object based on the model.
        /// </summary>
        /// <param name="modelType"></param>
        public void AddDependency(Type modelType)
        {
            if (modelType != null)
            {
                this.AddDependency(this.Schema.SchemaObjects.Where(obj => obj.SQLName.ToLower() == modelType.Name.ToLower()).First());
            }
        }

        /// <summary>
        /// Adds a dependency to a schema object based on the model type.
        /// </summary>
        public void AddDependency<T>() where T : class
        {
            this.AddDependency(typeof(T));
        }

        #endregion

        #region data related methods
        /// <summary>
        /// Sets seed data to seed the schema object when constructed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
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


            this.seedDataTable = new OrpheusTable<T>(this.DB, this.SQLName, tableKeys);
            data.ForEach(dataRow =>
            {
                ((IOrpheusTable<T>)this.seedDataTable).Add(dataRow);
            });
            this.dataToSeed = data;
        }

        /// <summary>
        /// Returns the seed data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetData<T>()
        {
            return (List<T>)this.dataToSeed;
        }
        #endregion;

        #region public methods

        /// <summary>
        /// Generate the schema DDL string.
        /// </summary>
        /// <returns>Returns the DDL string ready to be executed.</returns>
        public List<string> GetDDLString() { return this.createDDLString(); }

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
        public void Execute()
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
                                this.logger.LogDebug("========= Start creating dependencies for {0}=========", this.SQLName);
                                this.SchemaObjectsThatIDepend.ForEach(dep => dep.Execute());
                                this.logger.LogDebug("========= End creating dependencies for {0}=========", this.SQLName);
                            }

                            break;
                        }
                    case DDLAction.ddlDrop:
                        {
                            if (this.SchemaObjectsThatDependOnMe.Count > 0)
                            {
                                this.logger.LogDebug("========= Start dropping dependencies for {0}=========", this.SQLName);
                                this.SchemaObjectsThatDependOnMe.ForEach(dep => dep.Drop());
                                this.logger.LogDebug("========= End dropping dependencies for {0}=========", this.SQLName);
                            }
                            break;
                        }
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
                                                //registering the newly created schema object.
                                                this.registerSchema(transaction);
                                                //apply constraints primary,foreign keys
                                                this.applyConstraints(cmd);
                                                //seeding any data if set.
                                                this.seedData(cmd);

                                                this.IsCreated = true;
                                                this.logger.LogDebug("Created schema {0}", this.SQLName);
                                                break;
                                            }
                                        case DDLAction.ddlDrop:
                                            {
                                                if (this.SQLName != this.Schema.SchemaObjectsTable)
                                                    this.unRegisterSchema(transaction);
                                                this.IsCreated = false;
                                                this.logger.LogDebug("Dropped schema {0}", this.SQLName);
                                                break;
                                            }
                                        case DDLAction.ddlAlter:
                                            {
                                                //apply constraints primary,foreign keys
                                                this.applyConstraints(cmd);
                                                this.IsCreated = true;
                                                this.seedData(cmd);
                                                this.logger.LogDebug("Altered schema {0}", this.SQLName);
                                                break;
                                            }
                                    }
                                }
                                catch (Exception e)
                                {
                                    transaction.Rollback();
                                    var ex = e;
                                    while (ex != null)
                                    {
                                        this.logger.LogError(ex.Message);
                                        ex = ex.InnerException;
                                    }
                                    this.logger.LogError(ddlCommand);
                                    this.logger.LogError(this.getConstraintsDDL());
                                    throw e;
                                }
                            }
                            transaction.Commit();
                        }
                        finally
                        {
                            transaction.Dispose();
                            cmd.Dispose();
                        }
                    }
                }
                else
                    this.logger.LogWarning("Schema object {0} did not create any DDL string.", this.SQLName);

            }
        }

        /// <summary>
        /// Drops the schema from the database.
        /// </summary>
        public void Drop()
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
        /// Constructor.
        /// </summary>
        public SchemaObject()
        {
            this.SchemaObjectsThatIDepend = new List<ISchemaObject>();
            this.SchemaObjectsThatDependOnMe = new List<ISchemaObject>();
            this.Fields = new List<ISchemaField>();
            this.Constraints = new List<ISchemaConstraint>();
            this.IsCreated = false;
            this.logger = ServiceProvider.OrpheusServiceProvider.Resolve<ILogger>();
        }
        #endregion
    }

    /// <summary>
    /// Derived class to specifically handle TABLE type schema objects.
    /// </summary>
    public class SchemaObjectTable:SchemaObject,ISchemaTable
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
                        result.AddRange(this.modelHelper.GetAlterDDLCommands(this));
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// Applies schema's constraints.
        /// </summary>
        /// <param name="cmd"><see cref="IDbCommand"/></param>
        protected override void applyConstraints(IDbCommand cmd)
        {
            //first run the primary key constraints.
            this.Constraints.Where(ct => ct.GetType() == typeof(PrimaryKeySchemaConstraint)).ToList().ForEach(constraint => {
                if(this.Action == DDLAction.ddlAlter)
                {
                    if (this.DB.DDLHelper.SchemaObjectExists(constraint.Name))
                    {
                        cmd.CommandText = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}",this.SQLName,constraint.Name);
                        cmd.ExecuteNonQuery();
                    }
                }
                cmd.CommandText = String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL());
                cmd.ExecuteNonQuery();
            });

            //then the unique key constraints.
            this.Constraints.Where(ct => ct.GetType() == typeof(UniqueKeySchemaConstraint)).ToList().ForEach(constraint => {
                if (this.Action == DDLAction.ddlAlter)
                {
                    if (this.DB.DDLHelper.SchemaObjectExists(constraint.Name))
                    {
                        cmd.CommandText = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", this.SQLName, constraint.Name);
                        cmd.ExecuteNonQuery();
                    }
                }
                cmd.CommandText = String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL());
                cmd.ExecuteNonQuery();
            });

            //finally the foreign key constraints
            this.Constraints.Where(ct => ct.GetType() == typeof(ForeignKeySchemaConstraint)).ToList().ForEach(constraint => {
                if (this.Action == DDLAction.ddlAlter)
                {
                    if (this.DB.DDLHelper.SchemaObjectExists(constraint.Name))
                    {
                        cmd.CommandText = String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", this.SQLName, constraint.Name);
                        cmd.ExecuteNonQuery();
                    }
                }
                cmd.CommandText = String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL());
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

        /// <summary>
        /// Table's Join definition. How and if this table is connected to other tables.
        /// </summary>
        public ISchemaJoinDefinition JoinDefinition { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SchemaObjectTable() { }
    }


    /// <summary>
    /// Derived class to specifically handle VIEW type schema objects.
    /// </summary>
    public class SchemaObjectView:SchemaObject, ISchemaView
    {
        /// <summary>
        /// View's Join definition. How and if this table is connected to other tables.
        /// </summary>
        public ISchemaJoinDefinition JoinDefinition { get; set; }

        /// <summary>
        /// List of schema table to be included in the view.
        /// </summary>
        public List<ISchemaTable> JoinSchemaObjects { get; set; }

        /// <summary>
        /// The main table around which the view will be built.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets the <see cref="SchemaObjectType"/>.
        /// </summary>
        /// <returns></returns>
        protected override SchemaObjectType getType() { return SchemaObjectType.sotView; }

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
                        result.Add(String.Format("CREATE VIEW {0} AS SELECT {1} FROM {2} {3}", 
                            this.SQLName, 
                            string.Join(",", fields.ToArray()), 
                            this.TableName, 
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
        /// Constructor.
        /// </summary>
        public SchemaObjectView(){ }
    }
}
