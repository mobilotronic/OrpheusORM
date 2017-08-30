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
        string Name { get; }
        
        /// <summary>
        /// True when table data have been modified.
        /// </summary>
        bool Modified { get; }
        
        /// <summary>
        /// Master table name.
        /// </summary>
        IOrpheusTable MasterTable { get; set; }
        
        /// <summary>
        /// Master table's key field(s)
        /// </summary>
        List<IOrpheusTableKeyField> MasterTableKeyFields { get; }
        
        /// <summary>
        /// Table's key field(s).
        /// </summary>
        List<IOrpheusTableKeyField> KeyFields { get; set; }
        
        /// <summary>
        /// List of dependent detail tables.
        /// </summary>
        List<IOrpheusTable> DetailTables { get; }
        
        /// <summary>
        /// Table's level. Zero if the table is not a child to any other table.
        /// </summary>
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
        /// Clears existing loaded data.
        /// </summary>
        void ClearData();
        
        /// <summary>
        /// Returns list of current key values.
        /// </summary>
        /// <returns></returns>
        List<object> GetKeyValues();
    }
    
    /// <summary>
    /// Orpheus table is the core component of an Orpheus module. Every module needs to have at least 1 table.
    /// Holds all the data of the connected table.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOrpheusTable<T> : IOrpheusTable
    {

        /// <summary>
        /// Table's data.
        /// </summary>
        List<T> Data { get; }

        /// <summary>
        /// Adds a new record to the table.
        /// </summary>
        /// <param name="newRecord"></param>
        void Add(T newRecord);

        /// <summary>
        /// Adds a list of new records.
        /// </summary>
        /// <param name="newRecords"></param>
        void Add(List<T> newRecords);
        
        /// <summary>
        /// Updates an existing record.
        /// </summary>
        /// <param name="record"></param>
        void Update(T record);

        /// <summary>
        /// Updates existing records.
        /// </summary>
        /// <param name="records"></param>
        void Update(List<T> records);

        /// <summary>
        /// Deletes records.
        /// </summary>
        /// <param name="records"></param>
        void Delete(List<T> records);
        
        /// <summary>
        /// Deletes a record.
        /// </summary>
        /// <param name="record"></param>
        void Delete(T record);

    }

    /// <summary>
    /// Represents <see cref="IOrpheusTable"/> options, which can be used to instantiate a table.
    /// </summary>
    public interface IOrpheusTableOptions
    {
        /// <summary>
        /// Database that the table is a part of.
        /// </summary>
        [IgnoreDataMember]
        IOrpheusDatabase Database { get; set; }
        

        /// <summary>
        /// Table's name.
        /// </summary>
        string TableName { get; set; }
       
        /// <summary>
        /// Table's key field(s). Can be more than one to support composite keys.
        /// </summary>
        List<IOrpheusTableKeyField> KeyFields { get; set; }

        /// <summary>
        /// Creates a new key field.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isAutoGenerated"></param>
        /// <param name="isDBGenerated"></param>
        /// <param name="keySQLUpdate"></param>
        void AddKeyField(string name, bool isAutoGenerated = false,bool isDBGenerated = false, Func<string> keySQLUpdate = null);

        /// <summary>
        /// Creates a new master key field
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isAutoGenerated"></param>
        /// <param name="isDBGenerated"></param>
        /// <param name="keySQLUpdate"></param>
        void AddMasterKeyField(string name, bool isAutoGenerated = false, bool isDBGenerated = false, Func<string> keySQLUpdate = null);

        /// <summary>
        /// Table's master table. To support the master-detail relationship.
        /// </summary>
        [IgnoreDataMember]
        IOrpheusTable MasterTable { get; set; }

        /// <summary>
        /// Table's master table. To support the master-detail relationship.
        /// </summary>
        string MasterTableName { get; set; }

        /// <summary>
        /// Master table's key field(s). Can be more than one to support composite keys.
        /// </summary>
        List<IOrpheusTableKeyField> MasterTableKeyFields { get; set; }

        /// <summary>
        /// Model type.
        /// </summary>
        Type ModelType { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public interface IOrpheusReferenceTable : IOrpheusTable { }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOrpheusReferenceTable<T>: IOrpheusReferenceTable {    }
}
