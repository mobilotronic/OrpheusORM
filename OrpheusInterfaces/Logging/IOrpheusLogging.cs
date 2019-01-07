using Microsoft.Extensions.Logging;
using System;

namespace OrpheusInterfaces.Logging
{
    /// <summary>
    /// Orpheus logger interface.
    /// </summary>
    public interface IOrpheusLogger : ILogger
    {
        /// <summary>
        /// Logging configuration.
        /// </summary>
        IFileLoggingConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the name of the current file.
        /// </summary>
        /// <value>
        /// The name of the current file.
        /// </value>
        string CurrentFileName { get; }
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
    public interface IFileLoggingConfiguration
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
        /// Gets or sets the name of the file. If not defined, a file name will be automatically assigned.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file extension. Default extension is .log
        /// </summary>
        /// <value>
        /// The file extension.
        /// </value>
        string FileExtension { get; set; }

        /// <summary>
        /// Maximum log file size.
        /// </summary>
        int MaxFileSize { get; set; }
    }
}
