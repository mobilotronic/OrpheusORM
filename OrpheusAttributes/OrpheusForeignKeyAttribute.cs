using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusAttributes
{
    /// <summary>
    /// Primary key constraint attribute.
    /// Decorate a property with attribute to create a foreign key constraint on a schema object.
    /// </summary>
    public class ForeignKey : OrpheusBaseAttribute
    {
        /// <summary>
        /// The reference table.
        /// </summary>
        public string ReferenceTable { get; private set; }

        /// <summary>
        /// The reference table key.
        /// </summary>
        public string ReferenceField { get; private set; }

        /// <summary>
        /// On delete cascade flag.
        /// </summary>
        public bool OnDeleteCascade { get; private set; }

        /// <summary>
        /// On update cascade flag.
        /// </summary>
        public bool OnUpdateCascade { get; private set; }

        /// <summary>
        /// Foreign key attribute constructor.
        /// </summary>
        /// <param name="referenceTable"></param>
        /// <param name="referenceField"></param>
        /// <param name="onDeleteCascade"></param>
        /// <param name="onUpdateCascade"></param>
        public ForeignKey(string referenceTable, string referenceField,bool onDeleteCascade = false, bool onUpdateCascade = false)
        {
            this.ReferenceField = referenceField;
            this.ReferenceTable = referenceTable;
            this.OnDeleteCascade = onDeleteCascade;
            this.OnUpdateCascade = onUpdateCascade;
        }

        /// <summary>
        /// Foreign key attribute constructor.
        /// </summary>
        /// <param name="referenceTableType"></param>
        /// <param name="referenceField"></param>
        /// <param name="onDeleteCascade"></param>
        /// <param name="onUpdateCascade"></param>
        public ForeignKey(Type referenceTableType, string referenceField, bool onDeleteCascade = false, bool onUpdateCascade = false):this(referenceTableType.Name,referenceField,onDeleteCascade,onUpdateCascade)
        {
        }
    }
}
