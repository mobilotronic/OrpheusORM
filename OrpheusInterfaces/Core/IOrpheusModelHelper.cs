using OrpheusInterfaces.Interfaces.Attributes;
using OrpheusInterfaces.Schema;
using System.Collections.Generic;
using System.Reflection;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// Orpheus model helper
    /// </summary>
    public interface IOrpheusModelHelper
    {
        /// <value>
        /// Model's primary keys.
        /// </value>
        Dictionary<string, IPrimaryKey> PrimaryKeys { get; }

        /// <value>
        /// Model's foreign keys.
        /// </value>
        Dictionary<string, IForeignKey> ForeignKeys { get; }

        /// <value>
        /// Model's unique keys.
        /// </value>
        Dictionary<string, IUniqueKey> UniqueKeys { get; }

        /// <value>
        /// Model's composite primary keys.
        /// </value>
        List<IOrpheusBaseCompositeKeyAttribute> PrimaryCompositeKeys { get; }

        /// <value>
        /// Model's composite unique keys.
        /// </value>
        List<IOrpheusBaseCompositeKeyAttribute> UniqueCompositeKeys { get; }

        /// <value>
        /// Model properties that are not part of the schema.
        /// </value>
        List<string> SchemaIgnoreProperties { get;  }

        /// <value>
        /// Model properties that have an explicitly set field name.
        /// </value>
        Dictionary<string, string> CustomFieldNameProperties { get;  }

        /// <value>
        /// Model's properties.
        /// </value>
        PropertyInfo[] ModelProperties { get; }

        /// <value>
        /// Model's SQLName. Defaults to the model's type name.
        /// </value>
        string SQLName { get;  }

        /// <value>
        /// SQL server's schema name.
        /// </value>
        string SQLServerSchemaName { get;  }

        /// <summary>
        /// Helper function that returns true if the property is not actually part of the schema.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        bool IsSchemaProperty(PropertyInfo property);

        /// <summary>
        /// Helper function that returns the corresponding field name for a property.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        string GetFieldNameForProperty(PropertyInfo prop);

        /// <summary>
        /// Creates an instance of the model and typecasts it to the given type.
        /// </summary>
        /// <typeparam name="T">Type to cast the model</typeparam>
        /// <returns></returns>
        T CreateInstance<T>();

        /// <summary>
        /// Creates schema fields and constraints for a model.
        /// </summary>
        /// <param name="schemaObj"></param>
        void CreateSchemaFields(ISchemaDataObject schemaObj);

        /// <summary>
        /// Creates a list of SQL ALTER commands, based on the differences between the current version of the model
        /// and the current version of the corresponding db table.
        /// </summary>
        /// <param name="schemaObj"></param>
        /// <param name="ddlHelper"></param>
        /// <returns></returns>
        List<string> GetAlterDDLCommands(ISchemaDataObject schemaObj, IOrpheusDDLHelper ddlHelper);
    }
}
