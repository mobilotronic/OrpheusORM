using OrpheusInterfaces;

namespace OrpheusCore.SchemaBuilder
{
    public class SchemaJoinDefinition : ISchemaJoinDefinition
    {
        public string JoinKeyField { get; set; }

        public SchemaJoinOperator JoinOperator { get; set; }

        public SchemaJoinType JoinType { get; set; }

        public string KeyField { get; set; }
    }
}
