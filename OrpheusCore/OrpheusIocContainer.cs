using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;

namespace OrpheusCore
{
    public class OrpheusIocContainer
    {
        private static IUnityContainer container;

        public static IUnityContainer Container
        {
            get
            {
                if(OrpheusIocContainer.container == null)
                {
                    var unitySection = (UnityConfigurationSection)Orpheus.Configuration.GetSection("unity");
                    OrpheusIocContainer.container = new UnityContainer();
                    OrpheusIocContainer.container.LoadConfiguration(unitySection);
                }
                return OrpheusIocContainer.container;
            }
        }

        public static T Resolve<T>()
        {
            T result;
            try
            {
                result = OrpheusIocContainer.Container.Resolve<T>();
            }
            catch(Exception e)
            {
                throw e;
            }
            return result;
        }

        public static T Resolve<T>(ResolverOverride[] overrides)
        {
            T result;
            try
            {
                result = OrpheusIocContainer.Container.Resolve<T>(overrides);
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }

        public static T Resolve<T>(string name, ResolverOverride[] overrides)
        {
            T result;
            try
            {
                result = OrpheusIocContainer.Container.Resolve<T>(name, overrides);
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }
    }
}
