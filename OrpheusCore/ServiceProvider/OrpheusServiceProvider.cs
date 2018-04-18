using Microsoft.Extensions.DependencyInjection;
using OrpheusCore.Configuration;
using OrpheusCore.SchemaBuilder;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OrpheusCore.ServiceProvider
{
    /// <summary>
    /// Orpheus DI service provider.
    /// </summary>
    public class OrpheusServiceProvider
    {
        private static IServiceProvider serviceProvider;
        private static void initializeInternalOrpheusServices(IServiceCollection serviceCollection)
        {

            //data services.
            serviceCollection.AddTransient<IOrpheusTableOptions, OrpheusTableOptions>();
            serviceCollection.AddTransient<IOrpheusModuleDefinition, OrpheusModuleDefinition>();
            serviceCollection.AddTransient<IOrpheusTableKeyField, OrpheusTableKeyField>();
            serviceCollection.AddTransient<IOrpheusModule, OrpheusModule>();

            //Schema services.
            serviceCollection.AddTransient<ISchema, SchemaBuilder.Schema>();
            serviceCollection.AddTransient<ISchemaView, SchemaObjectView>();
            serviceCollection.AddTransient<ISchemaViewTable, SchemaObjectViewTable>();
            serviceCollection.AddTransient<ISchemaTable, SchemaObjectTable>();
            serviceCollection.AddTransient<ISchemaObject, SchemaObject>();
            serviceCollection.AddTransient<ISchemaJoinDefinition, SchemaJoinDefinition>();
            serviceCollection.AddTransient<ISchemaDataObject, SchemaDataObject>();

            serviceCollection.AddOptions();
            serviceCollection.Configure<LoggingConfiguration>(ConfigurationManager.ConfigurationInstance.GetSection("Logging"));
            serviceCollection.Configure<List<ServiceProviderItem>>(ConfigurationManager.ConfigurationInstance.GetSection("Services"));
            serviceCollection.Configure<OrpheusConfiguration>(ConfigurationManager.ConfigurationInstance);
        }

        /// <summary>
        /// Initializes services DI.
        /// </summary>
        public static void InitializeServiceProvider()
        {
            if (OrpheusServiceProvider.serviceProvider == null)
            {
                var sc = new ServiceCollection();
                initializeInternalOrpheusServices(sc);
                ServiceProvider.OrpheusServiceProvider.InitializeServiceCollection(sc);
                OrpheusServiceProvider.serviceProvider = sc.BuildServiceProvider();
            }
        }

        /// <summary>
        ///  Defines a mechanism for retrieving a service object; that is, an object that
        ///  provides custom support to other objects.
        /// </summary>
        public static IServiceProvider Provider
        {
            get
            {
                OrpheusServiceProvider.InitializeServiceProvider();
                return OrpheusServiceProvider.serviceProvider;
            }
        }

        /// <summary>
        /// Initialize the service collection.
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void InitializeServiceCollection(IServiceCollection serviceCollection)
        {
            foreach (var scItem in ConfigurationManager.Configuration.Services)
            {
                var serviceType = Type.GetType(scItem.Service);
                var implementationType = Type.GetType(scItem.Implementation);
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
                result = OrpheusServiceProvider.Provider.GetService<T>();
            }
            catch(Exception e)
            {
                throw e;
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
                foreach(var obj in constructorParameters)
                {
                    if (obj != null)
                    {
                        parametersType.Add(obj.GetType());
                        parameterValues.Add(obj);
                    }
                }

                T service = Resolve<T>();

                //try to find a matching constructor based on the constructor parameters.
                //if a constructor is found, then instantiate the class.
                ConstructorInfo[] constructors = service.GetType().GetConstructors();
                ConstructorInfo constructorInfo = service.GetType().GetConstructor(parametersType.ToArray());
                if(constructorInfo != null)
                {
                    return (T)constructorInfo.Invoke(parameterValues.ToArray());
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return default(T);
        }
    }
}
