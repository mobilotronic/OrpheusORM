using System;
using Microsoft.Extensions.DependencyInjection;
using OrpheusCore.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OrpheusCore.ServiceProvider
{
    /// <summary>
    /// Orpheus DI service provider.
    /// </summary>
    public class OrpheusServiceProvider
    {
        private static IServiceProvider serviceProvider;

        /// <summary>
        ///  Defines a mechanism for retrieving a service object; that is, an object that
        ///  provides custom support to other objects.
        /// </summary>
        public static IServiceProvider Provider
        {
            get
            {
                if(OrpheusServiceProvider.serviceProvider == null)
                {
                    var sc = new ServiceCollection();
                    ServiceProvider.OrpheusServiceProvider.InitializeServiceCollection(sc);
                    OrpheusServiceProvider.serviceProvider = sc.BuildServiceProvider();
                }
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
    }
}
