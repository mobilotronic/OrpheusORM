using OrpheusInterfaces.Interfaces.Attributes;

namespace OrpheusAttributes
{
    /// <summary>
    /// Composite key attribute, to decorate models that have primary or unique keys that are comprised from than one field.
    /// </summary>
    /// <seealso cref="OrpheusAttributes.OrpheusBaseAttribute" />
    /// <seealso cref="OrpheusInterfaces.Interfaces.Attributes.IOrpheusBaseCompositeKeyAttribute" />
    public class OrpheusCompositeKeyBaseAttribute : OrpheusBaseAttribute, IOrpheusBaseCompositeKeyAttribute
    {
        /// <value>
        /// List of fields that are the key.
        /// </value>
        public string[] Fields { get; private set; }

        /// <value>
        /// Sort for the key.
        /// </value>
        public string Sort { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrpheusCompositeKeyBaseAttribute"/> class.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="sort">The sort direction.</param>
        public OrpheusCompositeKeyBaseAttribute(string[] fields,string sort = null):base()
        {
            this.Fields = fields;
            this.Sort = sort;
        }
    }
}
