namespace OrpheusInterfaces.Logging
{
    /// <summary>
    /// Orpheus logger contract.
    /// </summary>
    public interface IOrpheusLogger
    {
        /// <summary>The active file logging configuration.</summary>
        IFileLoggingConfiguration Configuration { get; set; }
        /// <summary>Full path to the currently active log file.</summary>
        string CurrentFileName { get; }
    }
}
