using OrpheusInterfaces.Logging;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Logging configuration model.
    /// </summary>
    public class LoggingConfiguration : IFileLoggingConfiguration
    {
        /// <inheritdoc/>
        public string Level { get; set; } = "Information";
        /// <inheritdoc/>
        public int MaxFileSize { get; set; } = 10;
        /// <inheritdoc/>
        public string FilePath { get; set; }
        /// <inheritdoc/>
        public bool Enabled { get; set; } = true;
        /// <inheritdoc/>
        public string FileName { get; set; } = "orpheus";
        /// <inheritdoc/>
        public string FolderName { get; set; } = "Orpheus";
        /// <inheritdoc/>
        public string FileExtension { get; set; } = "log";
    }
}
