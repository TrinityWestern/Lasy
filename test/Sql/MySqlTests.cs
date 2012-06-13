using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;

namespace LasyTests.Sql
{
    [TestFixture]
    public class MySqlTests
    {
        [Test]
        public void ConnectstoMySQL()
        {
            var db = ConnectTo.MySql("Server=localhost:3306;Database=lasytest;Uid=lasy;Pwd=abc123;");
            var res = db.ReadAll("House");
            Assert.True(res.Any());
        }

        [Test]
        public void Read()
        {
            Assert.Fail();
        }

        [Test]
        public void Insert()
        {
            Assert.Fail();
        }

        [Test]
        public void Update()
        {
            Assert.Fail();
        }

        [Test]
        public void Delete()
        {
            Assert.Fail();
        }
    }
}
