namespace OrpheusAttributes
{
    /// <summary>
    /// Default value attribute.
    /// Decorate a property with attribute to set it's default value.
    /// </summary>
    /// <seealso cref="OrpheusAttributes.OrpheusBaseAttribute" />
    public class DefaultValue : OrpheusBaseAttribute
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValue"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public DefaultValue(object value)
        {
            this.Value = value;
        }
    }
}
