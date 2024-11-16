using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrpheusCore.Configuration.Models;
using OrpheusCore.SchemaBuilder;
using OrpheusInterfaces.Configuration;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Orpheus configuration manager.
    /// </summary>
    public static class ConfigurationManager
    {
        private static IConfiguration configurationInstance;
        private static OrpheusConfiguration configuration;
        private static IServiceProvider serviceProvider;
        private static Assembly[] assemblies;
        private static ILoggerFactory loggerFactory;

        #region private methods
        private static void initializeConfigurableServices(IServiceCollection serviceCollection)
        {
            try
            {
                var dynamicServices = configurationInstance.GetSection("Services").Get<List<ServiceProviderItem>>();
                if(dynamicServices != null)
                {
                    foreach (var scItem in dynamicServices)
                    {
                        var serviceType = Type.GetType(scItem.Service);
                        var implementationType = Type.GetType(scItem.Implementation);
                        //if either of the service or implementation cannot be resolved, throw an error.
                        if (serviceType == null || implementationType == null)
                        {
                            throw new Exception($"Service {scItem.Service} or {scItem.Implementation} could not be resolved.");
                        }
                        switch (scItem.ServiceLifetime)
                        {
                            case ServiceLifetime.Transient:
                                {
                                    serviceCollection.AddTransient(serviceType, implementationType);
                                    break;
                                }

                            case ServiceLifetime.Singleton:
                                {
                                    serviceCollection.AddSingleton(serviceType, implementationType);
                                    break;
                                }
                            case ServiceLifetime.Scoped:
                                {
                                    serviceCollection.AddScoped(serviceType, implementationType);
                                    break;
                                }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

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
            initializeConfigurableServices(serviceCollection);
            //if there is no service provider registered, register at least one, so Orpheus can work.
            var isLoggingRegistered = serviceCollection.Where((sd => sd.ServiceType == typeof(ILoggerFactory))).FirstOrDefault();
            if (isLoggingRegistered == null)
            {
                serviceCollection.AddLogging((builder) => {
                    builder.ClearProviders();
                    builder.AddConsole();
                });
            }
        }
        #endregion

        /// <summary>
        /// Initializes the configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="services">The services.</param>
        public static void InitializeConfiguration(IConfiguration configuration, IServiceCollection services = null)
        {
            ConfigurationManager.configurationInstance = configuration;
            //if no service collection is passed, we are in self service mode.
            //meaning all services are registered to the Orpheus internal service collection.
            if (services == null)
            {
                services = new ServiceCollection();
            }
            ConfigurationManager.initializeServices(services);
            ConfigurationManager.serviceProvider = services.BuildServiceProvider();
            ConfigurationManager.configuration = new OrpheusConfiguration(configuration, serviceProvider);
        }

        /// <summary>
        /// Initializes the configuration.
        /// </summary>
        /// <param name="configurationFile">The configuration file.</param>
        public static void InitializeConfiguration(string configurationFile)
        {
            try
            {
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.SetBasePath(Path.GetDirectoryName(configurationFile));
                configurationBuilder.AddJsonFile(configurationFile, optional: false, reloadOnChange: true);
                InitializeConfiguration(configurationBuilder.Build());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <param name="configurationFile">The configuration file.</param>
        public static void SaveConfiguration(string configurationFile)
        {
            File.WriteAllText(configurationFile,JsonConvert.SerializeObject(configuration));
        }

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
                result = serviceProvider.GetService<T>();
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

                var concreteType = assemblies.Where(asm=>asm.IsDynamic == false).SelectMany(x => x.GetExportedTypes())
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
        /// Gets the current configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public static OrpheusConfiguration Configuration => configuration;

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
                if(loggerFactory == null)
                {
                    loggerFactory = Resolve<ILoggerFactory>();
                }
                return loggerFactory;
            }
        }
    }
}
