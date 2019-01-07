# Configuring NLog
To configure NLog to work with Orpheus, you'll need to have a wrapper class.

Prerequisite is to add [NLog](https://github.com/NLog/NLog.Extensions.Logging) to your project.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace YourNameSpace
{
    /// <summary>
    /// NLog wrapper
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILogger" />
    public class CommonLogger : ILogger
    {
        private ILogger nlogLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonLogger"/> class.
        /// </summary>
        public CommonLogger()
        {
            //your ServiceProvider instance.
            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
            //load nlog configuration
            NLog.Web.NLogBuilder.ConfigureNLog("logging.config");
            this.nlogLogger = loggerFactory.CreateLogger("YourLoggerName");
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>
        /// An IDisposable that ends the logical operation scope on dispose.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public IDisposable BeginScope<TState>(TState state)
        {
            return this.nlogLogger.BeginScope<TState>(state);
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns>
        ///   <c>true</c> if enabled.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsEnabled(LogLevel logLevel)
        {
            return this.nlogLogger.IsEnabled(logLevel);
        }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.nlogLogger.Log<TState>(logLevel, eventId, state, exception, formatter);
        }
    }
}
```
After that you'll have to register your wrapper into Orpheu's configuration.
```json
  "Services": [
    {
      "Implementation": "System.Data.SqlClient.SqlConnection, System.Data.SqlClient, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
      "Service": "System.Data.IDbConnection, System.Data.Common, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
      "ServiceLifeTime": "Transient"
    },
    {
      "Implementation": "OrpheusCore.OrpheusDatabase, OrpheusCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
      "Service": "OrpheusInterfaces.Core.IOrpheusDatabase, OrpheusInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
      "ServiceLifeTime": "Transient"
    },
    {
      "Implementation": "OrpheusSQLDDLHelper.OrpheusSQLServerDDLHelper, OrpheusSQLServerDDLHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
      "Service": "OrpheusInterfaces.Core.IOrpheusDDLHelper, OrpheusInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
      "ServiceLifeTime": "Transient"
    },
    {
      "Implementation": "YourAssembly.CommonLogger, YourAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
      "Service": "Microsoft.Extensions.Logging.ILogger, Microsoft.Extensions.Logging.Abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60",
      "ServiceLifeTime": "Transient"
    }
  ],
```

Other logging frameworks, might require a similar approach.