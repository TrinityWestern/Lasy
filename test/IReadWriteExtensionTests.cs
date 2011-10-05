using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;
using Nvelope;

namespace LasyTests
{
    [TestFixture]
    public class IReadWriteExtensionTests
    {
        [Test]
        public void Ensure()
        {
            var db = new FakeDB();
            var data = new { MyTableId = 1, Foo = "bar"};
            // This should do an insert
            db.Ensure("MyTable", data);
            Assert.AreEqual("(([Foo,bar],[MyTableId,1]))", db.Table("MyTable").Print());

            var newData = new { MyTableId = 1, Foo = "sums" };
            // This should do an update
            db.Ensure("MyTable", newData);
            Assert.AreEqual("(([Foo,sums],[MyTableId,1]))", db.Table("MyTable").Print());
        }
    }
}
