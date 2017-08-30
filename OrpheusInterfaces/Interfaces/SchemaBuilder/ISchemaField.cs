using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
{
    /// <summary>
    /// Represents a schema field.
    /// </summary>
    public interface ISchemaField
    {
        /// <summary>
        /// Field name.
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Field alias.
        /// </summary>
        string Alias { get; set; }
        
        /// <summary>
        /// Field size (if applicable).
        /// </summary>
        string Size { get; set; }
        
        /// <summary>
        /// Field data type.
        /// </summary>
        string DataType { get; set; }
        
        /// <summary>
        /// Returns SQL definition for the field.
        /// </summary>
        /// <returns></returns>
        string SQL();
        
        /// <summary>
        /// True if field accepts null values.
        /// </summary>
        bool Nullable { get; set; }
        
        /// <summary>
        /// Field's default value.
        /// </summary>
        string DefaultValue { get; set; }

        /// <summary>
        /// Schema object were this schema field exists
        /// </summary>
        ISchemaObject SchemaObject { get; }
    }
}
