using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;
using Nvelope;
using Nvelope.Reflection;

namespace LasyTests.Sql
{
    [TestFixture]
    public class MySqlTests
    {
        protected string _connStr = "Server=localhost;Database=lasytest;Uid=lasy;Pwd=abc123;";

        [Test]
        public void Read()
        {
            var db = ConnectTo.MySql(_connStr);
            var res = db.ReadAll("House");
            Assert.True(res.Any());
        }

        [Test]
        public void Insert()
        {
            var db = ConnectTo.MySql(_connStr);
            var obj = new { Number = 1, Street = "Main"};
            var keys = db.Insert("House", obj);
            var fromDb = db.Read("House", keys).Single();
            Assert.True(obj._AsDictionary().IsSameAs(fromDb));
        }

        [Test]
        public void Update()
        {
            var db = ConnectTo.MySql(_connStr);
            var obj = new { Number = 1, Street = "Main" };
            var keys = db.Insert("House", obj);
            var newObj = obj._AsDictionary().Assoc("Number", 2);
            db.Update("House", newObj, keys);
            var fromDb = db.Read("House", keys).Single();
            Assert.True(newObj.IsSameAs(fromDb));
        }

        [Test]
        public void Delete()
        {
            var db = ConnectTo.MySql(_connStr);
            var obj = new { Number = 1, Street = "Main" };
            var keys = db.Insert("House", obj);
            Assert.True(db.Read("House", keys).Any());
            db.Delete("House", keys);
            Assert.False(db.Read("House", keys).Any());
        }
    }
}
