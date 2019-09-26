using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OrpheusCore.Configuration.Models
{
    /// <summary>
    /// Binder options with dependency injection type resolution support.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Configuration.BinderOptions" />
    public class DIBinderOptions : BinderOptions
    {
        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <value>
        /// The service provider.
        /// </value>
        public IServiceProvider ServiceProvider { get; set; }
    }

    /// <summary>
    /// Configuration extensions, with DI support.
    /// </summary>
    public static class TardigradeConfigurationBinder
    {
        #region private
        private static void BindNonScalar(this IConfiguration configuration, object instance, DIBinderOptions options)
        {
            if (instance != null)
            {
                foreach (var property in GetAllProperties(instance.GetType().GetTypeInfo()))
                {
                    BindProperty(property, instance, configuration, options);
                }
            }
        }

        private static void BindProperty(PropertyInfo property, object instance, IConfiguration config, DIBinderOptions options)
        {
            // We don't support set only, non public, or indexer properties
            if (property.GetMethod == null ||
                (!options.BindNonPublicProperties && !property.GetMethod.IsPublic) ||
                property.GetMethod.GetParameters().Length > 0)
            {
                return;
            }

            var propertyValue = property.GetValue(instance);
            var hasSetter = property.SetMethod != null && (property.SetMethod.IsPublic || options.BindNonPublicProperties);

            if (propertyValue == null && !hasSetter)
            {
                // Property doesn't have a value and we cannot set it so there is no
                // point in going further down the graph
                return;
            }

            propertyValue = BindInstance(property.PropertyType, propertyValue, config.GetSection(property.Name), options);

            if (propertyValue != null && hasSetter)
            {
                property.SetValue(instance, propertyValue);
            }
        }

        private static object BindToCollection(TypeInfo typeInfo, IConfiguration config, DIBinderOptions options)
        {
            var type = typeof(List<>).MakeGenericType(typeInfo.GenericTypeArguments[0]);
            var instance = Activator.CreateInstance(type);
            BindCollection(instance, type, config, options);
            return instance;
        }

        // Try to create an array/dictionary instance to back various collection interfaces
        private static object AttemptBindToCollectionInterfaces(Type type, IConfiguration config, DIBinderOptions options)
        {
            var typeInfo = type.GetTypeInfo();

            if (!typeInfo.IsInterface)
            {
                return null;
            }

            var collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyList<>), type);
            if (collectionInterface != null)
            {
                // IEnumerable<T> is guaranteed to have exactly one parameter
                return BindToCollection(typeInfo, config, options);
            }

            collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyDictionary<,>), type);
            if (collectionInterface != null)
            {
                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0], typeInfo.GenericTypeArguments[1]);
                var instance = Activator.CreateInstance(dictionaryType);
                BindDictionary(instance, dictionaryType, config, options);
                return instance;
            }

            collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
            if (collectionInterface != null)
            {
                var instance = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0], typeInfo.GenericTypeArguments[1]));
                BindDictionary(instance, collectionInterface, config, options);
                return instance;
            }

            collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyCollection<>), type);
            if (collectionInterface != null)
            {
                // IReadOnlyCollection<T> is guaranteed to have exactly one parameter
                return BindToCollection(typeInfo, config, options);
            }

            collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
            if (collectionInterface != null)
            {
                // ICollection<T> is guaranteed to have exactly one parameter
                return BindToCollection(typeInfo, config, options);
            }

            collectionInterface = FindOpenGenericInterface(typeof(IEnumerable<>), type);
            if (collectionInterface != null)
            {
                // IEnumerable<T> is guaranteed to have exactly one parameter
                return BindToCollection(typeInfo, config, options);
            }

            return null;
        }

        private static object BindInstance(Type type, object instance, IConfiguration config, DIBinderOptions options)
        {
            // if binding IConfigurationSection, break early
            if (type == typeof(IConfigurationSection))
            {
                return config;
            }

            var section = config as IConfigurationSection;
            var configValue = section?.Value;
            object convertedValue;
            Exception error;
            if (configValue != null && TryConvertValue(type, configValue, out convertedValue, out error))
            {
                if (error != null)
                {
                    throw error;
                }

                // Leaf nodes are always reinitialized
                return convertedValue;
            }

            if (config != null && config.GetChildren().Any())
            {
                // If we don't have an instance, try to create one
                if (instance == null)
                {
                    // We are alrady done if binding to a new collection instance worked
                    instance = AttemptBindToCollectionInterfaces(type, config, options);
                    if (instance != null)
                    {
                        return instance;
                    }

                    instance = CreateInstance(type, options);
                }

                // See if its a Dictionary
                var collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
                if (collectionInterface != null)
                {
                    BindDictionary(instance, collectionInterface, config, options);
                }
                else if (type.IsArray)
                {
                    instance = BindArray((Array)instance, config, options);
                }
                else
                {
                    // See if its an ICollection
                    collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
                    if (collectionInterface != null)
                    {
                        BindCollection(instance, collectionInterface, config, options);
                    }
                    // Something else
                    else
                    {
                        BindNonScalar(config, instance, options);
                    }
                }
            }

            return instance;
        }

        private static object CreateInstance(Type type, DIBinderOptions options)
        {
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsInterface || typeInfo.IsAbstract)
            {
                if (options != null && options.ServiceProvider != null)
                {
                    return options.ServiceProvider.GetService(type);
                }

                throw new InvalidOperationException("Cannot active abstract or interface");
            }

            if (type.IsArray)
            {
                if (typeInfo.GetArrayRank() > 1)
                {
                    throw new InvalidOperationException("Unsupported multi-dimensional array");
                }

                return Array.CreateInstance(typeInfo.GetElementType(), 0);
            }

            var hasDefaultConstructor = typeInfo.DeclaredConstructors.Any(ctor => ctor.IsPublic && ctor.GetParameters().Length == 0);
            if (!hasDefaultConstructor)
            {
                throw new InvalidOperationException("No parameterless constructor");
            }

            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to activate type{type.Name}", ex);
            }
        }

        private static void BindDictionary(object dictionary, Type dictionaryType, IConfiguration config, DIBinderOptions options)
        {
            var typeInfo = dictionaryType.GetTypeInfo();

            // IDictionary<K,V> is guaranteed to have exactly two parameters
            var keyType = typeInfo.GenericTypeArguments[0];
            var valueType = typeInfo.GenericTypeArguments[1];
            var keyTypeIsEnum = keyType.GetTypeInfo().IsEnum;

            if (keyType != typeof(string) && !keyTypeIsEnum)
            {
                // We only support string and enum keys
                return;
            }

            var setter = typeInfo.GetDeclaredProperty("Item");
            foreach (var child in config.GetChildren())
            {
                var item = BindInstance(
                    type: valueType,
                    instance: null,
                    config: child,
                    options: options);
                if (item != null)
                {
                    if (keyType == typeof(string))
                    {
                        var key = child.Key;
                        setter.SetValue(dictionary, item, new object[] { key });
                    }
                    else if (keyTypeIsEnum)
                    {
                        var key = Convert.ToInt32(Enum.Parse(keyType, child.Key));
                        setter.SetValue(dictionary, item, new object[] { key });
                    }
                }
            }
        }

        private static void BindCollection(object collection, Type collectionType, IConfiguration config, DIBinderOptions options)
        {
            var typeInfo = collectionType.GetTypeInfo();

            // ICollection<T> is guaranteed to have exactly one parameter
            var itemType = typeInfo.GenericTypeArguments[0];
            var addMethod = typeInfo.GetDeclaredMethod("Add");

            foreach (var section in config.GetChildren())
            {
                try
                {
                    var item = BindInstance(
                        type: itemType,
                        instance: null,
                        config: section,
                        options: options);
                    if (item != null)
                    {
                        addMethod.Invoke(collection, new[] { item });
                    }
                }
                catch
                {
                }
            }
        }

        private static Array BindArray(Array source, IConfiguration config, DIBinderOptions options)
        {
            var children = config.GetChildren().ToArray();
            var arrayLength = source.Length;
            var elementType = source.GetType().GetElementType();
            var newArray = Array.CreateInstance(elementType, arrayLength + children.Length);

            // binding to array has to preserve already initialized arrays with values
            if (arrayLength > 0)
            {
                Array.Copy(source, newArray, arrayLength);
            }

            for (int i = 0; i < children.Length; i++)
            {
                try
                {
                    var item = BindInstance(
                        type: elementType,
                        instance: null,
                        config: children[i],
                        options: options);
                    if (item != null)
                    {
                        newArray.SetValue(item, arrayLength + i);
                    }
                }
                catch
                {
                }
            }

            return newArray;
        }

        private static bool TryConvertValue(Type type, string value, out object result, out Exception error)
        {
            error = null;
            result = null;
            if (type == typeof(object))
            {
                result = value;
                return true;
            }

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return true;
                }
                return TryConvertValue(Nullable.GetUnderlyingType(type), value, out result, out error);
            }

            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    result = converter.ConvertFromInvariantString(value);
                }
                catch (Exception ex)
                {
                    //error = new InvalidOperationException(Resources.FormatError_FailedBinding(value, type), ex);
                    error = new InvalidOperationException("Failed binding", ex);
                }
                return true;
            }

            return false;
        }

        private static object ConvertValue(Type type, string value)
        {
            object result;
            Exception error;
            TryConvertValue(type, value, out result, out error);
            if (error != null)
            {
                throw error;
            }
            return result;
        }

        private static Type FindOpenGenericInterface(Type expected, Type actual)
        {
            var actualTypeInfo = actual.GetTypeInfo();
            if (actualTypeInfo.IsGenericType &&
                actual.GetGenericTypeDefinition() == expected)
            {
                return actual;
            }

            var interfaces = actualTypeInfo.ImplementedInterfaces;
            foreach (var interfaceType in interfaces)
            {
                if (interfaceType.GetTypeInfo().IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == expected)
                {
                    return interfaceType;
                }
            }
            return null;
        }

        private static IEnumerable<PropertyInfo> GetAllProperties(TypeInfo type)
        {
            var allProperties = new List<PropertyInfo>();

            do
            {
                allProperties.AddRange(type.DeclaredProperties);
                type = type.BaseType.GetTypeInfo();
            }
            while (type != typeof(object).GetTypeInfo());

            return allProperties;
        }
        #endregion

        #region extension methods
        /// <summary>
        /// Attempts to bind the given object instance to configuration values by matching property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="instance">The object to bind.</param>
        /// <param name="configureOptions">Configures the binder options.</param>
        public static void BindDI(this IConfiguration configuration, object instance, Action<DIBinderOptions> configureOptions)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (instance != null)
            {
                var options = new DIBinderOptions();
                configureOptions?.Invoke(options);
                BindInstance(instance.GetType(), instance, configuration, options);
            }
        }
        #endregion
    }

}
