using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Lasy
{
    /// <summary>
    /// Represents the type of a sql column
    /// </summary>
    public class SqlColumnType
    {
        public SqlColumnType(SqlDbType type, bool isNullable = false, int? length = null)
        {
            Type = type;
            IsNullable = isNullable;
            Length = length;
        }

        public SqlDbType Type;
        public bool IsNullable;
        public int? Length = null;

        public override string ToString()
        {
            return Type.ToString().ToLower() + 
                (Length.HasValue ? "(" + Length.Value + ")" : "")
                + " " + (IsNullable ? "NULL" : "NOT NULL");
        }
    }
}
