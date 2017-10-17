using System;
using Microsoft.Extensions.DependencyInjection;
using OrpheusCore.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OrpheusCore.ServiceProvider
{
    public class OrpheusServiceProvider
    {
        private static IServiceProvider serviceProvider;

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
