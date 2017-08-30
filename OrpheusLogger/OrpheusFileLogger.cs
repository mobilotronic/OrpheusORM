using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;

namespace OrpheusLogger
{
    public class OrpheusFileLogger : ILogger
    {
        #region private properties
        private object objectLock = new object();
        private string logFileName;
        private const string information = "Information";
        private const string warning = "Warning";
        private const string error = "Error";
        private const string logEntryFormat = "{0} DateTime:{1} Message:{2}";
        private const string logPrefix = "ORPHEUS_";
        private const string fileExtension = ".log";
        #endregion

        #region private methods
        private bool needToStartNewFile()
        {
            return this.getFileSizeInMB(this.logFileName) > OrpheusLoggerConfiguration.Configuration.MaxFileSize;
        }
        private double getFileSizeInMB(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return (fileInfo.Length / 1024) / 1024;
        }

        private void createLogFile()
        {
            var logFilePath = String.IsNullOrWhiteSpace(OrpheusLoggerConfiguration.Configuration.FilePath) ?  Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" :
                OrpheusLoggerConfiguration.Configuration.FilePath;
            logFilePath = Path.Combine(logFilePath, " ").Trim();

            Directory.CreateDirectory(logFilePath + "Orpheus");
            this.logFileName = logFilePath + @"Orpheus\" + logPrefix + DateTime.Now.ToString("yyyy-MM-dd");
            //getting existing logfiles.
            var logFiles = Directory.GetFiles(logFilePath + "Orpheus").ToList();
            var lastLogFile = logFiles.Where(logFile => logFile.Contains(this.logFileName)).LastOrDefault();
            if(lastLogFile != null)
            {
                var sizeInMB = this.getFileSizeInMB(lastLogFile);
                if (sizeInMB > OrpheusLoggerConfiguration.Configuration.MaxFileSize)
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

        private string formatLogEntry(string entryType, string logEntry)
        {
            return string.Format(logEntryFormat, 
                entryType.PadRight(12), 
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").PadRight(20), 
                logEntry.PadRight(50));
        }

        private void initialize()
        {
            this.createLogFile();
        }
        #endregion

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            //if passed in log level do nothing.
            if (logLevel == LogLevel.None)
                return false;
            LogLevel configurationLevel = LogLevel.None;
            Enum.TryParse<LogLevel>(OrpheusLoggerConfiguration.Configuration.Level, out configurationLevel);

            //if configured level is none, then do nothing.
            if (configurationLevel == LogLevel.None)
                return false;
            return logLevel >= configurationLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            lock (this.objectLock)
            {
                if (!String.IsNullOrWhiteSpace(OrpheusLoggerConfiguration.Configuration.FilePath))
                {
                    if (Path.GetFullPath(this.LogFileName).ToLower() != Path.GetFullPath(OrpheusLoggerConfiguration.Configuration.FilePath).ToLower())
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
                                        logWriter.WriteLine(this.formatLogEntry(error, message));
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

        public string LogFileName { get { return this.logFileName; } }

        public OrpheusFileLogger()
        {
            this.initialize();
        }

        ~OrpheusFileLogger()
        {
        }
    }
}
