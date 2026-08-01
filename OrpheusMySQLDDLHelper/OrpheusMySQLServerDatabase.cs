using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OrpheusCore;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using System;

namespace OrpheusMySQLDDLHelper
{
    /// <summary>
    /// Factory for creating an OrpheusDatabase backed by a pooled MySQL connection,
    /// without requiring a dependency injection container.
    /// </summary>
    public static class OrpheusMySQLServerDatabase
    {
        /// <summary>
        /// Creates an <see cref="IOrpheusDatabase"/> with a pooled <see cref="MySqlConnectionFactory"/>
        /// and MySQL DDL helper, using the supplied connection configuration.
        /// </summary>
        /// <param name="connectionConfiguration">The database connection configuration.</param>
        /// <param name="loggerFactory">Optional logger factory. When omitted, logging is a no-op.</param>
        public static IOrpheusDatabase CreateDatabase(IDatabaseConnectionConfiguration connectionConfiguration, ILoggerFactory loggerFactory = null)
        {
            if (connectionConfiguration == null)
                throw new ArgumentNullException(nameof(connectionConfiguration));

            // OrpheusDatabase resolves internal helper types (IOrpheusTableOptions, IOrpheusTableKeyField,
            // etc.) from its serviceProvider, and also uses its loggerFactory field directly (e.g. in
            // CreateTable<T>()) — so both need a real, non-null instance even though the caller isn't
            // using DI and didn't supply logging.
            var effectiveLoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;

            var services = new ServiceCollection();
            services.AddSingleton(effectiveLoggerFactory);
            services.AddOrpheusServices();
            var serviceProvider = services.BuildServiceProvider();

            var connectionFactory = new MySqlConnectionFactory { ConnectionConfiguration = connectionConfiguration };
            var helper = new OrpheusMySQLServerDDLHelper(effectiveLoggerFactory.CreateLogger<OrpheusMySQLServerDDLHelper>());
            var db = new OrpheusDatabase(
                connectionFactory,
                helper,
                effectiveLoggerFactory.CreateLogger<IOrpheusDatabase>(),
                serviceProvider,
                effectiveLoggerFactory);
            db.DatabaseConnectionConfiguration = connectionConfiguration;
            return db;
        }

        /// <summary>
        /// Creates an <see cref="IOrpheusDatabase"/>, building the connection configuration inline.
        /// </summary>
        /// <param name="configureConnection">Callback to populate the connection configuration.</param>
        /// <param name="loggerFactory">Optional logger factory. When omitted, logging is a no-op.</param>
        public static IOrpheusDatabase CreateDatabase(Action<IDatabaseConnectionConfiguration> configureConnection, ILoggerFactory loggerFactory = null)
        {
            if (configureConnection == null)
                throw new ArgumentNullException(nameof(configureConnection));

            var config = new OrpheusCore.Configuration.Models.DatabaseConnectionConfiguration();
            configureConnection(config);
            return CreateDatabase(config, loggerFactory);
        }
    }
}
