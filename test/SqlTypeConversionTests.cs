using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;

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
    }
}
