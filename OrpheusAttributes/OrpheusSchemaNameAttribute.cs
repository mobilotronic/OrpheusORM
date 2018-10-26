using System;

namespace OrpheusAttributes
{

    /// <summary>
    /// Schema name attribute. Decorate a class with this attribute, to define to which schema the model exists.
    /// It's only applicable when the underlying database engine is SQL server, as it's the only one, from the supported database engines, that has this functionality.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SQLServerSchemaName:OrpheusBaseAttribute
    {
        /// <value>
        /// The schema name.
        /// </value>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLServerSchemaName"/> class.
        /// </summary>
        /// <param name="schemaName">Name of the schema.</param>
        public SQLServerSchemaName(string schemaName) { this.SchemaName = schemaName; }
    }
}
