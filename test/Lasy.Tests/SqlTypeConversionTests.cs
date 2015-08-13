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
            Assert.IsNotNull(SqlTypeConversion.GetSqlType(null));
        }

        private void isSqlType<T>(SqlDbType sqlType, bool isNullable = false, int? length = null, int? precision = null, int? scale = null)
        {
            var expected = new SqlColumnType(sqlType, isNullable, length, precision, scale);
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
            isSqlType<decimal>(SqlDbType.Decimal, precision: 36, scale: 12);
            isSqlType<decimal?>(SqlDbType.Decimal, true, precision: 36, scale: 12);
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

        [Test]
        public void HandlesNumeric()
        {
            Assert.AreEqual(SqlDbType.Decimal, SqlTypeConversion.ParseDbType("numeric"));
        }

        [Test]
        public void DetectsLongStrings()
        {
            var types = new TestObj()._SqlFieldTypes();
            Assert.AreEqual(1, types.Count());
            Assert.AreEqual("([StrVal,nvarchar(4000) NOT NULL])", types.Print());

        }

        [Test]
        public void DetectsXLongStrings()
        {
            var types = new XLTestObj()._SqlFieldTypes();
            Assert.AreEqual(1, types.Count());
            Assert.AreEqual("([XLongStrVal,nvarchar(8000) NOT NULL])", types.Print());
        }

        public class TestObj
        {
            [SqlType(false, 4000)]
            public string StrVal;
        }

        public class XLTestObj
        {
            [SqlType(false, 8000)]
            public string XLongStrVal;
        }
    }
}
