namespace OrpheusAttributes
{
    /// <summary>
    /// TableName attribute. Decorate a model with this attribute to
    /// to explicitly define the corresponding db table name.
    /// </summary>
    public class TableName : OrpheusBaseAttribute
    {
        /// <value>
        /// Table name.
        /// </value>
        public string Name { get; private set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="TableName"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public TableName(string tableName)
        {
            this.Name = tableName;
        }
    }
}
