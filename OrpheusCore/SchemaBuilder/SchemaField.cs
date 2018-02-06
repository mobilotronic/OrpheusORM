using OrpheusInterfaces;
using System;

namespace OrpheusCore.SchemaBuilder
{
    /// <summary>
    /// Represents a schema field.
    /// </summary>
    public class SchemaField : ISchemaField
    {
        private ISchemaDataObject schemaObject;

        /// <summary>
        /// Field alias.
        /// </summary>
        /// <returns>Field alias</returns>
        public string Alias { get; set; }

        /// <summary>
        /// Field data type.
        /// </summary>
        /// <returns>Field data type</returns>
        public string DataType { get; set; }

        /// <summary>
        /// Field name.
        /// </summary>
        /// <returns>Field name</returns>
        public string Name { get; set; }

        /// <summary>
        /// Field size (if applicable).
        /// </summary>
        /// <returns>Field size</returns>
        public string Size { get; set; }

        /// <summary>
        /// Field's default value.
        /// </summary>
        /// <returns>Field's default value</returns>
        public string DefaultValue { get; set; }

        /// <summary>
        /// True if field accepts null values.
        /// </summary>
        /// <returns>True if field accepts null values</returns>
        public bool Nullable { get; set; }

        /// <summary>
        /// Schema object where this schema field exists
        /// </summary>
        /// <returns>Schema object where the field exists</returns>
        public ISchemaObject SchemaObject { get { return this.schemaObject; } }

        /// <summary>
        /// Table where the field belongs.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets the full field name.
        /// </summary>
        public string FullFieldName {
            get
            {
                return this.TableName != null ? String.Format("{0}.{1}", this.TableName, this.Name) : this.Name;
            }
        }

        /// <summary>
        /// Returns SQL definition for the field.
        /// </summary>
        /// <returns>SQL definition for the field</returns>
        public string SQL()
        {
            var result = "";
            if (this.Name == null || this.DataType == null)
                throw new Exception("Field has no name or data type defined.");
            var fieldName = this.Alias != null ? this.FullFieldName + " AS " + this.Alias : this.FullFieldName;
            if (this.Size != null)
                result = String.Format("{0}{1}{2} {3} ({4}) {5}", 
                    this.schemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                    fieldName,
                    this.schemaObject.DB.DDLHelper.DelimitedIndetifierEnd,
                    this.DataType,
                    this.Size,
                    this.Nullable ? "" :"NOT NULL");
            else
                result = String.Format("{0}{1}{2} {3} {4}",
                    this.schemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                    fieldName,
                    this.schemaObject.DB.DDLHelper.DelimitedIndetifierEnd,
                    this.DataType, 
                    this.Nullable ? "" : "NOT NULL");
            if(this.DefaultValue != null)
            {
                result = String.Format(result + " " + " DEFAULT {0}",this.DefaultValue);
            }
            return result;
        }
        
        /// <summary>
        /// Creates a schema field.
        /// </summary>
        /// <param name="schemaObject">Schema where the field belongs</param>
        public SchemaField(ISchemaDataObject schemaObject)
        {
            this.Nullable = true;
            this.schemaObject = schemaObject;
        }
    }
}
