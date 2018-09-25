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
        /// <summary>
        /// Model's primary keys.
        /// </summary>
        Dictionary<string, IPrimaryKey> PrimaryKeys { get; }

        /// <summary>
        /// Model's foreign keys.
        /// </summary>
        Dictionary<string, IForeignKey> ForeignKeys { get; }

        /// <summary>
        /// Model's unique keys.
        /// </summary>
        Dictionary<string, IUniqueKey> UniqueKeys { get; }

        /// <summary>
        /// Model's composite primary keys.
        /// </summary>
        List<IOrpheusBaseCompositeKeyAttribute> PrimaryCompositeKeys { get; }

        /// <summary>
        /// Model's composite unique keys.
        /// </summary>
        List<IOrpheusBaseCompositeKeyAttribute> UniqueCompositeKeys { get; }

        /// <summary>
        /// Model properties that are not part of the schema.
        /// </summary>
        List<string> SchemaIgnoreProperties { get;  }

        /// <summary>
        /// Model properties that have an explicitly set field name.
        /// </summary>
        Dictionary<string, string> CustomFieldNameProperties { get;  }

        /// <summary>
        /// Model's properties.
        /// </summary>
        PropertyInfo[] ModelProperties { get; }

        /// <summary>
        /// Model's SQLName. Defaults to the model's type name.
        /// </summary>
        string SQLName { get;  }

        /// <summary>
        /// SQL server's schema name.
        /// </summary>
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
