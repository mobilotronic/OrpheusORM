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
        /// <returns>The referenced table name</returns>
        public string ReferenceTable { get; private set; }

        /// <summary>
        /// The reference table key.
        /// </summary>
        /// <returns>The referenced table key</returns>
        public string ReferenceField { get; private set; }

        /// <summary>
        /// Set to true to enable cascade delete.
        /// </summary>
        /// <returns>Delete cascade flag</returns>
        public bool OnDeleteCascade { get; private set; }

        /// <summary>
        /// Set to true to enable cascade update.
        /// </summary>
        /// <returns>Update cascade flag</returns>
        public bool OnUpdateCascade { get; private set; }

        /// <summary>
        /// Optional. Set the schema name of the reference table, if there is one.
        /// </summary>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Foreign key attribute constructor.
        /// </summary>
        /// <param name="referenceTable">The referenced table name</param>
        /// <param name="referenceField">The referenced field name</param>
        /// <param name="onDeleteCascade">Delete cascade flag</param>
        /// <param name="onUpdateCascade">Update cascade flag</param>
        public ForeignKey(string referenceTable, string referenceField,string schemaName = null,bool onDeleteCascade = false, bool onUpdateCascade = false)
        {
            this.ReferenceField = referenceField;
            this.ReferenceTable = referenceTable;
            this.OnDeleteCascade = onDeleteCascade;
            this.OnUpdateCascade = onUpdateCascade;
            this.SchemaName = schemaName;
        }

        /// <summary>
        /// Foreign key attribute constructor.
        /// </summary>
        /// <param name="referenceTableType">The referenced table type</param>
        /// <param name="referenceField">The referenced field name</param>
        /// <param name="onDeleteCascade">Delete cascade flag</param>
        /// <param name="onUpdateCascade">Update cascade flag</param>
        public ForeignKey(Type referenceTableType, string referenceField,string schemaName = null, bool onDeleteCascade = false, bool onUpdateCascade = false):this(referenceTableType.Name,referenceField,schemaName,onDeleteCascade,onUpdateCascade)
        {
        }
    }
}
