using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusAttributes
{
    /// <summary>
    /// Length attribute.
    /// Decorate a property with attribute to set a maximum length value.
    /// Applies only to string types.
    /// </summary>
    public class Length : OrpheusBaseAttribute
    {
        /// <summary>
        /// The maximum length value.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Length attribute constructor.
        /// </summary>
        /// <param name="value"></param>
        public Length(int value)
        {
            this.Value = value;
        }
    }
}
