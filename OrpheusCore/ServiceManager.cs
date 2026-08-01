using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrpheusCore.Configuration.Models;
using OrpheusCore.SchemaBuilder;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrpheusCore
{
    /// <summary>
    /// Class to register internal services needed by Orpheus.
    /// In v2.0.0+, prefer constructor injection via IServiceProvider/ILoggerFactory
    /// over the static methods. The ServiceProvider property is deprecated.
    /// </summary>
    public static class ServiceManager
    {
        #region private
        private static ILoggerFactory loggerFactory;

        private static void initializeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IOrpheusTableOptions, OrpheusTableOptions>();
            serviceCollection.AddTransient<IOrpheusModuleDefinition, OrpheusModuleDefinition>();
            serviceCollection.AddTransient<IOrpheusTableKeyField, OrpheusTableKeyField>();
            serviceCollection.AddTransient<IOrpheusModule, OrpheusModule>();

            serviceCollection.AddTransient<ISchema, Schema>();
            serviceCollection.AddTransient<ISchemaView, SchemaObjectView>();
            serviceCollection.AddTransient<ISchemaViewTable, SchemaObjectViewTable>();
            serviceCollection.AddTransient<ISchemaTable, SchemaObjectTable>();
            serviceCollection.AddTransient<ISchemaObject, SchemaObject>();
            serviceCollection.AddTransient<ISchemaJoinDefinition, SchemaJoinDefinition>();
            serviceCollection.AddTransient<ISchemaDataObject, SchemaDataObject>();

            serviceCollection.AddTransient<IDatabaseConnectionConfiguration, DatabaseConnectionConfiguration>();

            var isLoggingRegistered = serviceCollection.Any(sd => sd.ServiceType == typeof(ILoggerFactory));
            if (!isLoggingRegistered)
            {
                serviceCollection.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                });
            }
        }

        /// <summary>
        /// Finds a constructor whose parameter types are assignable from
        /// the provided arguments (not just exact type match).
        /// </summary>
        private static ConstructorInfo FindMatchingConstructor(Type type, object[] args)
        {
            foreach (var ctor in type.GetConstructors())
            {
                var parameters = ctor.GetParameters();
                if (parameters.Length != args.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != null && !parameters[i].ParameterType.IsAssignableFrom(args[i].GetType()))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return ctor;
            }
            return null;
        }
        #endregion

        #region initialization
        /// <summary>
        /// Registers the internal services Orpheus needs at runtime: table options, module and
        /// schema types, a default <see cref="IDatabaseConnectionConfiguration"/>, and (only if
        /// nothing has registered <see cref="ILoggerFactory"/> yet) a console logger fallback.
        /// Engine-specific connection factories and DDL helpers are <b>not</b> registered here — use
        /// the per-engine <c>AddOrpheusSqlServer</c>/<c>AddOrpheusMySql</c>/<c>AddOrpheusPostgreSql</c>
        /// extension methods (which call this internally) instead of calling this directly.
        /// </summary>
        /// <param name="services">The service collection to register Orpheus's services into.</param>
        /// <returns>The same <paramref name="services"/> instance, for chaining.</returns>
        public static IServiceCollection AddOrpheusServices(this IServiceCollection services)
        {
            initializeServices(services);
            return services;
        }
        #endregion

        #region service resolution

        /// <summary>
        /// The service provider used by <see cref="Resolve{T}()"/>, <see cref="LoggerFactory"/>, and
        /// the other static resolution helpers below. Must be assigned (typically right after
        /// building the service collection with <c>BuildServiceProvider()</c>) before calling them.
        /// </summary>
        [Obsolete("Prefer constructor injection of IServiceProvider. This static property is retained for backward compatibility.")]
        public static IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Resolves a service of type <typeparamref name="T"/> from <see cref="ServiceProvider"/>.
        /// </summary>
        public static T Resolve<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// Resolves a service by type from <see cref="ServiceProvider"/>. When
        /// <paramref name="constructorParameters"/> is supplied, instead of returning the
        /// DI-resolved instance directly, this looks up the resolved service's concrete type and
        /// invokes whichever of its constructors' parameter types are assignable from the supplied
        /// arguments — for constructing an instance with runtime-known arguments that aren't
        /// themselves resolvable from the container.
        /// </summary>
        /// <param name="serviceType">The service type to resolve.</param>
        /// <param name="constructorParameters">
        /// Constructor arguments to invoke a matching constructor with, or null/empty to just return
        /// the DI-resolved instance.
        /// </param>
        public static object Resolve(Type serviceType, object[] constructorParameters)
        {
            if (constructorParameters == null || constructorParameters.Length == 0)
                return ServiceProvider.GetService(serviceType);

            var concreteType = ServiceProvider.GetService(serviceType)?.GetType();
            if (concreteType == null)
                return null;

            var ctor = FindMatchingConstructor(concreteType, constructorParameters);
            if (ctor != null)
                return ctor.Invoke(constructorParameters);

            return null;
        }

        /// <summary>
        /// Generic counterpart of <see cref="Resolve(Type, object[])"/>.
        /// </summary>
        /// <param name="constructorParameters">
        /// Constructor arguments to invoke a matching constructor with, or null/empty to just return
        /// the DI-resolved instance.
        /// </param>
        public static T Resolve<T>(object[] constructorParameters)
        {
            if (constructorParameters == null || constructorParameters.Length == 0)
                return ServiceProvider.GetService<T>();

            var concreteType = ServiceProvider.GetService<T>()?.GetType();
            if (concreteType == null)
                return default;

            var ctor = FindMatchingConstructor(concreteType, constructorParameters);
            if (ctor != null)
                return (T)ctor.Invoke(constructorParameters);

            return default;
        }

        /// <summary>
        /// The <see cref="ILoggerFactory"/> resolved from <see cref="ServiceProvider"/>, cached
        /// after the first access.
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (loggerFactory == null)
                    loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
                return loggerFactory;
            }
        }

        /// <summary>
        /// Creates a logger for <typeparamref name="T"/> via <see cref="LoggerFactory"/>. Returns
        /// null if no <see cref="ILoggerFactory"/> is registered in <see cref="ServiceProvider"/>.
        /// </summary>
        public static ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory?.CreateLogger<T>();
        }

        /// <summary>
        /// Resolves an arbitrary service by type from <see cref="ServiceProvider"/>. Despite the
        /// name, this is not limited to logging-related services — it's a general-purpose lookup.
        /// </summary>
        /// <param name="type">The service type to resolve.</param>
        public static object GetLoggerService(Type type)
        {
            return ServiceProvider.GetService(type);
        }
        #endregion
    }
}
