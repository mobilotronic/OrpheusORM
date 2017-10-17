using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusAttributes
{
    public class TableName : OrpheusBaseAttribute
    {
        public string Name { get; private set;}

        public TableName(string tableName)
        {
            this.Name = tableName;
        }
    }
}
