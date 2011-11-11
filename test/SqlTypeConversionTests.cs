using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;
using System.Data;
using System.Xml;
using Nvelope;

namespace LasyTests
{
    [TestFixture]
    public class SqlTypeConversionTests
    {
        [Test(Description = "Check if we can pass null values into InferSqlType")]
        public void InferNull()
        {
            Assert.IsNotNull(SqlTypeConversion.InferSqlType(null));
        }

        private void isSqlType<T>(SqlDbType sqlType, bool isNullable = false, int? length = null)
        {
            var expected = new SqlColumnType(sqlType, isNullable, length);
            Assert.AreEqual(expected.Print(), SqlTypeConversion.GetSqlType(typeof(T)).Print());
        }

        [Test]
        public void GetSqlType()
        {
            isSqlType<bool>(SqlDbType.Bit);
            isSqlType<bool?>(SqlDbType.Bit, true);
            isSqlType<char?>(SqlDbType.Char, true, 1);
            isSqlType<char>(SqlDbType.Char, false, 1);
            isSqlType<string>(SqlDbType.NVarChar, true, 100);
            isSqlType<DateTime>(SqlDbType.DateTime);
            isSqlType<DateTime?>(SqlDbType.DateTime, true);
            isSqlType<double>(SqlDbType.Float);
            isSqlType<double?>(SqlDbType.Float, true);
            isSqlType<float>(SqlDbType.Real);
            isSqlType<float?>(SqlDbType.Real, true);
            isSqlType<int>(SqlDbType.Int);
            isSqlType<int?>(SqlDbType.Int, true);
            isSqlType<decimal>(SqlDbType.Decimal);
            isSqlType<decimal?>(SqlDbType.Decimal, true);
            isSqlType<Guid>(SqlDbType.UniqueIdentifier);
            isSqlType<Guid?>(SqlDbType.UniqueIdentifier, true);
            isSqlType<XmlDocument>(SqlDbType.Xml, true);
        }

        private void isNetType<T>(SqlDbType sqlType, bool isNullable = false, int? length = null)
        {
            Assert.AreEqual(typeof(T), SqlTypeConversion.GetDotNetType(
                new SqlColumnType(sqlType, isNullable, length)));
        }

        [Test]
        public void GetDotNetType()
        {
            isNetType<bool>(SqlDbType.Bit);
            isNetType<bool?>(SqlDbType.Bit, true);
            isNetType<char?>(SqlDbType.Char, true);
            isNetType<char>(SqlDbType.Char);
            isNetType<string>(SqlDbType.NVarChar, false, 100);
            isNetType<string>(SqlDbType.NVarChar, true, 100);
            isNetType<DateTime>(SqlDbType.DateTime);
            isNetType<DateTime?>(SqlDbType.DateTime, true);
            isNetType<double>(SqlDbType.Float);
            isNetType<double?>(SqlDbType.Float, true);
            isNetType<float>(SqlDbType.Real);
            isNetType<float?>(SqlDbType.Real, true);
            isNetType<int>(SqlDbType.Int);
            isNetType<int?>(SqlDbType.Int, true);
            isNetType<decimal>(SqlDbType.Decimal);
            isNetType<decimal?>(SqlDbType.Decimal, true);
            isNetType<Guid>(SqlDbType.UniqueIdentifier);
            isNetType<Guid?>(SqlDbType.UniqueIdentifier, true);
            isNetType<XmlDocument>(SqlDbType.Xml);
            isNetType<XmlDocument>(SqlDbType.Xml, true);
        }
    }
}
