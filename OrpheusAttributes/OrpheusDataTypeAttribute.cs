using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusAttributes
{

    /// <summary>
    /// Annotate a property with attribute to set it's DbType
    /// </summary>
    public class DataTypeAttribute : OrpheusBaseAttribute
    {

        /// <summary>
        /// Field's data type.
        /// </summary>
        public DbType DataType { get; private set; }

        /// <summary>
        /// Data type attribute constructor.
        /// </summary>
        /// <param name="dbType"></param>
        public DataTypeAttribute(DbType dbType)
        {
            this.DataType = dbType;
        }

        /// <summary>
        /// Data type attribute constructor.
        /// </summary>
        /// <param name="dbType"></param>
        public DataTypeAttribute(int dbType):this((DbType)dbType)
        {

        }

    }
}
