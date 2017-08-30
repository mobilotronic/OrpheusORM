using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
{
    /// <summary>
    /// Schema join types.
    /// </summary>
    public enum SchemaJoinType
    {
        /// <summary>
        /// Left out join
        /// </summary>
        jtLeftOuter,

        /// <summary>
        /// Left inner join.
        /// </summary>
        jtLeftInner,

        /// <summary>
        /// Inner join.
        /// </summary>
        jtInner,

        /// <summary>
        /// Right outer join.
        /// </summary>
        jtRightOuter,

        /// <summary>
        /// Right inner join.
        /// </summary>
        jtRightInner
    }

    /// <summary>
    /// Join operator type.
    /// </summary>
    public enum SchemaJoinOperator
    {
        /// <summary>
        /// Equals.
        /// </summary>
        joEquals,

        /// <summary>
        /// Not equals.
        /// </summary>
        joNotEquals
    }

    /// <summary>
    /// Represents a schema join definition, which can be used by <see cref="ISchemaObject"/> when a schema is created.
    /// </summary>
    public interface ISchemaJoinDefinition
    {
        /// <summary>
        /// Key field name of the main object.
        /// </summary>
        string KeyField { get; set; }
        
        /// <summary>
        /// Key field name of the join object.
        /// </summary>
        string JoinKeyField { get; set; }
        
        /// <summary>
        /// Type of join <see cref="SchemaJoinType"/>.
        /// </summary>
        SchemaJoinType JoinType { get; set; }
        
        /// <summary>
        /// Join operator <see cref="SchemaJoinOperator"/>.
        /// </summary>
        SchemaJoinOperator JoinOperator { get; set; }
    }
}
