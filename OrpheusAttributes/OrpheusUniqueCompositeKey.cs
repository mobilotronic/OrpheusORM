using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusAttributes
{
    /// <summary>
    /// Unique composite key attribute, to decorate models that have primary or unique keys that are comprised from than one field.
    /// </summary>
    public class UniqueCompositeKey : OrpheusCompositeKeyBaseAttribute
    {
        /// <summary>
        /// Primary composite key.
        /// </summary>
        /// <param name="fields">Fields that are part of the key</param>
        /// <param name="sort">Sort direction</param>
        public UniqueCompositeKey(string[] fields,string sort = null) : base(fields) { }
    }
}
