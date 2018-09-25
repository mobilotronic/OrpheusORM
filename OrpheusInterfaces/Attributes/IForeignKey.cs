namespace OrpheusInterfaces.Interfaces.Attributes
{
    /// <summary>
    /// Orpheus foreign key attribute
    /// </summary>
    public interface IForeignKey : IOrpheusBaseAttribute
    {
        /// <summary>
        /// The foreign key field name.
        /// </summary>
        string Field { get; set; }

        /// <summary>
        /// The reference table.
        /// </summary>
        /// <returns>The referenced table name</returns>
        string ReferenceTable { get;  }

        /// <summary>
        /// The reference table key.
        /// </summary>
        /// <returns>The referenced table key</returns>
        string ReferenceField { get;  }

        /// <summary>
        /// Set to true to enable cascade delete.
        /// </summary>
        /// <returns>Delete cascade flag</returns>
        bool OnDeleteCascade { get;  }

        /// <summary>
        /// Set to true to enable cascade update.
        /// </summary>
        /// <returns>Update cascade flag</returns>
        bool OnUpdateCascade { get;  }

        /// <summary>
        /// Optional. Set the schema name of the reference table, if there is one.
        /// </summary>
        string SchemaName { get; }
    }
}
