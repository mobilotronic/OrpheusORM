using System.Collections.Generic;

namespace OrpheusInterfaces.Schema
{

    /// <summary>
    /// Schema sort type.
    /// </summary>
    public enum SchemaSort
    {
        /// <summary>
        /// Sort ascending.
        /// </summary>
        ssAsc,
        
        /// <summary>
        /// Sort descending.
        /// </summary>
        ssDesc
    }
    
    /// <summary>
    /// Represents a schema constraint.
    /// </summary>
    public interface ISchemaConstraint
    {
        /// <summary>
        /// Constraint name.
        /// </summary>
        /// <returns>Constraint name</returns>
        string Name { get; set; }

        ///<summary>
        /// Fields which the constraint will be applied.
        ///</summary>
        ///<returns>Fields affected from the constraint</returns>
        List<string> Fields { get; set; }

        /// <summary>
        /// Key's sort direction.
        /// </summary>
        /// <returns>Schema sort type</returns>
        SchemaSort Sort { get; set; }

        /// <summary>
        /// Returns the SQL definition of the key.
        /// </summary>
        /// <returns>Constraint's SQL</returns>
        string SQL();
        
        /// <summary>
        /// Returns true if the constraint needs to drop.
        /// </summary>
        /// <returns>Constraint's DDLAction</returns>
        DDLAction Action { get; set; }

        /// <summary>
        /// Schema object were this schema constraint exists
        /// </summary>
        /// <returns>The schema object where the constraint exists</returns>
        ISchemaDataObject SchemaObject { get; }

        /// <summary>
        /// The constraint SQL command. UNIQUE, PRIMARY KEY etc.
        /// </summary>
        string ConstraintSQLCommand { get; }
    }
    
    /// <summary>
    /// A primary key constraint.
    /// </summary>
    public interface IPrimaryKeySchemaConstraint : ISchemaConstraint { }
    
    /// <summary>
    /// A foreign key constraint.
    /// </summary>
    public interface IForeignKeySchemaConstraint : ISchemaConstraint
    {
        /// <summary>
        /// Referenced table name. Applicable only when key is of type ktForeign.
        /// </summary>
        /// <returns>Constraint's key</returns>
        string ForeignKeySchemaObject { get; set; }

        /// <summary>
        /// Foreign key fields. Applicable only when key is of type ktForeign.
        /// </summary>
        /// <returns>List of key fields</returns>
        List<string> ForeignKeyFields { get; set; }

        /// <summary>
        /// Cascade on delete.
        /// </summary>
        /// <returns>True if cascade on delete is on</returns>
        bool OnDeleteCascade { get; set; }

        /// <summary>
        /// Cascade on update.
        /// </summary>
        /// <returns>True if cascade on update is on</returns>
        bool OnUpdateCascade { get; set; }
    }
    
    /// <summary>
    /// A unique key constraint.
    /// </summary>
    public interface IUniqueKeySchemaConstraint: ISchemaConstraint { }
}
