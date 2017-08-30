using OrpheusInterfaces;
using System;

namespace OrpheusCore.SchemaBuilder
{
    public class SchemaField : ISchemaField
    {
        private ISchemaObject schemaObject;

        public string Alias { get; set; }

        public string DataType { get; set; }

        public string Name { get; set; }

        public string Size { get; set; }

        public string DefaultValue { get; set; }

        public bool Nullable { get; set; }

        public ISchemaObject SchemaObject { get { return this.schemaObject; } }

        public string SQL()
        {
            var result = "";
            if (this.Name == null || this.DataType == null)
                throw new Exception("Field has no name or data type defined.");
            var fieldName = this.Alias != null ? this.Name + " AS " + this.Alias : this.Name;
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
        public SchemaField(ISchemaObject schemaObject)
        {
            this.Nullable = true;
            this.schemaObject = schemaObject;
        }
    }
}
