using System.Collections.Generic;

namespace OrpheusCore.Errors
{
    /// <summary>
    /// List of known Orpheus error codes.
    /// </summary>
    public static class ErrorCodes
    {
        #region database related errors
        /// <summary>
        /// The error cannot connect to database
        /// </summary>
        public const int ERR_CANNOT_CONNECT_TO_DB = 100;
        /// <summary>
        /// The error cannot create database
        /// </summary>
        public const int ERR_CANNOT_CREATE_DB = 101;
        /// <summary>
        /// The error cannot run DDL
        /// </summary>
        public const int ERR_CANNOT_RUN_DDL = 102;
        #endregion

        #region table related erros
        /// <summary>
        /// The error loading data
        /// </summary>
        public const int ERR_LOADING_DATA = 200;
        /// <summary>
        /// The error deleting data
        /// </summary>
        public const int ERR_DELETING_DATA = 201;
        /// <summary>
        /// The error updating data
        /// </summary>
        public const int ERR_UPDATING_DATA = 202;
        /// <summary>
        /// The error inserting data
        /// </summary>
        public const int ERR_INSERTING_DATA = 203;
        /// <summary>
        /// The error saving data
        /// </summary>
        public const int ERR_SAVING_DATA = 204;
        #endregion

        #region schema related errors
        /// <summary>
        /// The error schema object exists
        /// </summary>
        public const int ERR_SCHEMA_OBJECT_EXISTS = 300;
        /// <summary>
        /// The error schema add dependency
        /// </summary>
        public const int ERR_SCHEMA_ADD_DEPENDENCY = 301;
        /// <summary>
        /// The error schema execute
        /// </summary>
        public const int ERR_SCHEMA_EXECUTE = 302;
        /// <summary>
        /// The error schema object identifier
        /// </summary>
        public const int ERR_SCHEMA_OBJECT_ID = 303;
        #endregion

    }
    /// <summary>
    /// 
    /// </summary>
    public static class ErrorDictionary
    {
        private static Dictionary<int, string> orpheusErrors = new Dictionary<int, string>()
        {
            {ErrorCodes.ERR_CANNOT_CONNECT_TO_DB,"Orpheus could not connect to the database engine." },
            {ErrorCodes.ERR_CANNOT_CREATE_DB,"Orpheus could not validate the existance of or create database name." },
            {ErrorCodes.ERR_CANNOT_RUN_DDL,"Orpheus could not execute the DDL command." },
            {ErrorCodes.ERR_LOADING_DATA,"Orpheus could not load data." },
            {ErrorCodes.ERR_DELETING_DATA,"Orpheus could not delete data." },
            {ErrorCodes.ERR_UPDATING_DATA,"Orpheus could not update data." },
            {ErrorCodes.ERR_INSERTING_DATA,"Orpheus could not insert data." },
            {ErrorCodes.ERR_SAVING_DATA,"Orpheus could not save data." },
            {ErrorCodes.ERR_SCHEMA_OBJECT_EXISTS,"Orpheus could not find the schema object." },
            {ErrorCodes.ERR_SCHEMA_ADD_DEPENDENCY,"Orpheus could not add a schema dependency." },
            {ErrorCodes.ERR_SCHEMA_EXECUTE,"Orpheus could not execute schema." },
            {ErrorCodes.ERR_SCHEMA_OBJECT_ID,"Orpheus could not retrieve the schema object id." }
        };

        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns></returns>
        public static string GetError(int errorCode) => orpheusErrors.ContainsKey(errorCode) ? orpheusErrors[errorCode] : "";

    }
}
