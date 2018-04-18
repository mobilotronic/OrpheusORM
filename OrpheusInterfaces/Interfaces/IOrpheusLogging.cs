using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusInterfaces.Interfaces
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
        /// Log entry source file information.
        /// </summary>
        string SourceFile { get; set; }

        /// <summary>
        /// Log entry line number.
        /// </summary>
        string LineNumber { get; set; }
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
