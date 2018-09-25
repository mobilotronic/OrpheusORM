namespace OrpheusInterfaces.Interfaces.Attributes
{
    /// <summary>
    /// Orpheus composite key attribute
    /// </summary>
    public interface IOrpheusBaseCompositeKeyAttribute : IOrpheusBaseAttribute
    {
        /// <summary>
        /// List of fields that are the key.
        /// </summary>
        /// <returns>Array of field names</returns>
        string[] Fields { get;  }

        /// <summary>
        /// Sort for the key.
        /// </summary>
        /// <returns>Sort direction</returns>
        string Sort { get; }
    }
}
