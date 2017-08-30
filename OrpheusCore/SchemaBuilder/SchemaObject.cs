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
using Microsoft.Practices.Unity;
using Microsoft.Extensions.Logging;

namespace OrpheusCore.SchemaBuilder
{
    public class SchemaObject : ISchemaObject
    {

        private IDbCommand registerSchemaPreparedQuery;
        private IDbCommand unregisterSchemaPreparedQuery;
        private ILogger logger;
        private object dataToSeed;
        
        /// <summary>
        /// Table that holds the initial seed data for the schema.
        /// </summary>
        protected IOrpheusTable seedDataTable;

        /// <summary>
        /// Creates the DDL string for the schema object.
        /// </summary>
        /// <returns></returns>
        protected virtual string createDDLString()
        {
            return "";
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

        protected virtual void registerSchema(IDbTransaction transaction, string ddlString)
        {
            if (this.registerSchemaPreparedQuery == null)
            {
                this.registerSchemaPreparedQuery = this.DB.CreatePreparedQuery(String.Format("INSERT INTO {0} (ObjectName,ObjectId,ObjectDDL,CreatedOn,ObjectType) values (@ObjectName,@ObjectId,@ObjectDDL,@CreatedOn,@ObjectType)", this.Schema.SchemaObjectsTable));
                this.unregisterSchemaPreparedQuery = this.DB.CreatePreparedQuery(String.Format("DELETE FROM {0} WHERE ObjectId=@ObjectId", this.Schema.SchemaObjectsTable));
            }

            this.registerSchemaPreparedQuery.Transaction = transaction;
            ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ObjectId"]).Value = this.UniqueKey;
            ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ObjectName"]).Value = this.SQLName;
            ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ObjectDDL"]).Value = ddlString;
            ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@CreatedOn"]).Value = DateTime.Now;
            ((IDataParameter)this.registerSchemaPreparedQuery.Parameters["@ObjectType"]).Value = (int)this.GetSchemaType();
            this.registerSchemaPreparedQuery.ExecuteNonQuery();
        }

        protected virtual void unRegisterSchema(IDbTransaction transaction)
        {
            if (this.unregisterSchemaPreparedQuery == null)
            {
                this.unregisterSchemaPreparedQuery = this.DB.CreatePreparedQuery(String.Format("DELETE FROM {0} WHERE ObjectId=@ObjectId", this.Schema.SchemaObjectsTable));
            }
            ((IDataParameter)this.unregisterSchemaPreparedQuery.Parameters["@ObjectId"]).Value = this.UniqueKey;
            this.unregisterSchemaPreparedQuery.Transaction = transaction;
            this.unregisterSchemaPreparedQuery.ExecuteNonQuery();

        }

        /// <summary>
        /// Returns the schema type.
        /// </summary>
        /// <returns>Returns <see cref="SchemaObjectType"/></returns>
        protected virtual SchemaObjectType getType() { return SchemaObjectType.sotUnknown; }

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
        /// Generate the schema DDL string.
        /// </summary>
        /// <returns>Returns the DDL string ready to be executed.</returns>
        public string GetDDLString() { return this.createDDLString(); }

        /// <summary>
        /// Generate the schema's constraints DDL string.
        /// </summary>
        /// <returns>Returns the DDL string ready to be executed.</returns>
        public string GetConstraintsDDL()
        {
            var list = new StringBuilder();
            this.Constraints.ForEach(constraint => {
                list.Append(String.Format("ALTER TABLE {0} {1}", this.SQLName, constraint.SQL()));
            });
            return list.ToString();
        }

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
        /// Creates fields from a given model.
        /// Supports <see cref="System.ComponentModel.DataAnnotations"/> attributes
        /// </summary>
        /// <param name="model"></param>
        public void CreateFieldsFromModel(object model)
        {
            var modelAttributes = model.GetType().GetCustomAttributes(true);

            foreach (var modelAttribute in modelAttributes)
            {
                Type attributeType = modelAttribute.GetType();
                if(attributeType == typeof(PrimaryCompositeKey))
                {
                    PrimaryCompositeKey primaryKey = (PrimaryCompositeKey)modelAttribute;
                    var primaryKeyName = String.Format("PK_COMPOSITE_{0}_{1}", this.SQLName, String.Join("_", primaryKey.Fields));
                    this.AddPrimaryKeyConstraint(primaryKeyName, primaryKey.Fields.ToList());
                }

                if (attributeType == typeof(UniqueCompositeKey))
                {
                    UniqueCompositeKey uniqueKey = (UniqueCompositeKey)modelAttribute;
                    var uniqueKeyName = String.Format("UNQ_COMPOSITE_{0}_{1}", this.SQLName, String.Join("_", uniqueKey.Fields));
                    this.AddUniqueKeyConstraint(uniqueKeyName, uniqueKey.Fields.ToList());
                }
            }

            var modelProperties = model.GetType().GetProperties();
            var primaryKeys = new List<string>();
            foreach (var prop in modelProperties)
            {
                var propertyType = prop.PropertyType;
                var isNullable = this.DB.IsNullableType(propertyType);
                var isRequired = false;
                var dataType = this.DB.DDLHelper.TypeToString(propertyType.IsEnum ? typeof(int) : propertyType);
                var size = "";
                var isStringBlob = false;
                string defaultValue = null;

                //first try to find if there is a StringLengthAttribute defined.
                foreach(var attr in prop.GetCustomAttributes(true))
                {
                    Type attributeType = attr.GetType();
                    //if both DataAnnotations and Orpheus attributes are set,
                    //then Orpheus's take precedence.
                    if(attributeType == typeof(StringLengthAttribute))
                    {
                        size = (attr as StringLengthAttribute).MaximumLength.ToString();
                    }

                    if(attributeType == typeof(Length))
                    {
                        size = (attr as Length).Value.ToString();
                    }

                    if(attributeType == typeof(KeyAttribute) || attr.GetType() == typeof(PrimaryKey))
                    {
                        primaryKeys.Add(prop.Name);
                    }

                    if(attributeType == typeof(UniqueKey))
                    {
                        this.AddUniqueKeyConstraint(String.Format("UNQ_{0}_{1}",this.SQLName,prop.Name), new List<string>() { prop.Name });
                    }

                    if(attributeType == typeof(ForeignKey))
                    {
                        ForeignKey fk = (ForeignKey)attr;
                        this.AddForeignKeyConstraint(
                            String.Format("FK_{0}_{1}_{2}",this.SQLName,fk.ReferenceTable,prop.Name),
                            new List<string>() { prop.Name},
                            fk.ReferenceTable,
                            new List<string>() { fk.ReferenceField },
                            fk.OnDeleteCascade,
                            fk.OnUpdateCascade
                            );
                    }

                    if (attributeType == typeof(RequiredField))
                        isRequired = false;

                    if (attributeType == typeof(DefaultValue))
                        defaultValue = (attr as DefaultValue).Value.ToString();

                    if (attributeType == typeof(OrpheusAttributes.DataTypeAttribute))
                    {
                        OrpheusAttributes.DataTypeAttribute dataTypeAttr = (attr as OrpheusAttributes.DataTypeAttribute);
                        dataType = this.DB.DDLHelper.DbTypeToString(dataTypeAttr.DataType);
                        isStringBlob = (int)dataTypeAttr.DataType == (int)ExtendedDbTypes.StringBlob;
                    }
                }
                if (size.Length == 0 && !isStringBlob)
                {
                    if (propertyType == typeof(string))
                        size = "60";
                    if (propertyType == typeof(char) || propertyType == typeof(char?))
                        size = "1";
                }
                //a field cannot be nullable if it is marked with the required attribute or part of a primary key.
                isNullable = !primaryKeys.Contains(prop.Name) && this.Constraints.Where(cnt => cnt.GetType() == typeof(PrimaryKeySchemaConstraint) &&  cnt.Fields.Contains(prop.Name)).Count() == 0 && !isRequired;
                this.AddField(prop.Name, dataType, isNullable, defaultValue, size.Length > 0 ? size : null);
            }

            if(primaryKeys.Count > 0)
            {
                var primaryKeyName = String.Format("PK_{0}_{1}",this.SQLName,String.Join("_",primaryKeys));
                this.AddPrimaryKeyConstraint(primaryKeyName, primaryKeys);
            }
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
            var schemaField = OrpheusIocContainer.Resolve<ISchemaField>(new ResolverOverride[] {
                new ParameterOverride("schemaObject",this),
            });
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
            var pkConstraint = OrpheusIocContainer.Resolve<IPrimaryKeySchemaConstraint>(new ResolverOverride[] {
                new ParameterOverride("schemaObject",this),
            });
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
            var fkConstraint = OrpheusIocContainer.Resolve<IForeignKeySchemaConstraint>(new ResolverOverride[] {
                new ParameterOverride("schemaObject",this),
            });
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
        /// <param name="sort"></param>
        /// <returns></returns>
        public IUniqueKeySchemaConstraint AddUniqueKeyConstraint(string name, List<string> fields)
        {
            var ukConstraint = OrpheusIocContainer.Resolve<IUniqueKeySchemaConstraint>(new ResolverOverride[] {
                new ParameterOverride("schemaObject",this),
            });
            ukConstraint.Name = name;
            ukConstraint.Fields = fields;
            this.Constraints.Add(ukConstraint);
            return ukConstraint;
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

        /// <summary>
        /// Creates the schema object in the database.
        /// </summary>
        public void Execute()
        {
           
            if (this.SQLName == null || (this.IsCreated && this.Action != DDLAction.ddlDrop))
                return;
            //check if the schema object is registered in Orpheus.
            var existingGuid = this.Schema.SchemaObjectExists(this);

            //if it is then set its key value and stop execution.
            if (existingGuid != Guid.Empty)
            {
                this.UniqueKey = existingGuid;
                this.IsCreated = true;
                if(this.Action != DDLAction.ddlDrop)
                    return;
            }
            else
            {
                //if the schema object is not registered, verify that the schema object exists or not in the database.
                if(this.Action != DDLAction.ddlCreate && this.IsCreated)
                    this.logger.LogWarning("Schema object {0} is not a registered schema object in Orpheus.", this.SQLName);

                if (this.Action != DDLAction.ddlCreate && this.IsCreated)
                    this.logger.LogWarning("Checking if the object exists in the database.", this.SQLName);

                var objectExists = this.DB.DDLHelper.SchemaObjectExists(this.SQLName);
                if (this.Action != DDLAction.ddlCreate && this.IsCreated)
                {
                    if (objectExists)
                        this.logger.LogWarning("Object {0} found in the database.", this.SQLName);
                    else
                    {
                        this.logger.LogWarning("Object {0} not found in the database.", this.SQLName);
                    }
                }
                switch (this.Action)
                {
                    case DDLAction.ddlDrop:
                        {
                            //if the object does not exist, then do nothing.
                            if (!objectExists)
                                return;
                            break;
                        }
                    case DDLAction.ddlCreate:
                        {
                            //if the object exists but it's not registered, then register it.
                            if (objectExists)
                            {
                                using (var transaction = this.DB.BeginTransaction())
                                {
                                    try
                                    {
                                        this.registerSchema(transaction, this.RawDDL == null ? this.createDDLString() : this.RawDDL);
                                        return;
                                    }
                                    catch(Exception e)
                                    {
                                        this.logger.LogError(e.Message);
                                    }
                                }
                            }
                            break;
                        }
                }
            }

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


            var DDLString = "";
            //if there was a direct DDL SQL then ignore any other configuration and run it.
            if (this.RawDDL != null)
            {
                DDLString = this.RawDDL;
            }
            else
            {
                DDLString = this.createDDLString();
            }
               
            //creating the schema and registering it in the schema objects table.
            using (var transaction = this.DB.BeginTransaction())
            {
                //actually creating the new schema object.
                var cmd = this.DB.CreateCommand();
                cmd.Transaction = transaction;
                if (DDLString.Length > 0)
                {
                    try
                    {
                        cmd.CommandText = DDLString;
                        cmd.ExecuteNonQuery();
                        switch (this.Action)
                        {
                            case DDLAction.ddlCreate:
                                {
                                    if (this.UniqueKey == Guid.Empty)
                                        this.UniqueKey = Guid.NewGuid();
                                    //registering the newly created schema object.
                                    this.registerSchema(transaction, DDLString);
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
                                    if(this.SQLName != this.Schema.SchemaObjectsTable)
                                        this.unRegisterSchema(transaction);
                                    this.IsCreated = false;
                                    this.logger.LogDebug("Dropped schema {0}", this.SQLName);
                                    break;
                                }
                        }
                        transaction.Commit();
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
                        if(this.Action == DDLAction.ddlCreate)
                            throw e;
                    }
                    finally
                    {
                        transaction.Dispose();
                        cmd.Dispose();
                    }
                }
                else
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    cmd.Dispose();
                    this.logger.LogWarning("Schema object {0} did not create any DDL string.", this.SQLName);
                }

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
            if(modelType != null)
            {
                this.AddDependency(this.Schema.SchemaObjects.Where(obj => obj.SQLName.ToLower() == modelType.Name.ToLower()).First());
            }
        }

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
            this.logger = OrpheusIocContainer.Resolve<ILogger>();
        }
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
        protected override SchemaObjectType getType() { return SchemaObjectType.sotCreateTable; }

        /// <summary>
        /// Creates the DDL string for the schema object.
        /// </summary>
        /// <returns></returns>
        protected override string createDDLString()
        {
            
            var result = "";
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        var fields = new List<string>();
                        this.Fields.ForEach(fld => fields.Add(fld.SQL()));
                        result = fields.Count > 0 ? String.Format("CREATE TABLE {0} ({1})", this.SQLName, string.Join(",", fields.ToArray())) : "";
                        break;
                    }
                case DDLAction.ddlDrop:
                    {
                        result = String.Format("DROP TABLE {0}", this.SQLName); 
                        break;
                    }
                case DDLAction.ddlAlter:
                    {
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
            this.Constraints.ForEach(constraint => {
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
        protected override SchemaObjectType getType() { return SchemaObjectType.sotCreateView; }

        /// <summary>
        /// Creates the DDL string for the schema object.
        /// </summary>
        /// <returns></returns>
        protected override string createDDLString()
        {
            if (this.TableName == null)
                return null;
            var result = "";
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        var fields = new List<string>();
                        List<string> joins = new List<string>();
                        this.Fields.ForEach(fld => fields.Add(String.Format("{0}.{1}", this.SQLName, fld.SQL())));
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
                            var joinString = String.Format(" {0} ON {1}.{2} {3} {4}.{5} ",
                                joinType,
                                this.SQLName,
                                joinObject.JoinDefinition.KeyField,
                                joinOperator, joinObject.SQLName,
                                joinObject.JoinDefinition.JoinKeyField);
                            joins.Add(joinString);
                        });
                        result = String.Format("CREATE VIEW {0} AS SELECT {1} FROM {2}", this.SQLName, string.Join(",", fields.ToArray()), this.TableName, string.Join(",", joins.ToArray()));
                        break;
                    }
                case DDLAction.ddlDrop:
                    {
                        result = String.Format("DROP VIEW {0}", this.SQLName);
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
