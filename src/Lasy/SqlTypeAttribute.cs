using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Lasy
{
    /// <summary>
    /// Used to decorate the fields of an object in order to hint to Lasy what 
    /// SQL type it should use if creating tables
    /// </summary>
    public class SqlTypeAttribute : Attribute
    {
        //public SqlTypeAttribute(bool? isNullable = null,
        //    int? precision = null,
        //    int? scale = null,
        //    int? length = null,
        //    SqlDbType? type = null)
        //{
        //    IsNullable = isNullable;
        //    Precision = precision;
        //    Scale = scale;
        //    Length = length;
        //    Type = type;
        //}

        public SqlTypeAttribute(SqlDbType? type, bool? isNullable, int? length, int? precision, int? scale)
        {
            IsNullable = isNullable;
            Precision = precision;
            Scale = scale;
            Length = length;
            Type = type;
        }

        public SqlTypeAttribute()
            : this(null, null, null, null, null)
        { }

        public SqlTypeAttribute(int length)
            : this(null, null, length, null, null)
        { }

        public SqlTypeAttribute(bool isNullable, int length)
            : this(null, isNullable, length, null, null)
        { }

    
        public bool? IsNullable;
        public int? Precision;
        public int? Scale;
        public int? Length;
        public SqlDbType? Type;
    }
}
