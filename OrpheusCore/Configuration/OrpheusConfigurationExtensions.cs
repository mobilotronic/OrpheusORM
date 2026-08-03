using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrpheusCore.Configuration.Models;
using OrpheusInterfaces.Configuration;
using System;
using System.Linq;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Extension methods for resolving an <see cref="IDatabaseConnectionConfiguration"/> out of
    /// an <see cref="IConfiguration"/> (e.g. an appsettings/OrpheusConfig.json file).
    /// </summary>
    public static class OrpheusConfigurationExtensions
    {
        /// <summary>
        /// Binds the given configuration section to an <see cref="Models.OrpheusConfiguration"/> and
        /// returns the named database connection from its <see cref="Models.OrpheusConfiguration.DatabaseConnections"/>.
        /// </summary>
        /// <param name="configuration">The root configuration (e.g. loaded from OrpheusConfig.json).</param>
        /// <param name="connectionName">The <see cref="IDatabaseConnectionConfiguration.ConfigurationName"/> to look up.</param>
        /// <param name="sectionName">The configuration section Orpheus's settings live under. Defaults to "OrpheusConfiguration".</param>
        public static IDatabaseConnectionConfiguration GetOrpheusConnection(this IConfiguration configuration, string connectionName, string sectionName = "OrpheusConfiguration")
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrEmpty(connectionName))
                throw new ArgumentNullException(nameof(connectionName));

            var orpheusConfig = new OrpheusConfiguration();
            configuration.GetSection(sectionName).Bind(orpheusConfig);
            var connectionConfig = orpheusConfig.DatabaseConnections?.FirstOrDefault(c => string.Equals(c.ConfigurationName, connectionName));
            if (connectionConfig == null)
                throw new InvalidOperationException($"No database connection named '{connectionName}' was found in configuration section '{sectionName}'.");

            return connectionConfig;
        }

        /// <summary>
        /// Binds Orpheus's own settings (currently <see cref="Models.OrpheusConfiguration.DefaultStringSize"/>)
        /// from the given configuration section, so components that need them can resolve
        /// <c>IOptions&lt;OrpheusConfiguration&gt;</c> from the service provider.
        /// </summary>
        /// <remarks>
        /// Called for you by the <c>AddOrpheusSqlServer</c>/<c>AddOrpheusMySql</c>/<c>AddOrpheusPostgreSql</c>
        /// overloads that take an <see cref="IConfiguration"/>. Everything has a working default, so
        /// applications that never call this still generate schema correctly.
        /// </remarks>
        public static IServiceCollection AddOrpheusConfiguration(this IServiceCollection services, IConfiguration configuration, string sectionName = "OrpheusConfiguration")
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.Configure<OrpheusConfiguration>(configuration.GetSection(sectionName));
            return services;
        }
    }
}
