namespace OrpheusInterfaces.Logging
{
    /// <summary>
    /// File logging configuration contract.
    /// </summary>
    public interface IFileLoggingConfiguration
    {
        /// <summary>Minimum log level (e.g. "Error", "Information").</summary>
        string Level { get; set; }
        /// <summary>Maximum log file size in MB before rotation.</summary>
        int MaxFileSize { get; set; }
        /// <summary>Full path to the log file directory.</summary>
        string FilePath { get; set; }
        /// <summary>Base file name for log files.</summary>
        string FileName { get; set; }
        /// <summary>Folder name for log files.</summary>
        string FolderName { get; set; }
        /// <summary>Whether file logging is enabled.</summary>
        bool Enabled { get; set; }
        /// <summary>File extension for log files (default "log").</summary>
        string FileExtension { get; set; }
    }
}
