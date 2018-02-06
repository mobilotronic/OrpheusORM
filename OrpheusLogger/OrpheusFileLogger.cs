using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;
using OrpheusCore.Configuration;

namespace OrpheusLogger
{
    /// <summary>
    /// Orpheus default logger implementation.
    /// </summary>
    public class OrpheusFileLogger : ILogger
    {
        #region private properties
        private object objectLock = new object();
        private string logFileName;
        private const string information = "Information";
        private const string warning = "Warning";
        private const string error = "Error";
        private const string logEntryFormat = "{0} {1} {2}";
        private const string callStacklogEntryFormat = "{0},Line:{1} {2} {3} {4}";
        private const string logPrefix = "ORPHEUS_";
        private const string fileExtension = ".log";
        private const string debug = "Debug";
        #endregion

        #region private methods
        private bool needToStartNewFile()
        {
            return this.getFileSizeInMB(this.logFileName) > ConfigurationManager.Configuration.Logging.MaxFileSize;
        }
        private double getFileSizeInMB(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return (fileInfo.Length / 1024) / 1024;
        }

        private void createLogFile()
        {
            var logFilePath = String.IsNullOrWhiteSpace(ConfigurationManager.Configuration.Logging.FilePath) ?  Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" :
                ConfigurationManager.Configuration.Logging.FilePath;
            logFilePath = Path.Combine(logFilePath, " ").Trim();

            Directory.CreateDirectory(logFilePath + "Orpheus");
            this.logFileName = logFilePath + @"Orpheus\" + logPrefix + DateTime.Now.ToString("yyyy-MM-dd");
            //getting existing logfiles.
            var logFiles = Directory.GetFiles(logFilePath + "Orpheus").ToList();
            var lastLogFile = logFiles.Where(logFile => logFile.Contains(this.logFileName)).LastOrDefault();
            if(lastLogFile != null)
            {
                var sizeInMB = this.getFileSizeInMB(lastLogFile);
                if (sizeInMB > ConfigurationManager.Configuration.Logging.MaxFileSize)
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
            if (stackFrame != null)
            {
                return string.Format(callStacklogEntryFormat,
                    Path.GetFileName(stackFrame.GetFileName()).PadRight(20),
                    stackFrame.GetFileLineNumber().ToString().PadRight(15),
                    entryType.PadRight(8),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(20),
                    logEntry.PadRight(50));
            }
            else
            {
                return string.Format(logEntryFormat,
                    entryType.PadRight(25),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(20),
                    logEntry.PadRight(50));
            }
        }

        private void initialize()
        {
            this.createLogFile();
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
            Enum.TryParse<LogLevel>(ConfigurationManager.Configuration.Logging.Level, out configurationLevel);

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
                if (!String.IsNullOrWhiteSpace(ConfigurationManager.Configuration.Logging.FilePath))
                {
                    if (Path.GetFullPath(this.LogFileName).ToLower() != Path.GetFullPath(ConfigurationManager.Configuration.Logging.FilePath).ToLower())
                        this.initialize();
                }
                if (!File.Exists(this.logFileName) ||  this.needToStartNewFile())
                    this.initialize();
                using(var fileStream = new FileStream(this.logFileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    var logWriter = new StreamWriter(fileStream);
                    logWriter.AutoFlush = true;
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
                            case LogLevel.Error:
                                {
                                    if (this.IsEnabled(logLevel))
                                    {
                                        var stackTrace = new System.Diagnostics.StackTrace(true);
                                        var frame = stackTrace.GetFrame(2);
                                        logWriter.WriteLine(this.formatLogEntry(error, message,frame));
                                    }
                                    break;
                                }
                            case LogLevel.Debug:
                                {
                                    if (this.IsEnabled(logLevel))
                                    {
                                        var stackTrace = new System.Diagnostics.StackTrace(true);
                                        var frame = stackTrace.GetFrame(2);
                                        logWriter.WriteLine(this.formatLogEntry(debug, message, frame));
                                    }
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
        /// Creates the default Orpheus logger.
        /// </summary>
        public OrpheusFileLogger()
        {
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
