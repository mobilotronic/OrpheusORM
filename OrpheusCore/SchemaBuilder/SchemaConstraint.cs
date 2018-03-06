using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrpheusCore.SchemaBuilder
{
    /// <summary>
    /// A primary key constraint.
    /// </summary>
    public class PrimaryKeySchemaConstraint : IPrimaryKeySchemaConstraint
    {
        private ISchemaDataObject schemaObject;


        ///<summary>
        /// Fields which the constraint will be applied.
        ///</summary>
        ///<returns>Fields affected from the constraint</returns>
        public List<string> Fields { get; set; }

        /// <summary>
        /// Constraint name.
        /// </summary>
        /// <returns>Constraint name</returns>
        public string Name { get; set; }

        /// <summary>
        /// Key's sort direction.
        /// </summary>
        /// <returns>Schema sort type</returns>
        public SchemaSort Sort { get; set; }

        /// <summary>
        /// Schema object were this schema constraint exists
        /// </summary>
        /// <returns>The schema object where the constraint exists</returns>
        public ISchemaDataObject SchemaObject { get { return this.schemaObject; } }

        /// <summary>
        /// Returns true if the constraint needs to drop.
        /// </summary>
        /// <returns>Constraint's DDLAction</returns>
        public DDLAction Action { get; set; }

        /// <summary>
        /// Returns the SQL definition of the key.
        /// </summary>
        /// <returns>Constraint's SQL</returns>
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

        /// <summary>
        /// The constraint SQL command. UNIQUE, PRIMARY KEY etc.
        /// </summary>
        public string ConstraintSQLCommand { get; protected set; }

        /// <summary>
        /// Creates a primary key constraint.
        /// </summary>
        /// <param name="schemaObject">Schema object where the constraint belong</param>
        public PrimaryKeySchemaConstraint(ISchemaDataObject schemaObject)
        {
            this.Fields = new List<string>();
            this.Sort = SchemaSort.ssAsc;
            this.Action = DDLAction.ddlCreate;
            this.schemaObject = schemaObject;
            this.ConstraintSQLCommand = "PRIMARY KEY";
        }

    }

    /// <summary>
    /// A foreign key constraint.
    /// </summary>
    public class ForeignKeySchemaConstraint : PrimaryKeySchemaConstraint, IForeignKeySchemaConstraint
    {

        /// <summary>
        /// Foreign key fields. Applicable only when key is of type ktForeign.
        /// </summary>
        /// <returns>List of key fields</returns>
        public List<string> ForeignKeyFields { get; set; }

        /// <summary>
        /// Referenced table name. Applicable only when key is of type ktForeign.
        /// </summary>
        /// <returns>Constraint's key</returns>
        public string ForeignKeySchemaObject { get; set; }

        /// <summary>
        /// Cascade on delete.
        /// </summary>
        /// <returns>True if cascade on delete is on</returns>
        public bool OnDeleteCascade { get; set; }

        /// <summary>
        /// Cascade on update.
        /// </summary>
        /// <returns>True if cascade on update is on</returns>
        public bool OnUpdateCascade { get; set; }

        /// <summary>
        /// Returns the SQL definition of the key.
        /// </summary>
        /// <returns>Constraint's SQL</returns>
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

        /// <summary>
        /// Creates a foreign key constraint.
        /// </summary>
        /// <param name="schemaObject">Schema object where the constraint belong</param>
        public ForeignKeySchemaConstraint(ISchemaDataObject schemaObject) :base(schemaObject)
        {
            this.ForeignKeyFields = new List<string>();
            this.ConstraintSQLCommand = "FOREIGN KEY";
        }
    }

    /// <summary>
    /// A unique key constraint.
    /// </summary>
    public class UniqueKeySchemaConstraint : PrimaryKeySchemaConstraint,IUniqueKeySchemaConstraint
    {
        /// <summary>
        /// Returns the SQL definition of the key.
        /// </summary>
        /// <returns>Constraint's SQL</returns>
        public override string SQL()
        {
            base.SQL();
            var result = "";
            switch (this.Action)
            {
                case DDLAction.ddlCreate:
                    {
                        //ADD CONSTRAINT {0}{1}{2} PRIMARY KEY ({3} {4})
                        var sBuilder = new StringBuilder();
                        sBuilder.Append(" ADD CONSTRAINT ");
                        sBuilder.Append(String.Format("{0}{1}{2}",this.SchemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                                        this.Name,
                                        this.SchemaObject.DB.DDLHelper.DelimitedIndetifierEnd));

                        sBuilder.Append(this.ConstraintSQLCommand);
                        sBuilder.Append(String.Format(" ({0}) ",string.Join(",", this.Fields.Select(fld => string.Format("{0}{1}{2}",
                                                    this.SchemaObject.DB.DDLHelper.DelimitedIndetifierStart,
                                                    fld,
                                                    this.SchemaObject.DB.DDLHelper.DelimitedIndetifierEnd)))));
                        result = sBuilder.ToString();
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

        /// <summary>
        /// Creates a unique key constraint.
        /// </summary>
        /// <param name="schemaObject">Schema object where the constraint belong</param>
        public UniqueKeySchemaConstraint(ISchemaDataObject schemaObject) : base(schemaObject)
        {
            this.ConstraintSQLCommand = "UNIQUE";
        }
    }
}
