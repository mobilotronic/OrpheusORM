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
    /// Class to register internal services needed by Orpheus
    /// </summary>
    public static class ServiceManager
    {
        #region private
        private static Assembly[] assemblies;
        private static ILoggerFactory loggerFactory;
        /// <summary>
        /// This set of services are necessary for Orpheus to function.
        /// </summary>
        /// <param name="serviceCollection"></param>
        private static void initializeServices(IServiceCollection serviceCollection)
        {
            //data services.
            serviceCollection.AddTransient<IOrpheusTableOptions, OrpheusTableOptions>();
            serviceCollection.AddTransient<IOrpheusModuleDefinition, OrpheusModuleDefinition>();
            serviceCollection.AddTransient<IOrpheusTableKeyField, OrpheusTableKeyField>();
            serviceCollection.AddTransient<IOrpheusModule, OrpheusModule>();

            //Schema services.
            serviceCollection.AddTransient<ISchema, Schema>();
            serviceCollection.AddTransient<ISchemaView, SchemaObjectView>();
            serviceCollection.AddTransient<ISchemaViewTable, SchemaObjectViewTable>();
            serviceCollection.AddTransient<ISchemaTable, SchemaObjectTable>();
            serviceCollection.AddTransient<ISchemaObject, SchemaObject>();
            serviceCollection.AddTransient<ISchemaJoinDefinition, SchemaJoinDefinition>();
            serviceCollection.AddTransient<ISchemaDataObject, SchemaDataObject>();
            //configuration services
            serviceCollection.AddTransient<IDatabaseConnectionConfiguration, DatabaseConnectionConfiguration>();
            //if there is no service provider registered, register at least one, so Orpheus can work.
            var isLoggingRegistered = serviceCollection.Where((sd => sd.ServiceType == typeof(ILoggerFactory))).FirstOrDefault();
            if (isLoggingRegistered == null)
            {
                serviceCollection.AddLogging((builder) =>
                {
                    builder.ClearProviders();
                    builder.AddConsole();
                });
            }
        }
        #endregion

        #region initialization
        /// <summary>
        /// Register Orpheus services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddOrpheusServices(this IServiceCollection services)
        {
            ServiceManager.initializeServices(services);
            return services;
        }
        #endregion

        #region service resolution
        /// <summary>
        /// This needs to be provided as not all classes within Orpheus can have constructor dependency injection.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Resolve an interface to a concrete implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            T result;
            try
            {
                result = ServiceProvider.GetService<T>();
            }
            catch
            {
                throw;
            }
            return result;
        }

        /// <summary>
        /// Resolve an interface to a concrete implementation, with constructor parameter support.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="constructorParameters"></param>
        /// <returns></returns>
        public static object Resolve(Type serviceType, object[] constructorParameters)
        {
            try
            {
                List<Type> parametersType = new List<Type>();
                List<object> parameterValues = new List<object>();
                foreach (var obj in constructorParameters)
                {
                    if (obj != null)
                    {
                        parametersType.Add(obj.GetType());
                        parameterValues.Add(obj);
                    }
                }

                if (assemblies == null)
                    assemblies = AppDomain.CurrentDomain.GetAssemblies();

                //var concreteType = assemblies.Where(x => x.FullName.Contains("Orpheus")).SelectMany(x => x.GetTypes())
                //                .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                //                .ToList().FirstOrDefault();

                var concreteType = assemblies.Where(asm => asm.IsDynamic == false).SelectMany(x => x.GetExportedTypes())
                                .Where(x => serviceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                                .ToList().FirstOrDefault();

                if (concreteType != null)
                {
                    //try to find a matching constructor based on the constructor parameters.
                    //if a constructor is found, then instantiate the class.
                    ConstructorInfo[] constructors = concreteType.GetConstructors();
                    ConstructorInfo constructorInfo = concreteType.GetConstructor(parametersType.ToArray());
                    if (constructorInfo != null)
                    {
                        return constructorInfo.Invoke(parameterValues.ToArray());
                    }
                }
            }
            catch
            {
                throw;
            }
            return null;
        }

        /// <summary>
        /// Resolve an interface to a concrete implementation, with constructor parameter support.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>(object[] constructorParameters)
        {
            try
            {
                List<Type> parametersType = new List<Type>();
                List<object> parameterValues = new List<object>();
                foreach (var obj in constructorParameters)
                {
                    if (obj != null)
                    {
                        parametersType.Add(obj.GetType());
                        parameterValues.Add(obj);
                    }
                }

                if (assemblies == null)
                    assemblies = AppDomain.CurrentDomain.GetAssemblies();

                //var concreteType = assemblies.Where(x => x.FullName.Contains("Orpheus")).SelectMany(x => x.GetTypes())
                //                .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                //                .ToList().FirstOrDefault();

                var concreteType = assemblies.Where(asm => asm.IsDynamic == false).SelectMany(x => x.GetExportedTypes())
                                .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                                .ToList().FirstOrDefault();

                if (concreteType != null)
                {
                    //try to find a matching constructor based on the constructor parameters.
                    //if a constructor is found, then instantiate the class.
                    ConstructorInfo[] constructors = concreteType.GetConstructors();
                    ConstructorInfo constructorInfo = concreteType.GetConstructor(parametersType.ToArray());
                    if (constructorInfo != null)
                    {
                        return (T)constructorInfo.Invoke(parameterValues.ToArray());
                    }
                }
            }
            catch
            {
                throw;
            }
            return default(T);
        }

        /// <summary>
        /// Gets the logger factory.
        /// </summary>
        /// <value>
        /// The logger factory.
        /// </value>
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (loggerFactory == null)
                {
                    loggerFactory = Resolve<ILoggerFactory>();
                }
                return loggerFactory;
            }
        }
        /// <summary>
        /// Creates a logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger<T> CreateLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
        /// <summary>
        /// Creates a logger.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetLoggerService(Type type)
        {
            return ServiceProvider.GetService(type);
        }
        #endregion
    }
}
