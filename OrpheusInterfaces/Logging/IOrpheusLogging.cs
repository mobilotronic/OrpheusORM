using System;

namespace OrpheusInterfaces.Logging
{
    /// <summary>
    /// Orpheus logger interface.
    /// </summary>
    public interface IOrpheusLogger
    {
        /// <summary>
        /// Logging configuration.
        /// </summary>
        ILoggingConfiguration LoggingConfiguration { get; set; }
    }

    /// <summary>
    /// Log entry model.
    /// </summary>
    public interface ILogEntry
    {

        /// <summary>
        /// Log entry type. Error, Information, Debug etc.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Log entry message.
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// Log entry time-stamp.
        /// </summary>
        DateTime TimeStamp { get; set; }

        /// <summary>
        /// Stack trace
        /// </summary>
        string StackTrace { get; set; }
    }

    /// <summary>
    /// Logging configuration model.
    /// </summary>
    public interface ILoggingConfiguration
    {
        /// <summary>
        /// Logging level.
        /// </summary>
        string Level { get; set; }

        /// <summary>
        /// Log file path.
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Maximum log file size.
        /// </summary>
        int MaxFileSize { get; set; }
    }
}
