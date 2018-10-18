using System;

namespace OrpheusAttributes
{
    /// <summary>
    /// Unique composite key attribute, to decorate models that have primary or unique keys that are comprised from than one field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UniqueCompositeKey : OrpheusCompositeKeyBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueCompositeKey"/> class.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="sort">The sort direction.</param>
        public UniqueCompositeKey(string[] fields,string sort = null) : base(fields) { }
    }
}
