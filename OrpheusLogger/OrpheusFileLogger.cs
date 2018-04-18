using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrpheusCore.Configuration;
using OrpheusInterfaces.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace OrpheusLogger
{
    internal class LogEntry : ILogEntry
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
        public string SourceFile { get; set; }
        public string LineNumber { get; set; }
    }

    /// <summary>
    /// Orpheus logger interface.
    /// </summary>
    public interface IOrpheusFileLogger : ILogger, IOrpheusLogger
    {
       
        /// <summary>
        /// Log filename.
        /// </summary>
        string LogFileName { get; }
    }

    /// <summary>
    /// Orpheus default logger implementation.
    /// </summary>
    public class OrpheusFileLogger : IOrpheusFileLogger
    {
        #region private properties
        private object objectLock = new object();
        private string logFileName;
        private ILoggingConfiguration loggingConfiguration;
        private const string information = "Information";
        private const string warning = "Warning";
        private const string error = "Error";
        private const string critical = "Critical";
        private const string logEntryFormat = "{0} {1} {2}";
        private const string callStacklogEntryFormat = "{0},Line:{1} {2} {3} {4}";
        private const string logPrefix = "ORPHEUS_";
        private const string fileExtension = ".log";
        private const string debug = "Debug";
        private const string trace = "Trace";
        private IOptionsMonitor<LoggingConfiguration> optionsMonitor;
        #endregion

        #region private methods
        private bool needToStartNewFile()
        {
            return this.getFileSizeInMB(this.logFileName) > this.loggingConfiguration.MaxFileSize;
        }
        private double getFileSizeInMB(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return (fileInfo.Length / 1024) / 1024;
        }

        private void createLogFile()
        {
            var logFilePath = String.IsNullOrWhiteSpace(this.loggingConfiguration.FilePath) ?  Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" :
                this.loggingConfiguration.FilePath;
            logFilePath = Path.Combine(logFilePath, " ").Trim();

            Directory.CreateDirectory(logFilePath + "Orpheus");
            this.logFileName = logFilePath + @"Orpheus\" + logPrefix + DateTime.Now.ToString("yyyy-MM-dd");
            //getting existing log files.
            var logFiles = Directory.GetFiles(logFilePath + "Orpheus").ToList();
            var lastLogFile = logFiles.Where(logFile => logFile.Contains(this.logFileName)).LastOrDefault();
            if(lastLogFile != null)
            {
                var sizeInMB = this.getFileSizeInMB(lastLogFile);
                if (sizeInMB > this.loggingConfiguration.MaxFileSize)
                    this.logFileName = this.logFileName + string.Format("_{0}", logFiles.Count() + 1) + fileExtension;
                else
                    this.logFileName = lastLogFile;
            }
            else
            {
                this.logFileName = this.logFileName + fileExtension;
            }
            using (var fs = new System.IO.FileStream(this.logFileName, FileMode.OpenOrCreate))
            {
                var streamWriter = new StreamWriter(fs);
                try
                {
                    streamWriter.WriteLine("Orpheus ORM log start");
                }
                finally
                {
                    streamWriter.Close();
                    fs.Close();
                }
            }
        }

        private string formatLogEntry(string entryType, string logEntry, System.Diagnostics.StackFrame stackFrame = null)
        {
            var logEntryModel = new LogEntry() {
                Type = entryType,
                Message = logEntry,
                TimeStamp = DateTime.Now,
                SourceFile = stackFrame?.GetFileName(),
                LineNumber = stackFrame?.GetFileLineNumber().ToString()
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
            this.loggingConfiguration = loggingConfiguration;
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
            Enum.TryParse<LogLevel>(this.loggingConfiguration.Level, out configurationLevel);

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
            lock (this.objectLock)
            {
                if (!String.IsNullOrWhiteSpace(this.loggingConfiguration.FilePath))
                {
                    if (Path.GetFullPath(this.LogFileName).ToLower() != Path.GetFullPath(this.loggingConfiguration.FilePath).ToLower())
                        this.initialize();
                }
                if (!File.Exists(this.logFileName) ||  this.needToStartNewFile())
                    this.initialize();
                using(var fileStream = new FileStream(this.logFileName, FileMode.Append, FileAccess.Write, FileShare.Read))
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
                        switch (logLevel)
                        {
                            case LogLevel.Information:
                                {
                                    if (this.IsEnabled(logLevel))
                                        logWriter.WriteLine(this.formatLogEntry(information, message));
                                    break;
                                }
                            case LogLevel.Warning:
                                {
                                    if (this.IsEnabled(logLevel))
                                        logWriter.WriteLine(this.formatLogEntry(warning, message));
                                    break;
                                }
                            case LogLevel.Critical:
                                {
                                    if (this.IsEnabled(logLevel))
                                        logWriter.WriteLine(this.formatLogEntry(critical, message));
                                    break;
                                }
                            case LogLevel.Error:
                                {
                                    if (this.IsEnabled(logLevel))
                                        logWriter.WriteLine(this.formatLogEntry(error, message));
                                    break;
                                }
                            case LogLevel.Debug:
                                {
                                    if (this.IsEnabled(logLevel))
                                    {
                                        var stackTrace = new System.Diagnostics.StackTrace(true);
                                        var frame = stackTrace.GetFrame(2);
                                        logWriter.WriteLine(this.formatLogEntry(debug,message,frame));
                                    }
                                    break;
                                }
                            case LogLevel.Trace:
                                {
                                    if (this.IsEnabled(logLevel))
                                        logWriter.WriteLine(this.formatLogEntry(trace, message));
                                    break;
                                }
                            default:
                                {
                                    if (this.IsEnabled(logLevel))
                                        logWriter.WriteLine(this.formatLogEntry(information, message));
                                    break;
                                }
                        }
                    }
                    finally
                    {
                        logWriter.Close();
                        fileStream.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Log file name.
        /// </summary>
        public string LogFileName { get { return this.logFileName; } }

        /// <summary>
        /// Logger configuration.
        /// </summary>
        public ILoggingConfiguration LoggingConfiguration {
            get
            {
                return this.loggingConfiguration;
            }
            set
            {
                this.loggingConfiguration = value;
            }
        }

        /// <summary>
        /// Creates the default Orpheus logger.
        /// </summary>
        public OrpheusFileLogger(IOptionsMonitor<LoggingConfiguration> options)
        {
            this.optionsMonitor = options;
            this.loggingConfiguration = this.optionsMonitor.CurrentValue ?? new LoggingConfiguration()
            {
                Level = "Error",
                MaxFileSize = 10
            };
            this.initialize();
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~OrpheusFileLogger()
        {
        }
    }
}
