using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using Microsoft.SqlServer.Server;
using Nvelope;
using Nvelope.Reflection;
using System.Reflection;

namespace Lasy
{
    /// <summary>
    /// Conversion to and from SQL data types
    /// </summary>
    public class SqlTypeConversion
    {
        static SqlTypeConversion()
        {
            _toSqlMappings.Add(typeof(bool?), new SqlColumnType(SqlDbType.Bit, true));
            _toSqlMappings.Add(typeof(bool), new SqlColumnType(SqlDbType.Bit));
            _toSqlMappings.Add(typeof(char?), new SqlColumnType(SqlDbType.Char, true, length: 1));
            _toSqlMappings.Add(typeof(char), new SqlColumnType(SqlDbType.Char, false, length: 1));
            _toSqlMappings.Add(typeof(int?), new SqlColumnType(SqlDbType.Int, true));
            _toSqlMappings.Add(typeof(int), new SqlColumnType(SqlDbType.Int));
            _toSqlMappings.Add(typeof(Nullable<Int64>), new SqlColumnType(SqlDbType.BigInt, true));
            _toSqlMappings.Add(typeof(Int64), new SqlColumnType(SqlDbType.BigInt));
            _toSqlMappings.Add(typeof(float?), new SqlColumnType(SqlDbType.Real, true));
            _toSqlMappings.Add(typeof(float), new SqlColumnType(SqlDbType.Real));
            _toSqlMappings.Add(typeof(double?), new SqlColumnType(SqlDbType.Float, true));
            _toSqlMappings.Add(typeof(double), new SqlColumnType(SqlDbType.Float));
            _toSqlMappings.Add(typeof(decimal?), new SqlColumnType(SqlDbType.Decimal, true, precision: 36, scale: 12));
            _toSqlMappings.Add(typeof(decimal), new SqlColumnType(SqlDbType.Decimal, precision: 36, scale: 12));
            _toSqlMappings.Add(typeof(DateTime?), new SqlColumnType(SqlDbType.DateTime, true));
            _toSqlMappings.Add(typeof(DateTime), new SqlColumnType(SqlDbType.DateTime));
            _toSqlMappings.Add(typeof(string), new SqlColumnType(SqlDbType.NVarChar, true, length: 100));
            _toSqlMappings.Add(typeof(XmlDocument), new SqlColumnType(SqlDbType.Xml, true));
            _toSqlMappings.Add(typeof(Guid?), new SqlColumnType(SqlDbType.UniqueIdentifier, true));
            _toSqlMappings.Add(typeof(Guid), new SqlColumnType(SqlDbType.UniqueIdentifier));
            _toSqlMappings.Add(typeof(Byte), new SqlColumnType(SqlDbType.TinyInt));
            // Note - this isn't ideal - SByte is signed 8-bit, while SmallInt is signed 16-bit
            // I don't think there's an exact match
            _toSqlMappings.Add(typeof(SByte), new SqlColumnType(SqlDbType.SmallInt));

            // _toCMappings are just the inverses of the _toSqlMappings, plus a few overloads
            _toCMappings = _toSqlMappings.Invert();
            _toCMappings.Add(new SqlColumnType(SqlDbType.NVarChar), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.VarChar, true), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.VarChar), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.Char), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.NChar), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.Xml), typeof(XmlDocument));


            // _toDbMappings contain mappings of SqlDbTypes to DbTypes
            // DbTypes are used for database-agnostic mapping, while SqlDbTypes are SQL-server specific
            _toDbMappings.Add(SqlDbType.BigInt, DbType.Int64);
            _toDbMappings.Add(SqlDbType.Binary, DbType.Binary);
            _toDbMappings.Add(SqlDbType.Bit, DbType.Boolean);
            _toDbMappings.Add(SqlDbType.Char, DbType.AnsiString);
            _toDbMappings.Add(SqlDbType.Date, DbType.Date);
            _toDbMappings.Add(SqlDbType.DateTime, DbType.DateTime);
            _toDbMappings.Add(SqlDbType.DateTime2, DbType.DateTime2);
            _toDbMappings.Add(SqlDbType.DateTimeOffset, DbType.DateTimeOffset);
            _toDbMappings.Add(SqlDbType.Decimal, DbType.Decimal);
            _toDbMappings.Add(SqlDbType.Float, DbType.Single);
            _toDbMappings.Add(SqlDbType.Image, DbType.Binary);
            _toDbMappings.Add(SqlDbType.Int, DbType.Int32);
            _toDbMappings.Add(SqlDbType.Money, DbType.Currency);
            _toDbMappings.Add(SqlDbType.NChar, DbType.String);
            
            _toDbMappings.Add(SqlDbType.NText, DbType.String);
            _toDbMappings.Add(SqlDbType.NVarChar, DbType.String);
            _toDbMappings.Add(SqlDbType.Real, DbType.Double);
            _toDbMappings.Add(SqlDbType.SmallDateTime, DbType.DateTime);
            _toDbMappings.Add(SqlDbType.SmallInt, DbType.Int16);
            _toDbMappings.Add(SqlDbType.SmallMoney, DbType.Currency);
            //SqlDbType.Structured
            _toDbMappings.Add(SqlDbType.Text, DbType.String);
            _toDbMappings.Add(SqlDbType.Time, DbType.Time);
            // SqlDbType.Timestamp
            _toDbMappings.Add(SqlDbType.TinyInt, DbType.Byte);
            //SqlDbType.Udt
            _toDbMappings.Add(SqlDbType.UniqueIdentifier, DbType.Guid);
            _toDbMappings.Add(SqlDbType.VarBinary, DbType.Binary);
            _toDbMappings.Add(SqlDbType.VarChar, DbType.String);
            //SqlDbType.Variant
            _toDbMappings.Add(SqlDbType.Xml, DbType.Xml);
        }

        private static Dictionary<Type, SqlColumnType> _toSqlMappings = new Dictionary<Type, SqlColumnType>();
        private static Dictionary<SqlColumnType, Type> _toCMappings = new Dictionary<SqlColumnType, Type>();
        private static Dictionary<SqlDbType, DbType> _toDbMappings = new Dictionary<SqlDbType, DbType>();

        private const int MAX_STRING_LENGTH = 4000;

        public static SqlColumnType GetSqlType(Type dotNetType)
        {
            if (dotNetType == null)
                return new SqlColumnType(SqlDbType.NVarChar, true, 100);
            
            // For enumerations, map to ints
            if (dotNetType.IsEnum)
                return new SqlColumnType(SqlDbType.Int);
            
            if (!_toSqlMappings.ContainsKey(dotNetType))
                throw new NotImplementedException("Don't know how to map type '" + dotNetType.Name + "' to a sql type");

            return _toSqlMappings[dotNetType];
        }

        public static SqlColumnType GetSqlType(object val)
        {
            if (val == null)
                return new SqlColumnType(SqlDbType.NVarChar, true, 100);
            if (val == DBNull.Value)
                return new SqlColumnType(SqlDbType.NVarChar, true, 100);
            
            var type = val.GetType();
            var res = GetSqlType(type);
            var length = GetAppropriateLength(val) ?? res.Length;
            var precision = GetAppropriatePrecision(val) ?? res.Precision;
            var scale = GetAppropriateScale(val) ?? res.Scale;

            return new SqlColumnType(res.SqlType, res.IsNullable, length, precision, scale);
        }

        public static DbType GetDbType(SqlDbType sqlDbType)
        {
            if (!_toDbMappings.ContainsKey(sqlDbType))
                throw new NotImplementedException("Don't know how to map SqlDbType '" + sqlDbType + "' to a DbType");
            return _toDbMappings[sqlDbType];
        }

        public static int? GetAppropriateLength(object val)
        {
            // Hack - if this is a string type, we have to figure out how long a column to make
            // This is largely guesswork
            if (val is string)
            {
                var strLen = (val as string).Length;
                if (strLen > 30)
                    return MAX_STRING_LENGTH;
                else
                    return 100;
            }

            return null;
        }

        public static int? GetAppropriateScale(object val)
        {
            // TODO: Something intelligent here, rather than just assuming 12 is enough
            if (val is decimal)
                return 12;

            return null;
        }

        public static int? GetAppropriatePrecision(object val)
        {
            // TODO: Something intelligent here, rather than just assuming 36 is enough
            if (val is decimal)
                return 36;
            return null;
        }

        /// <summary>
        /// If we have a null value we need to return DBNull for the Sql query, other types are converted automatically.
        /// </summary>
        public static object ConvertToSqlValue(object obj)
        {
            if (obj == null)
                return DBNull.Value;
            return obj;
        }

        /// <summary>
        /// Figure out the SQL type signified by the supplied type name
        /// </summary>
        /// <param name="sqlTypeName"></param>
        /// <returns></returns>
        public static SqlDbType ParseDbType(string sqlTypeName)
        {
            // Hack because microsoft doesn't correctly take this into consideration
            if (sqlTypeName == "numeric")
                sqlTypeName = "decimal";
            // MySql has a longtext type, which we'll convert to nvarchar
            if (sqlTypeName == "longtext")
                sqlTypeName = "nvarchar";

            return (SqlDbType)Enum.Parse(typeof(SqlDbType), sqlTypeName, true);
        }

        /// <summary>
        /// Convert to the equivalent .NET type
        /// </summary>
        /// <param name="sqlTypeName"></param>
        /// <returns></returns>
        public static Type GetDotNetType(SqlColumnType ct)
        {
            var mapped = _toCMappings.Where(kv => kv.Key.SqlType == ct.SqlType && kv.Key.IsNullable == ct.IsNullable);
            if (!mapped.Any())
                throw new NotImplementedException("Don't know how to convert SqlColumnType '" + ct.Print() + "' to a .Net type");

            return mapped.First().Value;
        }
    }

    public static class SqlTypeExtensions
    {
        public static Dictionary<string, SqlColumnType> _SqlFieldTypes(this object obj)
        {
            var type = obj.GetType();
            return _SqlFieldTypes(type);
        }

        public static Dictionary<string, SqlColumnType> _SqlFieldTypes(this Type type)
        {
            MemberTypes types = MemberTypes.Property | MemberTypes.Field;
            BindingFlags bind = BindingFlags.Instance | BindingFlags.Public;

            var props = type.GetMembers(bind).Where(m => types.HasFlag(m.MemberType));
            var res = props.ToDictionary(m => m.Name, _getSqlType);

            return res;
        }

        private static SqlColumnType _getSqlType(MemberInfo mi)
        {
            // See if there's any SqlTypeAttribute on the mi
            // If so, pull the info from there.
            var att = mi.GetCustomAttributes(typeof(SqlTypeAttribute), true)
                .FirstOr(new SqlTypeAttribute()) as SqlTypeAttribute;

            var baseType = SqlTypeConversion.GetSqlType(mi.ReturnType());

            // For anything that's missing, get the assumed type info
            var res = new SqlColumnType(
                att.Type ?? baseType.SqlType,
                att.IsNullable ?? baseType.IsNullable,
                att.Length ?? baseType.Length,
                att.Precision ?? baseType.Precision,
                att.Scale ?? baseType.Scale);

            return res;
        }
    }
}
