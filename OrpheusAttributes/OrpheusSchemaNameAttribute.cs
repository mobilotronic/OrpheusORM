using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    /// <summary>
    /// Schema name attribute. Decorate a class with this attribute, to define to which schema the model exists.
    /// It's only applicable when the underlying database engine is SQL server, as it's the only one, from the supported database engines, that has this functionality.
    /// </summary>
    public class SQLServerSchemaName:OrpheusBaseAttribute
    {
        /// <summary>
        /// The schema name.
        /// </summary>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Schema name attribute constructor.
        /// </summary>
        /// <param name="schemaName">Schema name.</param>
        public SQLServerSchemaName(string schemaName) { this.SchemaName = schemaName; }
    }
}
