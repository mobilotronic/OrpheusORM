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
        public static IServiceCollection AddOrpheusServices(this IServiceCollection services)
        {
            initializeServices(services);
            return services;
        }
        #endregion

        #region service resolution

        [Obsolete("Prefer constructor injection of IServiceProvider. This static property is retained for backward compatibility.")]
        public static IServiceProvider ServiceProvider { get; set; }

        public static T Resolve<T>()
        {
            return ServiceProvider.GetService<T>();
        }

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

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (loggerFactory == null)
                    loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
                return loggerFactory;
            }
        }

        public static ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory?.CreateLogger<T>();
        }

        public static object GetLoggerService(Type type)
        {
            return ServiceProvider.GetService(type);
        }
        #endregion
    }
}
