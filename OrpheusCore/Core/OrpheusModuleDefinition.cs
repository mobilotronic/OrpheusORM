using OrpheusAttributes;
using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace OrpheusCore
{
    internal class ModuleDefinitionModel
    {
        [PrimaryKey]
        public string MODULE_NAME { get; set; }
        public string MODULE_DEFINITION { get; set; }
    }
    /// <summary>
    /// The OrpheusModuleDefinition can save and load a module definition. A module definition, includes relationship between the module tables
    /// and that is required for a module to be functional. <see cref="IOrpheusModule"/>
    /// </summary>
    public class OrpheusModuleDefinition : IOrpheusModuleDefinition
    {

        private DataContractSerializer serializer;

        private void prepareSerializer()
        {
            if(this.serializer == null)
            {
                List<Type> extraTypes = new List<Type>();

                extraTypes.Add(this.Database.CreateTableOptions().GetType());
                extraTypes.Add(this.Database.CreateTableKeyField().GetType());
                this.serializer = new DataContractSerializer(this.GetType(), extraTypes.ToArray());
            }
        }

        /// <summary>
        /// Orpheus database.
        /// </summary>
        [IgnoreDataMember]
        public IOrpheusDatabase Database { get; set; }

        /// <summary>
        /// Module's main table options.
        /// </summary>
        public IOrpheusTableOptions MainTableOptions { get; set; }

        /// <summary>
        /// List of module's detail table options.
        /// </summary>
        public List<IOrpheusTableOptions> DetailTableOptions { get; set; }

        /// <summary>
        /// List of module reference tables.
        /// </summary>
        public List<IOrpheusTableOptions> ReferenceTableOptions { get; set; }

        /// <summary>
        /// Module's name.
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Creates an instance of OrpheusTableOptions.
        /// </summary>
        /// <returns></returns>
        public IOrpheusTableOptions CreateTableOptions()
        {
            return this.Database.CreateTableOptions();
        }

        /// <summary>
        /// Creates an instance of OrpheusTableOptions.
        /// </summary>
        /// <returns></returns>
        public IOrpheusTableOptions CreateTableOptions(string tableName, Type modelType)
        {
            var result = this.CreateTableOptions();
            result.TableName = tableName;
            result.ModelType = modelType;
            return result;
        }

        /// <summary>
        /// Creates an instance of OrpheusTableOptions.
        /// </summary>
        /// <returns></returns>
        public IOrpheusTableOptions CreateTableOptions(Type modelType)
        {
            return this.CreateTableOptions(modelType.Name, modelType);
        }

        /// <summary>
        /// Load definition from memory.
        /// </summary>
        /// <param name="stream"></param>
        public void LoadFrom(Stream stream)
        {
            this.prepareSerializer();
            IOrpheusModuleDefinition newDef =  (IOrpheusModuleDefinition)this.serializer.ReadObject(stream);
            this.MainTableOptions = newDef.MainTableOptions;
            this.DetailTableOptions = newDef.DetailTableOptions;
            this.ReferenceTableOptions = newDef.ReferenceTableOptions;
            this.Name = newDef.Name;
        }

        /// <summary>
        /// Load definition from the database.
        /// </summary>
        public void LoadFromDB(string moduleName)
        {
            if (this.Database != null)
            {
                var table = this.Database.CreateTable<ModuleDefinitionModel>("ORPH_MODULES");
                table.Load(new List<object>() { moduleName });
                if(table.Data.Count > 0)
                {
                    var moduleDef = table.Data.First();
                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(moduleDef.MODULE_DEFINITION));
                    try
                    {
                        this.LoadFrom(ms);
                    }
                    finally
                    {
                        ms.Close();
                    }
                }
            }
        }
        /// <summary>
        /// Load definition from a file.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadFrom(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                try
                {
                    this.LoadFrom(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// Save definition to a file.
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveTo(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            try
            {
                this.SaveTo(fs);
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// Save definition to memory.
        /// </summary>
        /// <param name="stream"></param>
        public void SaveTo(Stream stream)
        {
            this.prepareSerializer();
            this.serializer.WriteObject(stream, this);
        }

        /// <summary>
        /// Save definition to the database.
        /// </summary>
        public void SaveToDB()
        {
            if (this.Database != null)
            {
                MemoryStream ms = new MemoryStream();
                try
                {
                    this.SaveTo(ms);
                    var newDefinition = Encoding.UTF8.GetString(ms.ToArray());
                    var table = this.Database.CreateTable<ModuleDefinitionModel>("ORPH_MODULES");
                    table.Load(new List<object>() { this.Name });
                    var existingDefinition = table.Data.Where(m => m.MODULE_NAME.ToLower() == this.Name.ToLower()).FirstOrDefault();
                    if(existingDefinition == null)
                    {
                        table.Add(new ModuleDefinitionModel()
                        {
                            MODULE_NAME = this.Name,
                            MODULE_DEFINITION = newDefinition
                        });
                    }
                    else
                    {
                        existingDefinition.MODULE_DEFINITION = newDefinition;
                        table.Update(existingDefinition);
                    }

                    using (var tr = this.Database.BeginTransaction())
                    {
                        try
                        {
                            if (existingDefinition == null)
                                table.ExecuteInserts(tr);
                            else
                                table.ExecuteUpdates(tr);
                            this.Database.CommitTransaction(tr);
                        }
                        catch
                        {
                            this.Database.RollbackTransaction(tr);
                            throw;
                        }
                    }
                }
                finally
                {
                    ms.Close();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OrpheusModuleDefinition()
        {
            this.DetailTableOptions = new List<IOrpheusTableOptions>();
            this.ReferenceTableOptions = new List<IOrpheusTableOptions>();
        }
    }
}
