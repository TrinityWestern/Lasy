using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Nvelope;

namespace Lasy
{
    /// <summary>
    /// Represents the type of a sql column
    /// </summary>
    public class SqlColumnType
    {
        public SqlColumnType(SqlDbType type, bool isNullable = false, int? length = null, int? precision = null, int? scale = null)
        {
            SqlType = type;
            IsNullable = isNullable;
            Length = length;
            Precision = precision;
            Scale = scale;
        }

        public SqlDbType SqlType;
        public DbType DbType
        {
            get
            {
                return SqlTypeConversion.GetDbType(SqlType);
            }
        }
        public bool IsNullable;
        public int? Length;
        public int? Precision;
        public int? Scale;

        public override string ToString()
        {
            var lengthStr = Length.HasValue ? "(" + Length.Value + ")" : "";

            // Only decimal types have precisions, so don't print it, even if it's 
            // set for other types
            var precisionStr = "";
            if(SqlType == SqlDbType.Decimal && Precision.HasValue)
                precisionStr = "(" + Precision.Value + 
                    (Scale.HasValue ? "," + Scale.Value : "") + ")";

            return SqlType.ToString().ToLower() + 
                lengthStr + precisionStr 
                + " " + (IsNullable ? "NULL" : "NOT NULL");
        }
    }
}
