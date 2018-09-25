namespace OrpheusInterfaces.Schema
{
    /// <summary>
    /// Represents a schema field.
    /// </summary>
    public interface ISchemaField
    {
        /// <summary>
        /// Field name.
        /// </summary>
        /// <returns>Field name</returns>
        string Name { get; set; }

        /// <summary>
        /// Field alias.
        /// </summary>
        /// <returns>Field alias</returns>
        string Alias { get; set; }

        /// <summary>
        /// Field size (if applicable).
        /// </summary>
        /// <returns>Field size</returns>
        string Size { get; set; }

        /// <summary>
        /// Field data type.
        /// </summary>
        /// <returns>Field data type</returns>
        string DataType { get; set; }
        
        /// <summary>
        /// Returns SQL definition for the field.
        /// </summary>
        /// <returns>SQL definition for the field</returns>
        string SQL();

        /// <summary>
        /// True if field accepts null values.
        /// </summary>
        /// <returns>True if field accepts null values</returns>
        bool Nullable { get; set; }

        /// <summary>
        /// Field's default value.
        /// </summary>
        /// <returns>Field's default value</returns>
        string DefaultValue { get; set; }

        /// <summary>
        /// Schema object where this schema field exists
        /// </summary>
        /// <returns>Schema object where the field exists</returns>
        ISchemaObject SchemaObject { get; }

        /// <summary>
        /// Table where the field belongs.
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// Gets the full field name.
        /// </summary>
        string FullFieldName { get; }
    }
}
