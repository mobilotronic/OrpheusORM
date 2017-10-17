﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusInterfaces
{
    /// <summary>
    /// Represents an Orpheus table key field.
    /// </summary>
    public interface IOrpheusTableKeyField
    {
        /// <summary>
        /// Name of the field that is the table key.
        /// </summary>
        /// <returns>Field name</returns>
        string Name { get; set; }

        /// <summary>
        /// True if the underlying db engine is going to generate the value for the key.
        /// </summary>
        /// <returns>True if field value is DB generated</returns>
        bool IsDBGenerated { get; set; }

        /// <summary>
        /// Function that returns a SQL string to be used in a WHERE clause, to select the new key value(s) after an insert.
        /// </summary>
        /// <returns>Function that returns a SQL string</returns>
        Func<string> KeySQLUpdate { get; set; }

        /// <summary>
        /// If set to true and the type is System.Guid then with every new insert, a value will be
        /// auto generated.
        /// </summary>
        /// <returns>True if the field value is auto generated</returns>
        bool IsAutoGenerated { get; set; }
    }
}