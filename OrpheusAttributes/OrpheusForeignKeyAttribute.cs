using OrpheusInterfaces.Interfaces.Attributes;
using System;

namespace OrpheusAttributes
{
    /// <summary>
    /// Primary key constraint attribute.
    /// Decorate a property with attribute to create a foreign key constraint on a schema object.
    /// </summary>
    /// <seealso cref="OrpheusAttributes.OrpheusBaseAttribute" />
    /// <seealso cref="OrpheusInterfaces.Interfaces.Attributes.IForeignKey" />
    public class ForeignKey : OrpheusBaseAttribute, IForeignKey
    {
        /// <value>
        /// The foreign key field name.
        /// </value>
        public string Field { get; set; }

        /// <value>
        /// The reference table.
        /// </value>
        public string ReferenceTable { get; private set; }

        /// <value>
        /// The reference table key.
        /// </value>
        public string ReferenceField { get; private set; }

        /// <value>
        /// Set to true to enable cascade delete.
        /// </value>
        public bool OnDeleteCascade { get; private set; }

        /// <value>
        /// Set to true to enable cascade update.
        /// </value>
        public bool OnUpdateCascade { get; private set; }

        /// <value>
        /// Optional. Set the schema name of the reference table, if there is one.
        /// </value>
        public string SchemaName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKey"/> class.
        /// </summary>
        /// <param name="referenceTable">The reference table.</param>
        /// <param name="referenceField">The reference field.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="onDeleteCascade">if set to <c>true</c> [on delete cascade].</param>
        /// <param name="onUpdateCascade">if set to <c>true</c> [on update cascade].</param>
        public ForeignKey(string referenceTable, string referenceField,string schemaName = null,bool onDeleteCascade = false, bool onUpdateCascade = false)
        {
            this.ReferenceField = referenceField;
            this.ReferenceTable = referenceTable;
            this.OnDeleteCascade = onDeleteCascade;
            this.OnUpdateCascade = onUpdateCascade;
            this.SchemaName = schemaName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForeignKey"/> class.
        /// </summary>
        /// <param name="referenceTableType">Type of the reference table.</param>
        /// <param name="referenceField">The reference field.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="onDeleteCascade">if set to <c>true</c> [on delete cascade].</param>
        /// <param name="onUpdateCascade">if set to <c>true</c> [on update cascade].</param>
        public ForeignKey(Type referenceTableType, string referenceField,string schemaName = null, bool onDeleteCascade = false, bool onUpdateCascade = false):this(referenceTableType.Name,referenceField,schemaName,onDeleteCascade,onUpdateCascade)
        {
        }
    }
}
