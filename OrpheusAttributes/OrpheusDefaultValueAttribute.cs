using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusAttributes
{
    /// <summary>
    /// Default value attribute.
    /// Decorate a property with attribute to set it's default value.
    /// </summary>
    public class DefaultValue : OrpheusBaseAttribute
    {
        /// <summary>
        /// The default value.
        /// </summary>
        /// <returns>Default value</returns>
        public object Value { get; private set; }

        /// <summary>
        /// DefaultValue attribute constructor.
        /// </summary>
        /// <param name="value">Property's default value</param>
        public DefaultValue(object value)
        {
            this.Value = value;
        }
    }
}
