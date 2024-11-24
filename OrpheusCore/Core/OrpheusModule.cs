using Microsoft.Extensions.Logging;
using OrpheusCore.Errors;
using OrpheusInterfaces.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OrpheusCore
{
    /// <summary>
    /// OrpheusModule class represents a logical division and grouping of a set of tables.
    /// For example you can an OrdersModule, which will be comprised from many different tables.
    /// Orders,Customers,OrderLines etc. When you Save from the module level, all pending records in tables that belong to the module,
    /// will be saved as well. All master-detail relationships and keys will be updated automatically.
    /// </summary>
    public class OrpheusModule : IOrpheusModule
    {
        private IOrpheusTable mainTable;
        private ILogger<IOrpheusModule> logger;

        #region private methods
        /// <summary>
        /// Executes deletes for the module tables starting from the lowest level to the higher.
        /// </summary>
        /// <param name="transaction"></param>
        private void saveDeletes(IDbTransaction transaction)
        {
            var maxLevel = this.Tables.OrderByDescending(t => t.Level).ToList().First().Level;
            for (var level = maxLevel; level >= 0; level--)
            {
                var tables = this.Tables.FindAll(t => t.Level == level);
                foreach (IOrpheusTable table in tables)
                {
                    table.ExecuteDeletes(transaction);
                }
            }
        }
        /// <summary>
        /// Executes updates for the module tables starting from the lowest level to the higher.
        /// </summary>
        private void saveUpdates(IDbTransaction transaction)
        {
            var maxLevel = this.Tables.OrderByDescending(t => t.Level).ToList().First().Level;
            for (var level = maxLevel; level >= 0; level--)
            {
                var tables = this.Tables.FindAll(t => t.Level == level);
                foreach (IOrpheusTable table in tables)
                {
                    table.ExecuteUpdates(transaction);
                }
            }
        }
        /// <summary>
        /// Executes inserts for the module tables starting from the lowest level to the higher.
        /// </summary>
        private void saveInserts(IDbTransaction transaction)
        {
            var minLevel = this.Tables.OrderBy(t => t.Level).ToList().First().Level;
            var maxLevel = this.Tables.OrderByDescending(t => t.Level).ToList().First().Level;
            for (var level = minLevel; level <= maxLevel; level++)
            {
                var tables = this.Tables.FindAll(t => t.Level == level);
                foreach (IOrpheusTable table in tables)
                {
                    table.ExecuteInserts(transaction);
                }
            }
        }

        private IOrpheusTable createGenericTableFromOptions(IOrpheusTableOptions options)
        {
            Type generic = typeof(OrpheusTable<>);
            Type[] typeArgs = new Type[] { options.ModelType };
            Type constructedType = generic.MakeGenericType(typeArgs);

            // Ensure the Database property is set
            if (options.Database == null)
                options.Database = this.Database;

            // Dynamically create the logger type for IOrpheusTable<T>, not OrpheusTable<T>
            Type genericLogger = typeof(ILogger<>);
            Type tableInterface = typeof(IOrpheusTable<>).MakeGenericType(options.ModelType);
            Type constructedLoggerType = genericLogger.MakeGenericType(tableInterface);

            // Resolve the logger
            object logger = ServiceManager.GetLoggerService(constructedLoggerType);

            // Find the specific constructor with the matching parameters
            var constructor = constructedType.GetConstructor(new[] { typeof(IOrpheusTableOptions), constructedLoggerType });
            if (constructor == null)
                throw new InvalidOperationException($"Constructor with expected signature not found for type {constructedType}.");

            // Dynamically create the instance
            return (IOrpheusTable)constructor.Invoke(new object[] { options, logger });
        }

        private void initializeModuleDefinition()
        {
            if (this.Definition != null && this.Definition.MainTableOptions != null)
            {
                this.MainTable = this.createGenericTableFromOptions(this.Definition.MainTableOptions);

                foreach (var detailTableOption in this.Definition.DetailTableOptions)
                {
                    //if the table instance is not yet set, try to set it by using the master table name.
                    if (detailTableOption.MasterTable == null)
                    {
                        detailTableOption.MasterTable = this.getTableByName(detailTableOption.MasterTableName, this.Tables);// this.Tables.Where(t => t.Name.ToLower() == detailTableOption.MasterTableName.ToLower()).FirstOrDefault();
                    }
                    this.Tables.Add(this.createGenericTableFromOptions(detailTableOption));
                }

                //we cannot know in what order the detail table options will be entered.
                //so after the first iteration, we iterate through them again to set the master table, for each detail table.
                foreach (var detailTableOption in this.Definition.DetailTableOptions)
                {
                    var detailTable = this.getTableByName(detailTableOption.TableName, this.Tables);// this.Tables.Where(t => t.Name.ToLower() == detailTableOption.TableName.ToLower()).FirstOrDefault();
                    if (detailTable != null && detailTable.MasterTable == null)
                    {
                        detailTable.MasterTable = this.getTableByName(detailTableOption.MasterTableName, this.Tables);// this.Tables.Where(t => t.Name.ToLower() == detailTableOption.MasterTableName.ToLower()).FirstOrDefault();
                    }

                }

                foreach (var referenceTableOption in this.Definition.ReferenceTableOptions)
                {
                    this.ReferenceTables.Add(this.createGenericTableFromOptions(referenceTableOption));
                }
            }
        }

        private IOrpheusTable getTableByName(string tableName, List<IOrpheusTable> tableCollection)
        {
            //taking under consideration a named SQL server schema, of format [SchemaName].[ObjectName]. The splitter char is the dot.
            var moduleTable = tableCollection.Where(obj =>
                  obj.SchemaName == null ? obj.Name.ToLower() == tableName.ToLower() : obj.Name.Split(".")[1].Trim().ToLower() == tableName.ToLower()
               ).FirstOrDefault();
            //if (moduleTable == null)
            //    throw new Exception(String.Format("Table {0} not found.", tableName));
            return moduleTable;
        }

        private IOrpheusTable<T> getTableByName<T>(string tableName, List<IOrpheusTable> tableCollection)
        {
            //taking under consideration a named SQL server schema, of format [SchemaName].[ObjectName]. The splitter char is the dot.
            var moduleTable = tableCollection.Where(obj =>
                  obj.SchemaName == null ? obj.Name.ToLower() == tableName.ToLower() : obj.Name.Split(".")[1].Trim().ToLower() == tableName.ToLower()
               ).FirstOrDefault();
            //if (moduleTable == null)
            //    throw new Exception(String.Format("Table {0} not found.", tableName));
            return (IOrpheusTable<T>)moduleTable;
        }
        #endregion

        #region public properties

        /// <value>
        /// Module's definition.
        /// </value>
        public IOrpheusModuleDefinition Definition { get; private set; }

        /// <value>
        /// List of module's tables.
        /// </value>
        public List<IOrpheusTable> Tables { get; private set; }

        /// <value>
        /// List of module's reference tables. Reference tables are auxiliary or lookup tables.
        /// </value>
        public List<IOrpheusTable> ReferenceTables { get; private set; }

        /// <value>
        /// Module's database.
        /// </value>
        public IOrpheusDatabase Database { get; private set; }

        /// <value>
        /// The module's main table.
        /// </value>
        public IOrpheusTable MainTable
        {
            get
            {
                return this.mainTable;
            }
            set
            {
                this.mainTable = value;
                if (!this.Tables.Contains(this.mainTable))
                {
                    this.Tables.Add(this.mainTable);
                }
            }
        }

        /// <value>
        /// Occurs before records are save in the database.
        /// </value>
        public event EventHandler<ISaveEventArguments> OnBeforeSave;

        /// <value>
        /// Occurs after the transaction has been committed.
        /// </value>
        public event EventHandler<ISaveEventArguments> OnAfterSave;
        #endregion

        #region public methods
        /// <summary>
        /// Gets a reference table by index for a model
        /// </summary>
        /// <typeparam name="T">Table type</typeparam>
        /// <param name="index">Table index.</param>
        /// <returns></returns>
        public IOrpheusTable<T> GetTable<T>(int index)
        {
            return (IOrpheusTable<T>)this.Tables[index];
        }

        /// <summary>
        /// Gets a table by name for a model
        /// </summary>
        /// <typeparam name="T">Table type.</typeparam>
        /// <param name="tableName">Table name.</param>
        /// <returns></returns>
        public IOrpheusTable<T> GetTable<T>(string tableName)
        {
            return this.getTableByName<T>(tableName, this.Tables);
        }

        /// <summary>
        /// Gets a table by model. Uses the model class name as the table name.
        /// </summary>
        /// <typeparam name="T">Table type.</typeparam>
        /// <returns></returns>
        public IOrpheusTable<T> GetTable<T>()
        {
            var tableName = typeof(T).Name;
            return this.getTableByName<T>(tableName, this.Tables);
        }

        /// <summary>
        /// Gets a reference table by index for a model
        /// </summary>
        /// <typeparam name="T">Table type.</typeparam>
        /// <param name="index">Table index.</param>
        /// <returns></returns>
        public IOrpheusTable<T> GetReferenceTable<T>(int index)
        {
            return (IOrpheusTable<T>)this.ReferenceTables[index];
        }

        /// <summary>
        /// Gets a reference table by name for a model
        /// </summary>
        /// <typeparam name="T">Table type.</typeparam>
        /// <param name="tableName">Table name.</param>
        /// <returns></returns>
        public IOrpheusTable<T> GetReferenceTable<T>(string tableName)
        {
            return this.getTableByName<T>(tableName, this.ReferenceTables);
        }

        /// <summary>
        /// Gets a table by model. Uses the model class name as the table name.
        /// </summary>
        /// <typeparam name="T">Table type.</typeparam>
        /// <returns></returns>
        public IOrpheusTable<T> GetReferenceTable<T>()
        {
            var tableName = typeof(T).Name;
            return this.getTableByName<T>(tableName, this.ReferenceTables);
        }


        /// <summary>
        /// Saves all changes to the database within a transaction.
        /// </summary>
        public void Save()
        {
            var transaction = this.Database.BeginTransaction();
            this.OnBeforeSave?.Invoke(this, new SaveEventArguments() { Transaction = transaction });
            try
            {
                this.saveDeletes(transaction);
                this.saveUpdates(transaction);
                this.saveInserts(transaction);
                this.Database.CommitTransaction(transaction);
                this.OnAfterSave?.Invoke(this, new SaveEventArguments() { Transaction = transaction });
            }
            catch (Exception e)
            {
                this.logger.LogError(ErrorCodes.ERR_SAVING_DATA, e, $"{ErrorDictionary.GetError(ErrorCodes.ERR_SAVING_DATA)}");
                this.Database.RollbackTransaction(transaction);
                throw;
            }
        }

        /// <summary>
        /// Loads a module's record from the database.
        /// </summary>
        /// <param name="keyValues">The key values</param>
        /// <exception cref="Exception">If you want to load a record from the module level, you need to define the main table for the module.</exception>
        public void Load(List<object> keyValues = null)
        {
            if (this.MainTable != null)
            {
                this.MainTable.Load(keyValues);
                foreach (var table in this.Tables)
                {
                    if (table.MasterTable != null)
                        table.Load();
                }
            }
            else
            {
                throw new Exception("If you want to load a record from the module level, you need to define the main table for the module.");
            }
        }

        /// <summary>
        /// Loads records from the DB to the table.
        /// You can configure having multiple fields and multiple values per field.
        /// Multiple field values are bound with a logical OR.
        /// Multiple fields by default are bound with a logical OR.
        /// Defining a logical operator, you can change the default behavior.
        /// This applies only for the MainTable.
        /// </summary>
        /// <param name="keyValues">The key values.</param>
        /// <param name="logicalOperator">The logical operator.</param>
        /// <param name="clearExistingData">If true, it will clear all existing data. Default is true.</param>
        /// <exception cref="Exception">If you want to load a record from the module level, you need to define the main table for the module.</exception>
        public void Load(Dictionary<string, List<object>> keyValues, LogicalOperator logicalOperator = LogicalOperator.loOR, bool clearExistingData = true)
        {

            if (this.MainTable != null)
            {
                this.MainTable.Load(keyValues, logicalOperator, clearExistingData);
                this.MainTable.DetailTables.ForEach(t => t.Load());
            }
            else
            {
                throw new Exception("If you want to load a record from the module level, you need to define the main table for the module.");
            }
        }

        /// <summary>
        /// Loads main table data by executing a db command.
        /// </summary>
        /// <param name="dbCommand">The IDbCommand instance.</param>
        /// <param name="clearExistingData">If true, it will clear all existing data. Default is true.</param>
        /// <exception cref="Exception">If you want to load a record from the module level, you need to define the main table for the module.</exception>
        public void Load(IDbCommand dbCommand, bool clearExistingData = true)
        {
            if (this.MainTable != null)
            {
                this.MainTable.Load(dbCommand, clearExistingData);
                this.MainTable.DetailTables.ForEach(t => t.Load());
            }
            else
            {
                throw new Exception("If you want to load a record from the module level, you need to define the main table for the module.");
            }
        }

        /// <summary>
        /// Clears data from all module tables.
        /// </summary>
        public void ClearData()
        {
            this.Tables.ForEach(t => t.ClearData());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrpheusModule"/> class.
        /// OrpheusModule class represents a logical division and grouping of a set of tables.
        /// For example you can an OrdersModule, which will be comprised from many different tables.
        /// Orders,Customers,OrderLines etc. When you Save from the module level, all pending records in tables that belong to the module,
        /// will be saved as well. All master-detail relationships and keys will be updated automatically.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="logger">The logger</param>
        public OrpheusModule(IOrpheusDatabase database, ILogger<IOrpheusModule> logger)
        {
            this.Database = database;
            this.Tables = new List<IOrpheusTable>();
            this.ReferenceTables = new List<IOrpheusTable>();
            this.initializeModuleDefinition();
            this.logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrpheusModule"/> class.
        /// OrpheusModule class represents a logical division and grouping of a set of tables.
        /// For example you can an OrdersModule, which will be comprised from many different tables.
        /// Orders,Customers,OrderLines etc. When you Save from the module level, all pending records in tables that belong to the module,
        /// will be saved as well. All master-detail relationships and keys will be updated automatically.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="logger">The logger</param>
        public OrpheusModule(IOrpheusDatabase database, IOrpheusModuleDefinition definition, ILogger<IOrpheusModule> logger) : this(database, logger)
        {
            this.Definition = definition;
            this.initializeModuleDefinition();
        }
        #endregion


    }
}
