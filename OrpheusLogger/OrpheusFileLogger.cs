using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrpheusCore.Configuration;
using OrpheusInterfaces.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace OrpheusLogger
{
    /// <summary>
    /// Log entry model.
    /// </summary>
    /// <seealso cref="OrpheusInterfaces.Logging.ILogEntry" />
    public class LogEntry : ILogEntry
    {
        /// <summary>
        /// Log entry type. Error, Information, Debug etc.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Log entry message.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Log entry time-stamp.
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// Stack trace
        /// </summary>
        public string StackTrace { get; set; }
    }

    /// <summary>
    /// Orpheus default logger implementation.
    /// </summary>
    public class OrpheusFileLogger : IOrpheusLogger
    {
        #region private properties
        #region private properties
        private string defaultFileName = "oprheusFileLog";
        private string defaultFolderName = "Orpheus";
        private string defaultFileExtension = "log";
        private string newLogFileNamePath;
        private IOptionsMonitor<LoggingConfiguration> optionsMonitor;
        private object objectLock = new object();
        private LogLevel configurationLogLevel = LogLevel.Information;
        #endregion
        #endregion

        #region private methods
        private bool needToStartNewFile()
        {
            return this.getFileSizeInMB(this.newLogFileNamePath) > this.Configuration.MaxFileSize;
        }
        private double getFileSizeInMB(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return (fileInfo.Length / 1024) / 1024;
        }

        private void createLogFile()
        {
            newLogFileNamePath = null;
            var logFilePath = String.IsNullOrWhiteSpace(this.Configuration.FilePath) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\{this.defaultFolderName}":
                this.Configuration.FilePath;
            logFilePath = Path.Combine(logFilePath, " ").Trim();
            if (!Directory.Exists(logFilePath))
            {
                try
                {
                    Directory.CreateDirectory(logFilePath);
                }
                catch
                {
                    throw new UnauthorizedAccessException($"Could not create folder {logFilePath}");
                }
            }
            //building the dynamically changing part of the file name.
            var dynamicFilenamePart = DateTime.Now.ToString("yyyy-MM-dd");
            dynamicFilenamePart = this.Configuration.FileExtension == null ? $"{dynamicFilenamePart}.{defaultFileExtension}" : $"{dynamicFilenamePart}.{this.Configuration.FileExtension.Replace(".", "")}";

            //building the new log file name. just the name, not the full file name path.
            var newLogFileName = String.IsNullOrEmpty(this.Configuration.FileName) ? defaultFileName : this.Configuration.FileName;

            //setting the new full file name path.
            newLogFileNamePath = $"{logFilePath}\\{newLogFileName}_{dynamicFilenamePart}";

            //getting existing log files.
            var logFiles = Directory.GetFiles(logFilePath).ToList();
            var lastLogFile = logFiles.Where(logFile => logFile.Contains(newLogFileName)).LastOrDefault();
            if (lastLogFile != null)
            {
                var sizeInMB = this.getFileSizeInMB(lastLogFile);
                if (sizeInMB > this.Configuration.MaxFileSize)
                    newLogFileNamePath = $"{newLogFileNamePath}_{logFiles.Count() + 1}{dynamicFilenamePart}";
                else
                    newLogFileNamePath = lastLogFile;
            }

            using (var fs = new System.IO.FileStream(newLogFileNamePath, FileMode.OpenOrCreate))
            {
                var streamWriter = new StreamWriter(fs);
                try
                {
                    streamWriter.WriteLine("Simple file log start");
                }
                finally
                {
                    streamWriter.Close();
                    fs.Close();
                }
            }

            this.Configuration.FileName = this.newLogFileNamePath;
        }

        private string formatLogEntry(string entryType, string logEntry,string stackTrace = null)
        {
            var logEntryModel = new LogEntry() {
                Type = entryType,
                Message = logEntry,
                TimeStamp = DateTime.Now,
                StackTrace = stackTrace
            };
            return JsonConvert.SerializeObject(logEntryModel);
        }

        private void initialize()
        {
            this.createLogFile();
            //monitor option changes in a scenario where the ILogger is configured as a Singleton.
            this.optionsMonitor.OnChange(this.configurationChanged);
        }

        /// <summary>
        /// Callback when the underlying configuration file changed.
        /// </summary>
        /// <param name="loggingConfiguration"></param>
        /// <param name="state"></param>
        private void configurationChanged(LoggingConfiguration loggingConfiguration,string state)
        {
            this.Configuration = loggingConfiguration;
            Enum.TryParse<LogLevel>(this.Configuration.Level, out this.configurationLogLevel);
        }

        private string getCallStack()
        {
            var stackTrace = new System.Diagnostics.StackTrace(true);
            var stringBuilder = new StringBuilder();
            foreach (var f in stackTrace.GetFrames())
            {
                var fileName = f.GetFileName();
                if (!String.IsNullOrEmpty(fileName))
                    stringBuilder.AppendLine($"Method:{f.GetMethod().Name} File:{f.GetFileName()} Line:{f.GetFileLineNumber()} Column:{f.GetFileColumnNumber()}");
            }
            return stringBuilder.ToString();
        }

        #endregion

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns true if the log level is enabled.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            //if passed in log level do nothing.
            if (logLevel == LogLevel.None)
                return false;
            LogLevel configurationLevel = LogLevel.None;
            Enum.TryParse<LogLevel>(this.Configuration.Level, out configurationLevel);

            //if configured level is none, then do nothing.
            if (configurationLevel == LogLevel.None)
                return false;
            return logLevel >= configurationLevel;
        }

        /// <summary>
        /// Enters a log entry.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel">Log level</param>
        /// <param name="eventId">Event id</param>
        /// <param name="state">State</param>
        /// <param name="exception">Exception object</param>
        /// <param name="formatter">Formatter function</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (this.IsEnabled(logLevel))
            {
                lock (this.objectLock)
                {
                    if (!String.IsNullOrWhiteSpace(this.Configuration.FilePath))
                    {
                        if (Path.GetFullPath(this.newLogFileNamePath).ToLower() != Path.GetFullPath(this.Configuration.FilePath).ToLower())
                            this.initialize();
                    }
                    if (!File.Exists(newLogFileNamePath) || this.needToStartNewFile())
                        this.initialize();
                    using (var fileStream = new FileStream(this.newLogFileNamePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        var logWriter = new StreamWriter(fileStream)
                        {
                            AutoFlush = true
                        };
                        var message = formatter(state, exception);
                        try
                        {
                            if (string.IsNullOrEmpty(message) && exception == null)
                                return;


                            var callStack = this.configurationLogLevel == LogLevel.Trace ? this.getCallStack() : null;
                            logWriter.WriteLine(this.formatLogEntry(logLevel.ToString(), message, callStack));
                        }
                        finally
                        {
                            logWriter.Close();
                            fileStream.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IFileLoggingConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the name of the current file.
        /// </summary>
        /// <value>
        /// The name of the current file.
        /// </value>
        public string CurrentFileName => this.newLogFileNamePath;

        /// <summary>
        /// Creates the default Orpheus logger.
        /// </summary>
        public OrpheusFileLogger(IOptionsMonitor<LoggingConfiguration> options)
        {
            this.optionsMonitor = options;
            this.Configuration = this.optionsMonitor.CurrentValue ?? new LoggingConfiguration()
            {
                Level = "Error",
                MaxFileSize = 10
            };
            this.initialize();
        }
    }
}
