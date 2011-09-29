using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml;

namespace Lasy
{
    /// <summary>
    /// Conversion to and from SQL data types
    /// </summary>
    public class SqlTypeConversion
    {
        /// <summary>
        /// Figure out what the SQL type of the supplied object would be
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static SqlDbType InferSqlType(object obj)
        {
            if (obj == null)
                return SqlDbType.VarChar;
            if (obj == DBNull.Value)
                return SqlDbType.VarChar;
            return Microsoft.SqlServer.Server.SqlMetaData.InferFromValue(obj, "").SqlDbType;
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
        public static Type GetDotNetType(string sqlTypeName)
        {
            var type = ParseDbType(sqlTypeName);
            // Should implement this table when done: http://msdn.microsoft.com/en-us/library/cc716729.aspx
            switch (type)
            {
                case (SqlDbType.Bit):
                    return typeof(bool);
                case (SqlDbType.Char):
                case (SqlDbType.NChar):
                case (SqlDbType.NText):
                case (SqlDbType.NVarChar):
                case (SqlDbType.Text):
                case (SqlDbType.VarChar):
                    return typeof(string);
                case (SqlDbType.Date):
                case (SqlDbType.DateTime):
                case (SqlDbType.SmallDateTime):
                    return typeof(DateTime);
                case (SqlDbType.Decimal):
                case (SqlDbType.Money):
                case (SqlDbType.SmallMoney):
                    return typeof(decimal);
                case (SqlDbType.Float):
                    return typeof(double);
                case (SqlDbType.Int):
                    return typeof(int);
                case (SqlDbType.Real):
                    return typeof(float);
                case (SqlDbType.Xml):
                    return typeof(XmlDocument);
                case (SqlDbType.UniqueIdentifier):
                    return typeof(Guid);
                default:
                    throw new Exception("Unsupported type: '" + type.ToString() + "'");
            }
        }


    }
}
