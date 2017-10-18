using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusAttributes
{
    /// <summary>
    /// FieldName attribute. Decorate a model property with this attribute,
    /// to explicitly define the corresponding field name in the db table.
    /// </summary>
    public class FieldName : OrpheusBaseAttribute
    {
        /// <summary>
        /// Field name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Field name constructor.
        /// </summary>
        /// <param name="fieldName">Field name</param>
        public FieldName(string fieldName)
        {
            this.Name = fieldName;
        }
    }
}
