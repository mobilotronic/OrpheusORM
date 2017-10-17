using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusAttributes
{
    /// <summary>
    /// Composite key attribute, to decorate models that have primary or unique keys that are comprised from than one field.
    /// </summary>
    public class OrpheusCompositeKeyBaseAttribute : OrpheusBaseAttribute
    {
        /// <summary>
        /// List of fields that are the key.
        /// </summary>
        /// <returns>Array of field names</returns>
        public string[] Fields { get; private set; }

        /// <summary>
        /// Sort for the key.
        /// </summary>
        /// <returns>Sort direction</returns>
        public string Sort { get; private set; }

        /// <summary>
        /// Create a OrpheusCompositeKeyBaseAttribute.
        /// </summary>
        /// <param name="fields">Fields that are the key </param>
        /// <param name="sort">Sort direction for the key </param>
        public OrpheusCompositeKeyBaseAttribute(string[] fields,string sort = null):base()
        {
            this.Fields = fields;
            this.Sort = sort;
        }
    }
}
