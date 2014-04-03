using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;

namespace LasyTests.Sql
{
    public class ModifyableMySqlTests
    {
        [Test]
        public void Read()
        {
            var db = ConnectTo.ModifiableMySql(Config.TestMySqlConnectionString);
            var res = db.ReadAll("House");
            Assert.True(res.Any());
        }
    }
}
