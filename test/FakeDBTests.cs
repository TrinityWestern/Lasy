using System;
using Nvelope;
using Nvelope.Reflection;
using Lasy;
using NUnit.Framework;

namespace LasyTests
{
    [TestFixture]
    public class FakeDBTests
    {
        [Test]
        public void InsertPK()
        {
            var db = new FakeDB();
            var data = new { Filename = "foosums", Created = DateTime.Now };
            var key = db.Insert("Investment.PNImport", data);

            var fromDb = db.ReadPK("Investment.PNImport", key);

            Assert.AreEqual(1, fromDb["PNImportId"]);
            Assert.AreEqual(data._AsDictionary().Assoc("PNImportId", 1).Print(), fromDb.Print());
        }

        [Test(Description = "Does it work to Read from a table that we've never mentioned before? - It should return nothing")]
        public void ReadAllFromNewTable()
        {
            var db = new FakeDB();
            Assert.AreEqual("()", db.RawReadAll("foosums").Print());
        }

        [Test(Description = "Does it work to Read from a table that we've never mentioned before? - It should return nothing")]
        public void ReadFromNewTable()
        {
            var db = new FakeDB();
            Assert.AreEqual("()", db.Read("foosums", new { a = "b" }).Print());
        }

        [Test]
        public void IncrementsPKs()
        {
            var db = new FakeDB();
            var data = new { Z = "A" };
            2.Times(() => db.Insert("Foosums", data));

            Assert.AreEqual("(([FoosumsId,1],[Z,A]),([FoosumsId,2],[Z,A]))", db.Table("Foosums").Print());            
        }

        [Test]
        public void UpdateWorks()
        {
            var db = new FakeDB();
            var data = new { Z = "A" };
            db.Insert("Foosums", data);
            var keys = db.Insert("Foosums", data);

            // Now, we'll update the second row, and the first should remain unchanged
            db.Update("Foosums", keys.Assoc("Z", "B"));

            Assert.AreEqual("(([FoosumsId,1],[Z,A]),([FoosumsId,2],[Z,B]))", db.Table("Foosums").Print());
        }

        [Test(Description="If insert 2 rows then delete one, make sure the next autokey is 3, not 2")]
        public void DoesntRepeatAutoKeys()
        {
            var db = new FakeDB();
            var data = new { Z = "A" };
            2.Times(() => db.Insert("P", data));
            db.Delete("P", new { PId = 2 });

            db.Insert("P", new {Z = "B"});
            Assert.AreEqual("(([PId,1],[Z,A]),([PId,3],[Z,B]))", db.Table("P").Print());
        }
        
    }
}
