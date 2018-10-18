namespace OrpheusInterfaces.Interfaces.Attributes
{
    /// <summary>
    /// Orpheus foreign key attribute
    /// </summary>
    public interface IForeignKey : IOrpheusBaseAttribute
    {
        /// <value>
        /// The foreign key field name.
        /// </value>
        string Field { get; set; }

        /// <value>
        /// The reference table.
        /// </value>
        /// <returns>The referenced table name</returns>
        string ReferenceTable { get;  }

        /// <value>
        /// The reference table key.
        /// </value>
        /// <returns>The referenced table key</returns>
        string ReferenceField { get;  }

        /// <value>
        /// Set to true to enable cascade delete.
        /// </value>
        /// <returns>Delete cascade flag</returns>
        bool OnDeleteCascade { get;  }

        /// <value>
        /// Set to true to enable cascade update.
        /// </value>
        /// <returns>Update cascade flag</returns>
        bool OnUpdateCascade { get;  }

        /// <value>
        /// Optional. Set the schema name of the reference table, if there is one.
        /// </value>
        string SchemaName { get; }
    }
}
