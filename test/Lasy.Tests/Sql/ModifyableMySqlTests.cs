using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;
using Nvelope.Configuration;

namespace LasyTests.Sql
{
    public class ModifyableMySqlTests
    {
        protected string _connStr
        {
            get
            {
                return Config.ConnectionString("mysqldb");
            }
        }

        [Test]
        public void Read()
        {
            var db = ConnectTo.ModifiableMySql(_connStr);
            var res = db.ReadAll("House");
            Assert.True(res.Any());
        }
    }
}
