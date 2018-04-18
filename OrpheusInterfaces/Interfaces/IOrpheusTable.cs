using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
{

    /// <summary>
    /// Orpheus table is the core Orpheus data object. It is responsible for performing data operations.
    /// </summary>
    public interface IOrpheusTable
    {
        /// <summary>
        /// The table name.
        /// </summary>
        /// <returns>Table name</returns>
        string Name { get; }

        /// <summary>
        /// The table's schema name. Applicable only if the db engine is SQL Server.
        /// </summary>
        string SchemaName { get; }

        /// <summary>
        /// True when table data have been modified.
        /// </summary>
        /// <returns>True if table data have been modified</returns>
        bool Modified { get; }

        /// <summary>
        /// Master table name, if the table is a detail table.
        /// </summary>
        /// <returns>Master table name</returns>
        IOrpheusTable MasterTable { get; set; }

        /// <summary>
        /// Master table's key field(s), if the table is detail table.
        /// </summary>
        /// <returns>Master table key fields</returns>
        List<IOrpheusTableKeyField> MasterTableKeyFields { get; }

        /// <summary>
        /// Table's key field(s).
        /// </summary>
        /// <returns>Table's key field(s)</returns>
        List<IOrpheusTableKeyField> KeyFields { get; set; }

        /// <summary>
        /// List of dependent detail tables.
        /// </summary>
        /// <returns>List of detail tables</returns>
        List<IOrpheusTable> DetailTables { get; }

        /// <summary>
        /// Table's level. Zero if the table is not a child to any other table.
        /// </summary>
        /// <returns>Table's level</returns>
        int Level { get; }
        
        /// <summary>
        /// Executes any delete changes that the table has.
        /// </summary>
        void ExecuteDeletes(IDbTransaction transaction);
        
        /// <summary>
        /// Executes any update changes that the table has.
        /// </summary>
        void ExecuteUpdates(IDbTransaction transaction);
        
        /// <summary>
        /// Executes any insert changes that the table has.
        /// </summary>
        void ExecuteInserts(IDbTransaction transaction);
        
        /// <summary>
        /// Loads records from the DB to the table.
        /// </summary>
        void Load(List<object> keyValues = null, bool clearExistingData = true);

        /// <summary>
        /// Loads records from the DB to the table.
        /// You can configure having multiple fields and multiple values per field.
        /// Multiple field values are bound with a logical OR.
        /// Multiple fields are bound with a logical AND
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="clearExistingData"></param>
        void Load(Dictionary<string,List<object>> keyValues, bool clearExistingData = true);

        /// <summary>
        /// Loads table data by executing a SQL command.
        /// </summary>
        /// <param name="SQL">SQL command to be executed</param>
        /// <param name="clearExistingData"></param>
        void Load(string SQL, bool clearExistingData = true);

        /// <summary>
        /// Loads table data by executing a db command.
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="clearExistingData"></param>
        void Load(IDbCommand dbCommand, bool clearExistingData = true);

        /// <summary>
        /// Clears existing loaded data.
        /// </summary>
        void ClearData();
        
        /// <summary>
        /// Returns list of current key values.
        /// </summary>
        /// <returns>List of current key values</returns>
        List<object> GetKeyValues();

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        /// <param name="dbTransaction">Transaction in which the commands will be executed</param>
        /// <param name="commitTransaction">Commit transaction after save.</param>
        void Save(IDbTransaction dbTransaction = null, bool commitTransaction = true);

        /// <summary>
        /// Occurs before records are save in the database.
        /// </summary>
        event EventHandler<ISaveEventArguments> OnBeforeSave;

        /// <summary>
        /// Occurs after the transaction has been committed.
        /// </summary>
        event EventHandler<ISaveEventArguments> OnAfterSave;
    }
    
    /// <summary>
    /// Orpheus table is the core component of an Orpheus module. Every module needs to have at least 1 table.
    /// Holds all the data of the connected table.
    /// </summary>
    /// <typeparam name="T">Model type</typeparam>
    public interface IOrpheusTable<T> : IOrpheusTable
    {

        /// <summary>
        /// Table's data.
        /// </summary>
        /// <returns>Table's data</returns>
        List<T> Data { get; }

        /// <summary>
        /// Adds a new record to the table.
        /// </summary>
        /// <param name="newRecord">New record to be added</param>
        void Add(T newRecord);

        /// <summary>
        /// Adds a list of new records.
        /// </summary>
        /// <param name="newRecords">New records to be added</param>
        void Add(List<T> newRecords);
        
        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="record">Record to be updated</param>
        void Update(T record);

        /// <summary>
        /// Updates existing records.
        /// </summary>
        /// <param name="records">Records to be updated</param>
        void Update(List<T> records);

        /// <summary>
        /// Deletes records.
        /// </summary>
        /// <param name="records">Records to be deleted</param>
        void Delete(List<T> records);
        
        /// <summary>
        /// Deletes a record.
        /// </summary>
        /// <param name="record">Record to delete</param>
        void Delete(T record);

        /// <summary>
        /// Occurs before a table modifies a record. It is fired on any Add,Update,Delete
        /// </summary>
        event EventHandler<IModifyRecordEventArguments<T>> OnBeforeModify;

        /// <summary>
        /// Occurs after a table modifies a record. It is fired on any Add,Update,Delete
        /// </summary>
        event EventHandler<IModifyRecordEventArguments<T>> OnAfterModify;

    }

    /// <summary>
    /// Represents <see cref="IOrpheusTable"/> options, which can be used to instantiate a table.
    /// </summary>
    public interface IOrpheusTableOptions
    {
        /// <summary>
        /// Database that the table is a part of.
        /// </summary>
        /// <returns>Database that the table is part of</returns>
        [IgnoreDataMember]
        IOrpheusDatabase Database { get; set; }


        /// <summary>
        /// Table's name.
        /// </summary>
        /// <returns>Table's name</returns>
        string TableName { get; set; }

        /// <summary>
        /// Table's key field(s). Can be more than one to support composite keys.
        /// </summary>
        /// <returns>Table's key fields</returns>
        List<IOrpheusTableKeyField> KeyFields { get; set; }

        /// <summary>
        /// Creates a new key field.
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="isAutoGenerated">Auto generate flag</param>
        /// <param name="isDBGenerated">DB generated flag</param>
        /// <param name="keySQLUpdate">Function to return a custom SQL when updating the field value</param>
        void AddKeyField(string name, bool isAutoGenerated = false,bool isDBGenerated = false, Func<string> keySQLUpdate = null);

        /// <summary>
        /// Creates a new master key field
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="isAutoGenerated">Auto generate flag</param>
        /// <param name="isDBGenerated">DB generated flag</param>
        /// <param name="keySQLUpdate">Function to return a custom SQL when updating the field value</param>
        void AddMasterKeyField(string name, bool isAutoGenerated = false, bool isDBGenerated = false, Func<string> keySQLUpdate = null);

        /// <summary>
        /// Table's master table. To support the master-detail relationship.
        /// </summary>
        /// <returns>Table's master table</returns>
        [IgnoreDataMember]
        IOrpheusTable MasterTable { get; set; }

        /// <summary>
        /// Table's master table. To support the master-detail relationship.
        /// </summary>
        /// <returns>Table's master table name</returns>
        string MasterTableName { get; set; }

        /// <summary>
        /// Master table's key field(s). Can be more than one to support composite keys.
        /// </summary>
        /// <returns>Master table's key field</returns>
        List<IOrpheusTableKeyField> MasterTableKeyFields { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        /// <returns>Table model type</returns>
        Type ModelType { get; set; }
    }


    /// <summary>
    /// Orpheus reference table.
    /// </summary>
    public interface IOrpheusReferenceTable : IOrpheusTable { }

    /// <summary>
    /// Orpheus reference table.
    /// </summary>
    /// <typeparam name="T">Model type</typeparam>
    public interface IOrpheusReferenceTable<T>: IOrpheusReferenceTable {    }
}
