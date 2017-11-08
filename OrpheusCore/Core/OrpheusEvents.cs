using OrpheusInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace OrpheusCore
{
    internal class SaveEventArguments : ISaveEventArguments
    {
        public IDbTransaction Transaction { get; set; }
    }

    internal class ModifyRecordEventArguments<T> : IModifyRecordEventArguments<T>
    {
        public T Record { get; private set; }
        public int ModifyAction { get; private set; }

        public ModifyRecordEventArguments(int modifyAction, T record)
        {
            this.ModifyAction = modifyAction;
            this.Record = record;
        }
    }
}
