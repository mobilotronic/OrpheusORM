using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace OrpheusCore.Configuration.Models
{
    /// <summary>
    /// Service DI configuration item.
    /// </summary>
    public class ServiceProviderItem
    {
        /// <summary>
        /// Gets or sets the implementation.
        /// </summary>
        /// <value>
        /// The implementation.
        /// </value>
        public string Implementation { get; set; }

        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the service lifetime.
        /// </summary>
        /// <value>
        /// The service lifetime.
        /// </value>
        [JsonConverter(typeof(StringEnumConverter))]
        public ServiceLifetime ServiceLifetime { get; set; }

        /// <summary>
        /// Gets or sets the constructor parameters.
        /// </summary>
        /// <value>
        /// The constructor parameters.
        /// </value>
        /// <remarks>Obsolete</remarks>
        public List<string> ConstructorParameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderItem"/> class.
        /// </summary>
        public ServiceProviderItem()
        {
            this.ServiceLifetime = ServiceLifetime.Transient;
        }
    }
}
