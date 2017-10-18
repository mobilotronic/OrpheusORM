using OrpheusInterfaces;

namespace OrpheusCore.SchemaBuilder
{
    /// <summary>
    /// Represents a schema join definition, which can be used by <see cref="ISchemaObject"/> when a schema is created.
    /// </summary>
    public class SchemaJoinDefinition : ISchemaJoinDefinition
    {
        /// <summary>
        /// Key field name of the join object.
        /// </summary>
        /// <returns>Key field name of the join object</returns>
        public string JoinKeyField { get; set; }

        /// <summary>
        /// Join operator <see cref="SchemaJoinOperator"/>.
        /// </summary>
        /// <returns>Join operator</returns>
        public SchemaJoinOperator JoinOperator { get; set; }

        /// <summary>
        /// Type of join <see cref="SchemaJoinType"/>.
        /// </summary>
        /// <returns>Type of join</returns>
        public SchemaJoinType JoinType { get; set; }

        /// <summary>
        /// Key field name of the main object.
        /// </summary>
        /// <returns>Key field name of the main object</returns>
        public string KeyField { get; set; }
    }
}
