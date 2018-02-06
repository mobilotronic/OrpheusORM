using OrpheusAttributes;
using OrpheusCore.SchemaBuilder;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OrpheusCore
{
    /// <summary>
    /// Helper delegate, to be called when iterating through model's properties.
    /// </summary>
    /// <param name="property"></param>
    public delegate void ModelProperty(PropertyInfo property);
    
    /// <summary>
    /// Helper delegate, to be called when iterating through property's attributes.
    /// </summary>
    /// <param name="attribute"></param>
    public delegate void PropertyAttribute(Attribute attribute);

    /// <summary>
    /// OrpheusModelHelper is a helper class that analyzes a model and can create primary-foreign keys and/or schema fields, when creating a schema.
    /// </summary>
    public class OrpheusModelHelper
    {
        #region private fields
        private Type modelType;
        //caching in memory of properties and attributes, to improve performance.
        private PropertyInfo[] modelProperties;
        private Dictionary<PropertyInfo, object[]> propertyAttributes;
        #endregion

        #region private methods
        private PropertyInfo[] getModelProperties()
        {
            if (modelProperties == null)
            {
                modelProperties = this.modelType.GetProperties();
                Array.Sort<PropertyInfo>(modelProperties, delegate (PropertyInfo p1, PropertyInfo p2) { return p1.Name.CompareTo(p2.Name); });
                this.propertyAttributes = new Dictionary<PropertyInfo, object[]>();
                foreach(var prop in this.modelProperties)
                {
                    this.propertyAttributes.Add(prop, prop.GetCustomAttributes(true));
                }
            }
            return modelProperties;
        }

        private object[] getPropertyAttributes(PropertyInfo prop)
        {
            return this.propertyAttributes[prop];
        }

        /// <summary>
        /// Helper method that creates schema fields.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="schemaObj"></param>
        private void createSchemaField(PropertyInfo prop, ISchemaDataObject schemaObj)
        {
            var isNullable = schemaObj.DB.IsNullableType(prop.PropertyType);
            var isRequired = false;
            var dataType = schemaObj.DB.DDLHelper.TypeToString(prop.PropertyType.IsEnum ? typeof(int) : prop.PropertyType);
            var size = "";
            var isStringBlob = false;
            var isPrimaryKey = false;
            string defaultValue = null;
            var fieldName = this.GetFieldNameForProperty(prop);
            var isSchemaIgnore = false;
            this.IteratePropertyAttributes(prop, (Attribute attr) => {
                Type attributeType = attr.GetType();
                #region schema fields
                if (this.IsSchemaProperty(prop))
                {
                    if (attributeType == typeof(PrimaryKey))
                    {
                        isPrimaryKey = true;
                        var primaryKeyName = String.Format("PK_{0}_{1}", schemaObj.SQLName, fieldName);
                        schemaObj.AddPrimaryKeyConstraint(primaryKeyName, new List<string>() { fieldName });
                    }

                    if (attributeType == typeof(Length))
                    {
                        size = (attr as Length).Value.ToString();
                    }

                    if (attributeType == typeof(UniqueKey))
                    {
                        schemaObj.AddUniqueKeyConstraint(String.Format("UNQ_{0}_{1}", schemaObj.SQLName, fieldName), new List<string>() { fieldName });
                    }

                    if (attributeType == typeof(ForeignKey))
                    {
                        ForeignKey fk = (ForeignKey)attr;
                        schemaObj.AddForeignKeyConstraint(
                            String.Format("FK_{0}_{1}_{2}", schemaObj.SQLName, fk.ReferenceTable, fieldName),
                            new List<string>() { fieldName },
                            fk.ReferenceTable,
                            new List<string>() { fk.ReferenceField },
                            fk.OnDeleteCascade,
                            fk.OnUpdateCascade
                            );
                    }

                    if (attributeType == typeof(RequiredField))
                    {
                        isRequired = true;
                    }

                    if (attributeType == typeof(DefaultValue))
                    {
                        var defVal = (attr as DefaultValue).Value;
                        if (defVal == null)
                            isNullable = true;
                        else
                            defaultValue = defVal.GetType() == typeof(string) ? String.Format("'{0}'", defVal.ToString()) : defVal.ToString();
                    }

                    if (attributeType == typeof(DataTypeAttribute))
                    {
                        DataTypeAttribute dataTypeAttr = (attr as DataTypeAttribute);
                        dataType = schemaObj.DB.DDLHelper.DbTypeToString(dataTypeAttr.DataType);
                        isStringBlob = (int)dataTypeAttr.DataType == (int)ExtendedDbTypes.StringBlob;
                    }
                }
                else
                    isSchemaIgnore = true;

                #endregion
            });
            //if there was no explicit size set, then if the type of the property
            //is a string or a char, set a size value.
            if (size.Length == 0 && !isStringBlob)
            {
                var isStringType = false;
                if (prop.PropertyType == typeof(string))
                {
                    size = Configuration.ConfigurationManager.Configuration.DefaultStringSize.ToString();
                    isStringType = true;
                }
                if (prop.PropertyType == typeof(char) || prop.PropertyType == typeof(char?))
                {
                    size = "1";
                    isStringType = true;
                }
                if (isStringType)
                    isNullable = true;
            }
            if (!isSchemaIgnore)
            {
                //a field cannot be nullable if it is marked with the required attribute or part of a primary key.
                if (isPrimaryKey || isRequired)
                    isNullable = false;
                schemaObj.AddField(fieldName, dataType, isNullable, defaultValue, size.Length > 0 ? size : null);
            }
        }

        /// <summary>
        /// Iterates through the model properties to find out primary, foreign keys.
        /// </summary>
        private void intializeModelProperties()
        {
            var modelAttributes = this.modelType.GetCustomAttributes(true);

            foreach (var modelAttribute in modelAttributes)
            {
                Type attributeType = modelAttribute.GetType();
                if (attributeType == typeof(PrimaryCompositeKey))
                {
                    this.PrimaryCompositeKeys.Add((PrimaryCompositeKey)modelAttribute);
                }

                if (attributeType == typeof(UniqueCompositeKey))
                {
                    this.UniqueCompositeKeys.Add((UniqueCompositeKey)modelAttribute);
                }

                if(attributeType == typeof(TableName))
                {
                    this.SQLName = (modelAttribute as TableName).Name;
                }
            }

            this.IterateModelProperties((PropertyInfo prop) => {
                this.IteratePropertyAttributes(prop, (Attribute attr) => {
                    Type attributeType = attr.GetType();
                    #region primary keys
                    if (attributeType == typeof(PrimaryKey))
                    {
                        this.PrimaryKeys.Add(prop.Name, (PrimaryKey)attr);
                    }
                    #endregion

                    #region foreign keys
                    if (attributeType == typeof(ForeignKey))
                        this.ForeignKeys.Add(prop.Name, (ForeignKey)attr);
                    #endregion

                    #region foreign keys
                    if (attributeType == typeof(UniqueKey))
                       this.UniqueKeys.Add(prop.Name, (UniqueKey)attr);
                    #endregion

                    #region schema ignore properties
                    if (attributeType == typeof(SchemaIgnore))
                    {
                        this.SchemaIgnoreProperties.Add(prop.Name);
                    }
                    #endregion

                    #region custom field name properties
                    if (attr.GetType() == typeof(FieldName))
                    {
                        this.CustomFieldNameProperties.Add(prop.Name, (attr as FieldName).Name);
                    }
                    #endregion
                });
            });

        }

        #endregion

        #region public properties
        
        /// <summary>
        /// Model's primary keys.
        /// </summary>
        public Dictionary<string,PrimaryKey> PrimaryKeys { get; private set; }
        
        /// <summary>
        /// Model's foreign keys.
        /// </summary>
        public Dictionary<string,ForeignKey> ForeignKeys { get; private set; }
        
        /// <summary>
        /// Model's unique keys.
        /// </summary>
        public Dictionary<string,UniqueKey> UniqueKeys { get; private set; }
       
        /// <summary>
        /// Model's composite primary keys.
        /// </summary>
        public List<PrimaryCompositeKey> PrimaryCompositeKeys { get; private set; }
        
        /// <summary>
        /// Model's composite unique keys.
        /// </summary>
        public List<UniqueCompositeKey> UniqueCompositeKeys { get; private set; }
       
        /// <summary>
        /// Model properties that are not part of the schema.
        /// </summary>
        public List<string> SchemaIgnoreProperties { get; private set; }
        
        /// <summary>
        /// Model properties that have an explicitly set field name.
        /// </summary>
        public Dictionary<string,string> CustomFieldNameProperties { get; private set; }
        
        /// <summary>
        /// Model's properties.
        /// </summary>
        public PropertyInfo[] ModelProperties { get { return this.getModelProperties(); } }
        
        /// <summary>
        /// Model's SQLName. Defaults to the model's type name.
        /// </summary>
        public string SQLName { get; private set; }
        #endregion

        #region public methods
        /// <summary>
        /// Helper function to iterate through the model properties and calls the given callback,
        /// for each property.
        /// </summary>
        /// <param name="callback"></param>
        public void IterateModelProperties(ModelProperty callback)
        {
            foreach (var prop in this.getModelProperties())
            {
                if (callback != null)
                {
                    callback(prop);
                }
            }
        }

        /// <summary>
        /// Helper function to iterate through the property's attributes and calls the given callback,
        /// for each attribute.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="callback"></param>
        public void IteratePropertyAttributes(PropertyInfo property, PropertyAttribute callback)
        {
            foreach (var attr in this.getPropertyAttributes(property))
            {
                if (callback != null)
                {
                    callback((Attribute)attr);
                }
            }
        }

        /// <summary>
        /// Helper function that returns true if the property is not actually part of the schema.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public bool IsSchemaProperty(PropertyInfo property)
        {
            return this.SchemaIgnoreProperties.Where(p => p.ToLower() == property.Name.ToLower()).Count() == 0;
        }

        /// <summary>
        /// Helper function that returns the corresponding field name for a property.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public string GetFieldNameForProperty(PropertyInfo prop)
        {
            return this.CustomFieldNameProperties.ContainsKey(prop.Name) ? this.CustomFieldNameProperties[prop.Name] : prop.Name;
        }

        /// <summary>
        /// Creates an instance of the model and typecasts it to the given type.
        /// </summary>
        /// <typeparam name="T">Type to cast the model</typeparam>
        /// <returns></returns>
        public T CreateInstance<T>()
        {
            return (T)Activator.CreateInstance(modelType);
        }

        /// <summary>
        /// Creates schema fields and constraints for a model.
        /// </summary>
        /// <param name="schemaObj"></param>
        public void CreateSchemaFields(ISchemaDataObject schemaObj)
        {
            this.PrimaryCompositeKeys.ForEach(pk => {
                var primaryKeyName = String.Format("PK_COMPOSITE_{0}_{1}", schemaObj.SQLName, String.Join("_", pk.Fields));
                schemaObj.AddPrimaryKeyConstraint(primaryKeyName, pk.Fields.ToList());
            });

            this.UniqueCompositeKeys.ForEach(uniqueyKey => {
                var uniqueKeyName = String.Format("UNQ_COMPOSITE_{0}_{1}", schemaObj.SQLName, String.Join("_", uniqueyKey.Fields));
                schemaObj.AddUniqueKeyConstraint(uniqueKeyName, uniqueyKey.Fields.ToList());
            });

            this.IterateModelProperties((PropertyInfo prop) => {
                this.createSchemaField(prop, schemaObj);
            });
        }

        /// <summary>
        /// Creates a list of SQL ALTER commands, based on the differences between the current version of the model
        /// and the current version of the corresponding db table.
        /// </summary>
        /// <param name="schemaObj"></param>
        /// <returns></returns>
        public List<string> GetAlterDDLCommands(ISchemaDataObject schemaObj)
        {
            var result = new List<string>();
            var selectCommand = schemaObj.DB.CreateCommand();
            selectCommand.CommandText = String.Format("SELECT * FROM {0} WHERE 0=1", schemaObj.SQLName);
            var deletedFields = new List<string>();
            var newFields = new List<ISchemaField>();
            var alteredTypeFields = new List<string>();
            var tableFields = new List<string>();
            IDataReader dataReader = null;
            try
            {
                dataReader = selectCommand.ExecuteReader();
                dataReader.Read();
                //iterating first to find which table columns do not exist in the model
                for (var i = 0; i <= dataReader.FieldCount - 1; i++)
                {
                    var fieldName = dataReader.GetName(i);
                    tableFields.Add(fieldName);
                    var modelProperty = this.ModelProperties.FirstOrDefault(mp => mp.Name.ToLower() == fieldName.ToLower());
                    if (modelProperty == null)
                    {
                        deletedFields.Add(schemaObj.DB.DDLHelper.SafeFormatField(fieldName));
                    }
                    else
                    {
                        var fieldType = dataReader.GetFieldType(i);
                        var isNullable = Nullable.GetUnderlyingType(modelProperty.PropertyType) != null;
                        //nullable types are treated the same from the DB perspective, so we don't want to do any changes to nullable types.
                        //all enum types are treated a integers
                        //if the field exists but the data type has changed.
                        if (!isNullable && !modelProperty.PropertyType.IsEnum && modelProperty.PropertyType != fieldType)
                        {
                            var existingField = schemaObj.Fields.First(fld => fld.Name.ToLower() == fieldName.ToLower());
                            if (existingField != null)
                            {
                                alteredTypeFields.Add(existingField.SQL());
                            }
                        }
                    }
                }
                foreach (var prop in this.ModelProperties)
                {
                    if (!tableFields.Contains(prop.Name))
                    {
                        newFields.Add(schemaObj.Fields.First(fld => fld.Name.ToLower() == prop.Name.ToLower()));
                    }
                }
                if (deletedFields.Count > 0)
                {
                    //deleted columns.
                    result.Add(schemaObj.DB.DDLHelper.SafeFormatAlterTableDropColumn(schemaObj.SQLName, deletedFields));
                }

                //columns that the data type has changed.
                foreach (var fld in alteredTypeFields)
                {
                    result.Add(String.Format("ALTER TABLE {0} {1} {2} ", schemaObj.SQLName, schemaObj.DB.DDLHelper.ModifyColumnCommand, fld));
                }
                if (newFields.Count > 0)
                {
                    //new columns that do not exist in the table.
                    var newFieldsSQL = new List<string>();
                    foreach (var fld in newFields)
                    {
                        newFieldsSQL.Add(fld.SQL());
                    }
                    result.Add(schemaObj.DB.DDLHelper.SafeFormatAlterTableAddColumn(schemaObj.SQLName, newFieldsSQL));
                }
            }
            catch (Exception e)
            {
                //schemaObj.logger.LogError(e.Message);
                throw e;
            }
            finally
            {
                if (dataReader != null)
                {
                    dataReader.Close();
                    dataReader.Dispose();
                }
            }
            return result;
        }
        #endregion

        /// <summary>
        /// OrpheusModelHelper is a helper class that analyzes a model and can create primary-foreign keys and/or schema fields, when creating a schema.
        /// </summary>
        /// <param name="modelType"></param>
        public OrpheusModelHelper(Type modelType)
        {
            this.modelType = modelType;
            this.PrimaryKeys = new Dictionary<string, PrimaryKey>();
            this.ForeignKeys = new Dictionary<string, ForeignKey>();
            this.UniqueKeys = new Dictionary<string, UniqueKey>();
            this.PrimaryCompositeKeys = new List<PrimaryCompositeKey>();
            this.UniqueCompositeKeys = new List<UniqueCompositeKey>();
            this.SchemaIgnoreProperties = new List<string>();
            this.CustomFieldNameProperties = new Dictionary<string, string>();
            this.intializeModelProperties();
        }
    }
}
