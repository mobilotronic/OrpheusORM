namespace OrpheusAttributes
{
    /// <summary>
    /// FieldName attribute. Decorate a model property with this attribute,
    /// to explicitly define the corresponding field name in the db table.
    /// </summary>
    /// <seealso cref="OrpheusAttributes.OrpheusBaseAttribute" />
    public class FieldName : OrpheusBaseAttribute
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldName"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        public FieldName(string fieldName)
        {
            this.Name = fieldName;
        }
    }
}
