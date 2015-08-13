using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nvelope;
using Lasy;

namespace LasyTests.IntegrationTests
{
    [TestFixture]
    public class MySql
    {
        [Test]
        public void ClassesMd_User_read()
        {
            var db = ConnectTo.MySql(Config.TestMySqlConnectionString);
            var res = db.Read("classesmd_user", null).First();
            Assert.NotNull(res);
        }

        [Test]
        public void ClassesMd_User_write()
        {
            var db = ConnectTo.MySql(Config.TestMySqlConnectionString);
            var res = db.Read("classesmd_user", null).First();
            var key = res.Only("id");
            db.Update("classesmd_user", res, key);
            var updated = db.Read("classesmd_user", key);
        }
    }
}
