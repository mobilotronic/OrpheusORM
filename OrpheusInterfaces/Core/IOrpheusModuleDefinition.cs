using System;
using System.Collections.Generic;
using System.IO;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// The definition/database properties of a module.
    /// </summary>
    public interface IOrpheusModuleDefinition
    {
        /// <summary>
        /// Module name.
        /// </summary>
        /// <returns>Module name</returns>
        string Name { get; set; }

        /// <summary>
        /// Orpheus database.
        /// </summary>
        /// <returns>Orpheus Database</returns>
        IOrpheusDatabase Database { get; set; }

        /// <summary>
        /// Module's main table options.
        /// </summary>
        /// <returns>Module's main table options</returns>
        IOrpheusTableOptions MainTableOptions { get; set; }

        /// <summary>
        /// List of module's detail table options.
        /// </summary>
        /// <returns>Module's detail table options</returns>
        List<IOrpheusTableOptions> DetailTableOptions { get; set; }

        /// <summary>
        /// List of module reference tables.
        /// </summary>
        /// <returns>Module's reference table options</returns>
        List<IOrpheusTableOptions> ReferenceTableOptions { get; set; }


        /// <summary>
        /// Creates an instance of OrpheusTableOptions.
        /// </summary>
        /// <returns>An IOrpheusTableOptions instance</returns>
        IOrpheusTableOptions CreateTableOptions();

        /// <summary>
        /// Creates an instance of OrpheusTableOptions.
        /// </summary>
        /// <returns>An IOrpheusTableOptions instance</returns>
        IOrpheusTableOptions CreateTableOptions(string tableName,Type modelType);

        /// <summary>
        /// Creates an instance of OrpheusTableOptions.
        /// </summary>
        /// <returns>An IOrpheusTableOptions instance</returns>
        IOrpheusTableOptions CreateTableOptions(Type modelType);

        /// <summary>
        /// Saves definition to a stream.
        /// </summary>
        /// <param name="stream"></param>
        void SaveTo(Stream stream);
        
        /// <summary>
        /// Saves definition to a file.
        /// </summary>
        /// <param name="fileName"></param>
        void SaveTo(String fileName);
        
        /// <summary>
        /// Saves the definition the connected database.
        /// </summary>
        void SaveToDB();
        
        /// <summary>
        /// Load definition from stream.
        /// </summary>
        /// <param name="stream"></param>
        void LoadFrom(Stream stream);
       
        /// <summary>
        /// Loads definition from a file.
        /// </summary>
        /// <param name="fileName"></param>
        void LoadFrom(String fileName);
        
        /// <summary>
        /// Loads definition from the connected database.
        /// </summary>
        void LoadFromDB(string moduleName);

    }
}
