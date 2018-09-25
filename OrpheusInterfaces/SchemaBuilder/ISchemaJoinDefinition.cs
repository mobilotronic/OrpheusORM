namespace OrpheusInterfaces.Schema
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
        /// <returns>Key field name of the main object</returns>
        string KeyField { get; set; }

        /// <summary>
        /// Key field name of the join object.
        /// </summary>
        /// <returns>Key field name of the join object</returns>
        string JoinKeyField { get; set; }

        /// <summary>
        /// Type of join <see cref="SchemaJoinType"/>.
        /// </summary>
        /// <returns>Type of join</returns>
        SchemaJoinType JoinType { get; set; }

        /// <summary>
        /// Join operator <see cref="SchemaJoinOperator"/>.
        /// </summary>
        /// <returns>Join operator</returns>
        SchemaJoinOperator JoinOperator { get; set; }

        /// <summary>
        /// Table name to perform the join.
        /// </summary>
        string JoinTableName { get; set; }
    }
}
