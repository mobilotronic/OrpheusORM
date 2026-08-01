using System;

namespace OrpheusInterfaces.Logging
{
    /// <summary>
    /// Represents a single log entry.
    /// </summary>
    public interface ILogEntry
    {
        /// <summary>Log entry severity type.</summary>
        string Type { get; set; }
        /// <summary>Log entry message text.</summary>
        string Message { get; set; }
        /// <summary>Timestamp when the entry was created.</summary>
        DateTime TimeStamp { get; set; }
        /// <summary>Stack trace, if applicable.</summary>
        string StackTrace { get; set; }
    }
}
