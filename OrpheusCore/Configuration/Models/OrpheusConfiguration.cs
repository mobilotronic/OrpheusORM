using System.Collections.Generic;

namespace OrpheusCore.Configuration.Models
{

    /// <summary>
    /// Orpheus's configuration.
    /// </summary>
    public class OrpheusConfiguration
    {
        #region public properties
        /// <summary>
        /// Gets or sets the database connections.
        /// </summary>
        /// <value>
        /// The database connections.
        /// </value>
        public List<DatabaseConnectionConfiguration> DatabaseConnections { get; set; }

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
        public OrpheusConfiguration()
        {
            this.DefaultStringSize = 60;
        }
        #endregion
    }
}
