using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
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
        /// Key name.
        /// </summary>
        string Name { get; set; }
        
        ///<summary>
        /// Fields which the constraint will be applied.
        ///</summary>
        List<string> Fields { get; set; }
        
        /// <summary>
        /// Key's sort direction.
        /// </summary>
        SchemaSort Sort { get; set; }
        
        /// <summary>
        /// Returns the SQL definition of the key.
        /// </summary>
        string SQL();
        
        /// <summary>
        /// Returns true if the constraint needs to drop.
        /// </summary>
        /// <returns></returns>
        DDLAction Action { get; set; }

        /// <summary>
        /// Schema object were this schema constraint exists
        /// </summary>
        ISchemaObject SchemaObject { get; }
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
        string ForeignKeySchemaObject { get; set; }
        
        /// <summary>
        /// Foreign key fields. Applicable only when key is of type ktForeign.
        /// </summary>
        List<string> ForeignKeyFields { get; set; }
        
        /// <summary>
        /// Cascade on delete.
        /// </summary>
        bool OnDeleteCascade { get; set; }
        
        /// <summary>
        /// Cascade on update.
        /// </summary>
        bool OnUpdateCascade { get; set; }
    }
    
    /// <summary>
    /// A unique key constraint.
    /// </summary>
    public interface IUniqueKeySchemaConstraint: ISchemaConstraint { }
}
