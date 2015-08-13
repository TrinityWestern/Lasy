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

        [Test(Description="If we've got a data class that inherits from dictionary<string,object>, " +
            " we should be able to use the idiom of db.Read<T>(), even if we have fields that " +
            "might be null in the database. It's up to the dict-based class to handle that by " + 
            " providing default values for the fields. That's part of the appeal of using these " + 
            " dictionary-based data classes - they allow for incomplete specification")]
        public void ReadTCanSetFieldsFromNullForDictionaryBasedTypes()
        {
            var db = new FakeDB();
            var row = new Dictionary<string,object>{{"Name", "foosums"}};
            db.Insert("test", row);

            // Previously, the way we did this was to write this as:
            // obj = db.ReadAll("test").Select(r => new DictBasedTestObject()._SetFrom(r)).Single();
            var obj = db.Read<DictBasedTestObject>("test", row).Single();
            Assert.NotNull(obj);
            Assert.False(obj.IsSet);
            Assert.AreEqual("foosums", obj.Name);
        }

        [Test(Description = "If we've got a data class that inherits from dictionary<string,object>, " +
            " we should be able to use the idiom of db.Read<T>(), even if we have fields that " +
            "might be null in the database. It's up to the dict-based class to handle that by " +
            " providing default values for the fields. That's part of the appeal of using these " +
            " dictionary-based data classes - they allow for incomplete specification")]
        public void ReadAllTCanSetFieldsFromNullForDictionaryBasedTypes()
        {
            var db = new FakeDB();
            var row = new Dictionary<string, object> { { "Name", "foosums" } };
            db.Insert("test", row);

            // Previously, the way we did this was to write this as:
            // obj = db.ReadAll("test").Select(r => new DictBasedTestObject()._SetFrom(r)).Single();
            var obj = db.ReadAll<DictBasedTestObject>("test").Single();
            Assert.NotNull(obj);
            Assert.False(obj.IsSet);
            Assert.AreEqual("foosums", obj.Name);
        }
    }
}
