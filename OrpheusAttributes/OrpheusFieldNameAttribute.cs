using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusAttributes
{
    public class FieldName : OrpheusBaseAttribute
    {
        public string Name { get; private set; }

        public FieldName(string fieldName)
        {
            this.Name = fieldName;
        }
    }
}
