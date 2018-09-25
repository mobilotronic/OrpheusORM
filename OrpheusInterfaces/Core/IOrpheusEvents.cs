using System.Data;

namespace OrpheusInterfaces.Core
{
    /// <summary>
    /// Orpheus save event arguments. Occurs when data are actually being save to the database.
    /// </summary>
    public interface ISaveEventArguments
    {
        /// <summary>
        /// Current transaction.
        /// </summary>
        IDbTransaction Transaction { get; set; }
    }

    /// <summary>
    /// Orpheus record modify event. Occurs when data are processed in memory.
    /// </summary>
    /// <typeparam name="T">Model type</typeparam>
    public interface IModifyRecordEventArguments<T>
    {
        /// <summary>
        /// Modified record.
        /// </summary>
        T Record { get; }
        /// <summary>
        /// Modify action. 0 Insert, 1 Update, 2 Delete
        /// </summary>
        int ModifyAction { get;}
    }
}
