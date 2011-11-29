using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;
using Microsoft.SqlServer.Server;
using Nvelope;


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
            _toSqlMappings.Add(typeof(char?), new SqlColumnType(SqlDbType.Char, true, 1));
            _toSqlMappings.Add(typeof(char), new SqlColumnType(SqlDbType.Char, false, 1));
            _toSqlMappings.Add(typeof(int?), new SqlColumnType(SqlDbType.Int, true));
            _toSqlMappings.Add(typeof(int), new SqlColumnType(SqlDbType.Int));
            _toSqlMappings.Add(typeof(float?), new SqlColumnType(SqlDbType.Real, true));
            _toSqlMappings.Add(typeof(float), new SqlColumnType(SqlDbType.Real));
            _toSqlMappings.Add(typeof(double?), new SqlColumnType(SqlDbType.Float, true));
            _toSqlMappings.Add(typeof(double), new SqlColumnType(SqlDbType.Float));
            _toSqlMappings.Add(typeof(decimal?), new SqlColumnType(SqlDbType.Decimal, true));
            _toSqlMappings.Add(typeof(decimal), new SqlColumnType(SqlDbType.Decimal));
            _toSqlMappings.Add(typeof(DateTime?), new SqlColumnType(SqlDbType.DateTime, true));
            _toSqlMappings.Add(typeof(DateTime), new SqlColumnType(SqlDbType.DateTime));
            _toSqlMappings.Add(typeof(string), new SqlColumnType(SqlDbType.NVarChar, true, 100));
            _toSqlMappings.Add(typeof(XmlDocument), new SqlColumnType(SqlDbType.Xml, true));
            _toSqlMappings.Add(typeof(Guid?), new SqlColumnType(SqlDbType.UniqueIdentifier, true));
            _toSqlMappings.Add(typeof(Guid), new SqlColumnType(SqlDbType.UniqueIdentifier));

            // _toCMappings are just the inverses of the _toSqlMappings, plus a few overloads
            _toCMappings = _toSqlMappings.Invert();
            _toCMappings.Add(new SqlColumnType(SqlDbType.NVarChar), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.VarChar, true), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.VarChar), typeof(string));
            _toCMappings.Add(new SqlColumnType(SqlDbType.Xml), typeof(XmlDocument));
        }

        private static Dictionary<Type, SqlColumnType> _toSqlMappings = new Dictionary<Type, SqlColumnType>();
        private static Dictionary<SqlColumnType, Type> _toCMappings = new Dictionary<SqlColumnType, Type>();

        private const int MAX_STRING_LENGTH = 4000;

        public static SqlColumnType GetSqlType(Type dotNetType)
        {
            if (dotNetType == null)
                return new SqlColumnType(SqlDbType.NVarChar, true, 100);
            
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

            return new SqlColumnType(res.Type, res.IsNullable, length);
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
            return (SqlDbType)Enum.Parse(typeof(SqlDbType), sqlTypeName, true);
        }

        /// <summary>
        /// Convert to the equivalent .NET type
        /// </summary>
        /// <param name="sqlTypeName"></param>
        /// <returns></returns>
        public static Type GetDotNetType(SqlColumnType ct)
        {
            var mapped = _toCMappings.Where(kv => kv.Key.Type == ct.Type && kv.Key.IsNullable == ct.IsNullable);
            if (!mapped.Any())
                throw new NotImplementedException("Don't know how to convert SqlColumnType '" + ct.Print() + "' to a .Net type");

            return mapped.First().Value;
        }
    }
}
