using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrpheusCore;
using OrpheusCore.Configuration;
using OrpheusCore.Configuration.Models;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using System;

namespace OrpheusPostgreSQLDDLHelper
{
    /// <summary>
    /// Registers the services needed for an <see cref="IOrpheusDatabase"/> backed by PostgreSQL,
    /// mirroring the EF Core "UseNpgsql" provider-registration pattern.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a pooled PostgreSQL <see cref="IOrpheusConnectionFactory"/>, the PostgreSQL
        /// <see cref="IOrpheusDDLHelper"/>, and <see cref="IOrpheusDatabase"/> itself, using the
        /// supplied connection configuration.
        /// </summary>
        public static IServiceCollection AddOrpheusPostgreSql(this IServiceCollection services, IDatabaseConnectionConfiguration connectionConfiguration)
        {
            if (connectionConfiguration == null)
                throw new ArgumentNullException(nameof(connectionConfiguration));

            services.AddOrpheusServices();
            services.AddTransient<IOrpheusConnectionFactory>(_ => new NpgsqlConnectionFactory { ConnectionConfiguration = connectionConfiguration });
            services.AddTransient<IOrpheusDDLHelper, OrpheusPostgreSQLDDLHelper>();
            services.AddTransient<IOrpheusDatabase>(sp =>
            {
                var db = new OrpheusDatabase(
                    sp.GetRequiredService<IOrpheusConnectionFactory>(),
                    sp.GetRequiredService<IOrpheusDDLHelper>(),
                    sp.GetRequiredService<ILogger<IOrpheusDatabase>>(),
                    sp,
                    sp.GetService<ILoggerFactory>());
                db.DatabaseConnectionConfiguration = connectionConfiguration;
                return db;
            });
            return services;
        }

        /// <summary>
        /// Registers Orpheus for PostgreSQL, building the connection configuration inline.
        /// </summary>
        public static IServiceCollection AddOrpheusPostgreSql(this IServiceCollection services, Action<IDatabaseConnectionConfiguration> configureConnection)
        {
            if (configureConnection == null)
                throw new ArgumentNullException(nameof(configureConnection));

            var config = new DatabaseConnectionConfiguration();
            configureConnection(config);
            return services.AddOrpheusPostgreSql(config);
        }

        /// <summary>
        /// Registers Orpheus for PostgreSQL, resolving the named connection out of an
        /// <see cref="IConfiguration"/> (e.g. an OrpheusConfig.json file bound via
        /// <see cref="ConfigurationBuilder"/>).
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        /// <param name="configuration">The root configuration (e.g. loaded from OrpheusConfig.json).</param>
        /// <param name="connectionName">The <c>ConfigurationName</c> of the connection to use.</param>
        /// <param name="sectionName">The section Orpheus's settings live under. Defaults to "OrpheusConfiguration".</param>
        public static IServiceCollection AddOrpheusPostgreSql(this IServiceCollection services, IConfiguration configuration, string connectionName, string sectionName = "OrpheusConfiguration")
        {
            return services.AddOrpheusPostgreSql(configuration.GetOrpheusConnection(connectionName, sectionName));
        }
    }
}
