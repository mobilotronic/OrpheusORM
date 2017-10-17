using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace OrpheusCore.Configuration
{
    [Serializable]
    public class ServiceProviderItem
    {
        [XmlElement("Implementation")]
        public string Implementation { get; set; }

        [XmlElement("Service")]
        public string Service { get; set; }
        
        [XmlElement("ServiceLifeTime")]
        public ServiceLifetime ServiceLifetime { get; set; }

        [XmlArray("ConstructorParameters")]
        public List<string> ConstructorParameters { get; set; }

        public ServiceProviderItem()
        {
            this.ServiceLifetime = ServiceLifetime.Transient;
        }
    }

    [Serializable]
    public class LoggingConfiguration
    {
        [XmlAttribute("Level")]
        public string Level { get; set; }

        [XmlAttribute("FilePath")]
        public string FilePath { get; set; }

        [XmlAttribute("MaxFileSize")]
        public int MaxFileSize { get; set; }
    }

    [Serializable]
    public class OrpheusConfiguration
    {
        [XmlArray("Services")]
        public List<ServiceProviderItem> Services { get; set; }

        [XmlElement("Logging")]
        public LoggingConfiguration Logging { get; set; }

        [XmlElement("DefaultStringSize")]
        public int DefaultStringSize { get; set; }

        public OrpheusConfiguration()
        {
            this.DefaultStringSize = 60;
        }
    }
}
