using System.Data;

namespace OrpheusAttributes
{

    /// <summary>
    /// Annotate a property with attribute to set it's DbType
    /// </summary>
    /// <seealso cref="OrpheusAttributes.OrpheusBaseAttribute" />
    public class DataTypeAttribute : OrpheusBaseAttribute
    {

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public DbType DataType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeAttribute"/> class.
        /// </summary>
        /// <param name="dbType">Type of the database.</param>
        public DataTypeAttribute(DbType dbType)
        {
            this.DataType = dbType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeAttribute"/> class.
        /// </summary>
        /// <param name="dbType">Type of the database.</param>
        public DataTypeAttribute(int dbType):this((DbType)dbType)
        {

        }

    }
}
