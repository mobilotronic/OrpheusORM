namespace OrpheusAttributes
{
    /// <summary>
    /// Primary composite key attribute, to decorate models that have primary or unique keys that are comprised from than one field.
    /// </summary>
    public class PrimaryCompositeKey : OrpheusCompositeKeyBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimaryCompositeKey"/> class.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="sort">The sort direction.</param>
        public PrimaryCompositeKey(string[] fields,string sort = null) : base(fields,sort) { }
    }
}
