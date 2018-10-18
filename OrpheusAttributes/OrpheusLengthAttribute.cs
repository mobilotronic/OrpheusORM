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
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Length"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Length(int value)
        {
            this.Value = value;
        }
    }
}
