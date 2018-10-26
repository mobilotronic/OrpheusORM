using System.Data;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// Orpheus save event arguments. Occurs when data are actually being save to the database.
    /// </summary>
    public interface ISaveEventArguments
    {
        /// <value>
        /// Current transaction.
        /// </value>
        IDbTransaction Transaction { get; set; }
    }

    /// <summary>
    /// Orpheus record modify event. Occurs when data are processed in memory.
    /// </summary>
    /// <typeparam name="T">Model type</typeparam>
    public interface IModifyRecordEventArguments<T>
    {
        /// <value>
        /// Modified record.
        /// </value>
        T Record { get; }
        /// <value>
        /// Modify action. 0 Insert, 1 Update, 2 Delete
        /// </value>
        int ModifyAction { get;}
    }
}
