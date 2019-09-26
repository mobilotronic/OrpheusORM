using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using OrpheusInterfaces.Configuration;
using System;
using System.Collections.Generic;

namespace OrpheusCore.Configuration.Models
{

    /// <summary>
    /// Orpheus's configuration.
    /// </summary>
    public class OrpheusConfiguration
    {
        #region private
        private IConfiguration configuration;
        private IServiceProvider serviceProvider;
        private Action onConfigurationReload;
        #endregion

        #region private methods
        private void registerReloadToken()
        {
            ChangeToken.OnChange(() => this.configuration.GetReloadToken(), this.onConfigurationReload);
        }

        private void initialize()
        {
            this.onConfigurationReload = () => { this.reload(); };
            this.registerReloadToken();
            configuration.BindDI(this, (options) => { options.ServiceProvider = serviceProvider; });
            if (this.DatabaseConnections == null)
                this.DatabaseConnections = new List<IDatabaseConnectionConfiguration>();
            if (this.Services == null)
                this.Services = new List<ServiceProviderItem>();
        }

        /// <summary>
        /// Clears this instance settings.
        /// </summary>
        private void clear()
        {
            if (this.DatabaseConnections != null)
                this.DatabaseConnections.Clear();
            if (this.Services != null)
                this.Services.Clear();
        }

        /// <summary>
        /// Reloads the application settings.
        /// </summary>
        private void reload()
        {
            this.clear();
            this.configuration.BindDI(this, (options) => { options.ServiceProvider = this.serviceProvider; });
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public List<ServiceProviderItem> Services { get; set; }

        /// <summary>
        /// Gets or sets the database connections.
        /// </summary>
        /// <value>
        /// The database connections.
        /// </value>
        public List<IDatabaseConnectionConfiguration> DatabaseConnections { get; set; }

        /// <summary>
        /// Gets or sets the default size of the string.
        /// </summary>
        /// <value>
        /// The default size of a string field, when creating the db schema.
        /// </value>
        public int DefaultStringSize { get; set; }
        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="OrpheusConfiguration"/> class.
        /// </summary>
        public OrpheusConfiguration(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this.DefaultStringSize = 60;
            this.configuration = configuration;
            this.serviceProvider = serviceProvider;
            this.initialize();
        }
        #endregion
    }
}
