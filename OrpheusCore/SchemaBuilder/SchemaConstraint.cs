using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrpheusCore.SchemaBuilder
{
    public class PrimaryKeySchemaConstraint : IPrimaryKeySchemaConstraint
    {
        private ISchemaObject schemaObject;

        public List<string> Fields { get; set; }
        public string Name { get; set; }
        public SchemaSort Sort { get; set; }
        public ISchemaObject SchemaObject { get { return this.schemaObject; } }
        public DDLAction Action { get; set; }
        public virtual string SQL()
        {
            if (this.Name == null)
                throw new Exception("Schema does not have a name or no type defined.");
            var result = "";
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        result = String.Format(" ADD CONSTRAINT {0}{1}{2} PRIMARY KEY ({3} {4})", 
                            this.schemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                            this.Name,
                            this.schemaObject.DB.DDLHelper.DelimitedIndetifierEnd,
                            string.Join(",", this.Fields.Select(fld => string.Format("{0}{1}{2}",
                            this.schemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                            fld,
                            this.schemaObject.DB.DDLHelper.DelimitedIndetifierEnd)).ToArray()), 
                            this.Sort == SchemaSort.ssAsc ? "ASC" : "DESC");
                        break;
                    }
                case DDLAction.ddlDrop:
                    {
                        result = String.Format(" DROP CONSTRAINT {0}", this.Name);
                        break;
                    }
                default:
                    {
                        throw new Exception("If you want to alter a constraint, you need to drop it and re-create it.");
                    }
            }
            return result;
        }
        public PrimaryKeySchemaConstraint(ISchemaObject schemaObject)
        {
            this.Fields = new List<string>();
            this.Sort = SchemaSort.ssAsc;
            this.Action = DDLAction.ddlCreate;
            this.schemaObject = schemaObject;
        }

    }

    public class ForeignKeySchemaConstraint : PrimaryKeySchemaConstraint, IForeignKeySchemaConstraint
    {
        public List<string> ForeignKeyFields { get; set; }

        public string ForeignKeySchemaObject { get; set; }

        public bool OnDeleteCascade { get; set; }

        public bool OnUpdateCascade { get; set; }

        public override string SQL()
        {
            //raises an exception if name is not set.
            base.SQL();
            var result = "";
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        result = string.Format(" ADD CONSTRAINT {0}{1}{2} FOREIGN KEY ({3}{4}{5}) REFERENCES {6} ({7}{8}{9})",
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                            this.Name,
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierEnd,
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                            string.Join(",", this.Fields.ToArray()),
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierEnd,
                            this.ForeignKeySchemaObject,
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                            string.Join(",", this.ForeignKeyFields.ToArray()),
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierEnd
                            );
                        if (this.OnDeleteCascade)
                            result = result + " ON DELETE CASCADE";
                        if (this.OnUpdateCascade)
                            result = result + " ON UPDATE CASCADE";
                        break;
                    }
                case DDLAction.ddlDrop:
                    {
                        result = base.SQL();
                        break;
                    }
                default:
                    {
                        throw new Exception("If you want to alter a constraint, you need to drop it and re-create it.");
                    }
            }

            return result;
        }

        public ForeignKeySchemaConstraint(ISchemaObject schemaObject) :base(schemaObject)
        {
            this.ForeignKeyFields = new List<string>();
        }
    }

    public class UniqueKeySchemaConstraint : PrimaryKeySchemaConstraint,IUniqueKeySchemaConstraint
    {
        public override string SQL()
        {
            base.SQL();
            var result = "";
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        result = String.Format(" ADD CONSTRAINT {0}{1}{2} UNIQUE ({3}{4}{5})",
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                            this.Name,
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierEnd,
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                            string.Join(",", this.Fields.ToArray()),
                            this.SchemaObject.DB.DDLHelper.DelimitedIndetifierEnd);
                        break;
                    }
                case DDLAction.ddlDrop:
                    {
                        result = base.SQL();
                        break;
                    }
                default:
                    {
                        throw new Exception("If you want to alter a constraint, you need to drop it and re-create it.");
                    }
            }

            return result;
        }
        public UniqueKeySchemaConstraint(ISchemaObject schemaObject) : base(schemaObject)
        {
        }
    }
}
