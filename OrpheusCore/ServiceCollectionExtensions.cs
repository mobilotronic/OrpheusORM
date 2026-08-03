using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrpheusCore.Configuration.Models;
using OrpheusCore.SchemaBuilder;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System.Linq;

namespace OrpheusCore
{
    /// <summary>
    /// Registers the internal services Orpheus needs at runtime.
    /// </summary>
    public static class OrpheusServiceCollectionExtensions
    {
        /// <summary>
        /// Registers Orpheus's internal services: table options, module and schema types, a default
        /// <see cref="IDatabaseConnectionConfiguration"/>, the default key generator, and (only if
        /// nothing has registered <see cref="ILoggerFactory"/> yet) a console logger fallback.
        /// </summary>
        /// <remarks>
        /// Engine-specific connection factories and DDL helpers are <b>not</b> registered here — use
        /// the per-engine <c>AddOrpheusSqlServer</c>/<c>AddOrpheusMySql</c>/<c>AddOrpheusPostgreSql</c>
        /// extension methods, which call this internally, rather than calling it directly.
        /// </remarks>
        /// <param name="services">The service collection to register Orpheus's services into.</param>
        /// <returns>The same <paramref name="services"/> instance, for chaining.</returns>
        public static IServiceCollection AddOrpheusServices(this IServiceCollection services)
        {
            services.AddTransient<IOrpheusTableOptions, OrpheusTableOptions>();
            services.AddTransient<IOrpheusModuleDefinition, OrpheusModuleDefinition>();
            services.AddTransient<IOrpheusTableKeyField, OrpheusTableKeyField>();
            services.AddTransient<IOrpheusModule, OrpheusModule>();

            // ISchema is deliberately absent: a schema needs a database, so it is created through
            // IOrpheusDatabase.CreateSchema rather than resolved from the container.
            services.AddTransient<ISchemaView, SchemaObjectView>();
            services.AddTransient<ISchemaViewTable, SchemaObjectViewTable>();
            services.AddTransient<ISchemaTable, SchemaObjectTable>();
            services.AddTransient<ISchemaObject, SchemaObject>();
            services.AddTransient<ISchemaJoinDefinition, SchemaJoinDefinition>();
            services.AddTransient<ISchemaDataObject, SchemaDataObject>();

            services.AddTransient<IDatabaseConnectionConfiguration, DatabaseConnectionConfiguration>();

            // TryAdd so an AddOrpheusKeyGenerator<T>() call before this one wins.
            services.TryAddSingleton<IOrpheusKeyGenerator, SequentialGuidKeyGenerator>();

            var isLoggingRegistered = services.Any(sd => sd.ServiceType == typeof(ILoggerFactory));
            if (!isLoggingRegistered)
            {
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                });
            }

            return services;
        }

        /// <summary>
        /// Replaces the <see cref="IOrpheusKeyGenerator"/> used for auto-generated Guid keys. The
        /// default is <see cref="SequentialGuidKeyGenerator"/> (UUIDv7); register
        /// <see cref="RandomGuidKeyGenerator"/> to go back to random version 4 values.
        /// </summary>
        /// <remarks>
        /// Order does not matter relative to <c>AddOrpheusSqlServer</c> and friends: the default is
        /// only registered if nothing else has claimed the service, and this replaces whatever has.
        /// </remarks>
        public static IServiceCollection AddOrpheusKeyGenerator<T>(this IServiceCollection services)
            where T : class, IOrpheusKeyGenerator
        {
            services.RemoveAll<IOrpheusKeyGenerator>();
            services.AddSingleton<IOrpheusKeyGenerator, T>();
            return services;
        }
    }
}
