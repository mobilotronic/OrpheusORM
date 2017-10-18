using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusAttributes
{
    /// <summary>
    /// TableName attribute. Decorate a model with this attribute to
    /// to explicitly define the corresponding db table name.
    /// </summary>
    public class TableName : OrpheusBaseAttribute
    {
        /// <summary>
        /// Table name.
        /// </summary>
        public string Name { get; private set;}

        /// <summary>
        /// TableName constructor.
        /// </summary>
        /// <param name="tableName"></param>
        public TableName(string tableName)
        {
            this.Name = tableName;
        }
    }
}
