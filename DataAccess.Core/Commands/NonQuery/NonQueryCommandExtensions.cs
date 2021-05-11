using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public static class NonQueryCommandExtensions
    {
        public static NonQueryCommand ThrowWhenNoRecordIsUpdated(this NonQueryCommand command, bool throwWhenNoRecordIsUpdated)
        {
            command.ThrowWhenNoRecordIsUpdated = throwWhenNoRecordIsUpdated;

            return command;
        }
    }
}
