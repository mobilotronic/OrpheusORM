using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace OrpheusCore.Configuration
{
    /// <summary>
    /// Service DI configuration item.
    /// </summary>
    [Serializable]
    public class ServiceProviderItem
    {
        /// <summary>
        /// Fully qualified name for the service implementation.
        /// </summary>
        [XmlElement("Implementation")]
        public string Implementation { get; set; }

        /// <summary>
        /// Fully qualified name for the service interface.
        /// </summary>
        [XmlElement("Service")]
        public string Service { get; set; }
        
        /// <summary>
        /// Service life time.
        /// </summary>
        [XmlElement("ServiceLifeTime")]
        public ServiceLifetime ServiceLifetime { get; set; }

        /// <summary>
        /// Obsolete, to be removed.
        /// </summary>
        [XmlArray("ConstructorParameters")]
        public List<string> ConstructorParameters { get; set; }

        /// <summary>
        /// Creates a ServiceProviderItem.
        /// </summary>
        public ServiceProviderItem()
        {
            this.ServiceLifetime = ServiceLifetime.Transient;
        }
    }

    /// <summary>
    /// Orpheus logging configuration.
    /// </summary>
    [Serializable]
    public class LoggingConfiguration
    {
        /// <summary>
        /// Logging level.
        /// </summary>
        [XmlAttribute("Level")]
        public string Level { get; set; }

        /// <summary>
        /// Log file path.
        /// </summary>
        [XmlAttribute("FilePath")]
        public string FilePath { get; set; }

        /// <summary>
        /// Maximum log file size.
        /// </summary>
        [XmlAttribute("MaxFileSize")]
        public int MaxFileSize { get; set; }
    }

    /// <summary>
    /// Orpheus's configuration.
    /// </summary>
    [Serializable]
    public class OrpheusConfiguration
    {
        /// <summary>
        /// List of services.
        /// </summary>
        [XmlArray("Services")]
        public List<ServiceProviderItem> Services { get; set; }

        /// <summary>
        /// Logging configuration.
        /// </summary>
        [XmlElement("Logging")]
        public LoggingConfiguration Logging { get; set; }

        /// <summary>
        /// Default size for string field, when creating a db schema.
        /// </summary>
        [XmlElement("DefaultStringSize")]
        public int DefaultStringSize { get; set; }

        /// <summary>
        /// Creates an Orpheus configuration.
        /// </summary>
        public OrpheusConfiguration()
        {
            this.DefaultStringSize = 60;
        }
    }
}
