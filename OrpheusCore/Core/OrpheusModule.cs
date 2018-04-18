using OrpheusInterfaces;
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

        /// <summary>
        /// Executes deletes for the module tables starting from the lowest level to the higher.
        /// </summary>
        /// <param name="transaction"></param>
        private void saveDeletes(IDbTransaction transaction)
        {
            var maxLevel = this.Tables.OrderByDescending(t => t.Level).ToList().First().Level;
            for(var level = maxLevel; level >=0; level--)
            {
                var tables = this.Tables.FindAll(t => t.Level == level);
                foreach(IOrpheusTable table in tables)
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
            if (options.Database == null)
                options.Database = this.Database;
            return (IOrpheusTable)Activator.CreateInstance(constructedType, new object[] { options });
        }

        private void initializeModuleDefinition()
        {
            if(this.Definition != null && this.Definition.MainTableOptions != null)
            {
                this.MainTable = this.createGenericTableFromOptions(this.Definition.MainTableOptions);

                foreach(var detailTableOption in this.Definition.DetailTableOptions)
                {
                    //if the table instance is not yet set, try to set it by using the master table name.
                    if(detailTableOption.MasterTable == null)
                    {
                        detailTableOption.MasterTable = this.getTableByName(detailTableOption.MasterTableName,this.Tables);// this.Tables.Where(t => t.Name.ToLower() == detailTableOption.MasterTableName.ToLower()).FirstOrDefault();
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

        /// <summary>
        /// Module's definition.
        /// </summary>
        public IOrpheusModuleDefinition Definition { get; private set; }
        
        /// <summary>
        /// List of module's tables.
        /// </summary>
        public List<IOrpheusTable> Tables { get; private set; }

        /// <summary>
        /// Gets a reference table by index for a model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public IOrpheusTable<T> GetTable<T>(int index)
        {
            return (IOrpheusTable<T>)this.Tables[index];
        }

        /// <summary>
        /// Gets a table by name for a model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IOrpheusTable<T> GetTable<T>(string tableName)
        {
            return this.getTableByName<T>(tableName,this.Tables);
        }

        /// <summary>
        /// Gets a table by model. Uses the model class name as the table name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IOrpheusTable<T> GetTable<T>()
        {
            var tableName = typeof(T).Name;
            return this.getTableByName<T>(tableName, this.Tables);
        }

        /// <summary>
        /// Gets a reference table by index for a model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public IOrpheusTable<T> GetReferenceTable<T>(int index)
        {
            return (IOrpheusTable<T>)this.ReferenceTables[index];
        }

        /// <summary>
        /// Gets a reference table by name for a model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IOrpheusTable<T> GetReferenceTable<T>(string tableName)
        {
            return this.getTableByName<T>(tableName, this.ReferenceTables);
        }

        /// <summary>
        /// Gets a table by model. Uses the model class name as the table name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IOrpheusTable<T> GetReferenceTable<T>()
        {
            var tableName = typeof(T).Name;
            return this.getTableByName<T>(tableName, this.ReferenceTables);
        }

        /// <summary>
        /// List of module's reference tables. Reference tables are auxiliary or lookup tables.
        /// </summary>
        public List<IOrpheusTable> ReferenceTables { get; private set; }
        
        /// <summary>
        /// Module's database.
        /// </summary>
        public IOrpheusDatabase Database { get; private set; }

        /// <summary>
        /// The module's main table.
        /// </summary>
        public IOrpheusTable MainTable {
            get {
                return this.mainTable;
            }
            set {
                this.mainTable = value;
                if (!this.Tables.Contains(this.mainTable))
                {
                    this.Tables.Add(this.mainTable);
                }
            }
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
            catch(Exception exception)
            {
                this.Database.RollbackTransaction(transaction);
                throw exception;
            }
        }
        
        /// <summary>
        /// Loads a module's record from the database.
        /// </summary>
        /// <param name="keyValues"></param>
        public void Load(List<object> keyValues = null)
        {
            if(this.MainTable != null)
            {
                this.MainTable.Load(keyValues);
                foreach (var table in this.Tables)
                {
                    if(table.MasterTable != null)
                        table.Load();
                }
            }
            else
            {
                throw new Exception("If you want to load a record from the module level, you need to define the main table for the module.");
            }
        }

        /// <summary>
        /// Loads a module's record from the database.
        /// You can configure having multiple fields and multiple values per field.
        /// Multiple field values are bound with a logical OR.
        /// Multiple fields are bound with a logical AND
        /// </summary>
        /// <param name="keyValues"></param>
        public void Load(Dictionary<string, List<object>> keyValues)
        {
            if (this.MainTable != null)
            {
                this.MainTable.Load(keyValues);
                foreach (var table in this.Tables)
                    table.Load();
            }
            else
            {
                throw new Exception("If you want to load a record from the module level, you need to define the main table for the module.");
            }
        }

        /// <summary>
        /// OrpheusModule class represents a logical division and grouping of a set of tables.
        /// For example you can an OrdersModule, which will be comprised from many different tables.
        /// Orders,Customers,OrderLines etc. When you Save from the module level, all pending records in tables that belong to the module,
        /// will be saved as well. All master-detail relationships and keys will be updated automatically.
        /// </summary>
        /// <param name="database">Module's database</param>
        public OrpheusModule(IOrpheusDatabase database)
        {
            this.Database = database;
            this.Tables = new List<IOrpheusTable>();
            this.ReferenceTables = new List<IOrpheusTable>();
            this.initializeModuleDefinition();
        }

        /// <summary>
        /// OrpheusModule class represents a logical division and grouping of a set of tables.
        /// For example you can an OrdersModule, which will be comprised from many different tables.
        /// Orders,Customers,OrderLines etc. When you Save from the module level, all pending records in tables that belong to the module,
        /// will be saved as well. All master-detail relationships and keys will be updated automatically.
        /// </summary>
        /// <param name="database">Module's database.</param>
        /// <param name="definition">Module definition.</param>
        public OrpheusModule(IOrpheusDatabase database, IOrpheusModuleDefinition definition):this(database)
        {
            this.Definition = definition;
            this.initializeModuleDefinition();
        }

        /// <summary>
        /// Occurs before records are save in the database.
        /// </summary>
        public event EventHandler<ISaveEventArguments> OnBeforeSave;

        /// <summary>
        /// Occurs after the transaction has been committed.
        /// </summary>
        public event EventHandler<ISaveEventArguments> OnAfterSave;
    }
}
