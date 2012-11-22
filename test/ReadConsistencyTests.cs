using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;
using NUnit.Framework;

namespace LasyTests
{
    /// <summary>
    /// Makes sure that the IReadWrite implements IReadable methods correctly. To implement, 
    /// create a instantiation of this class for a particular IReadWrite subclass, and mark
    /// it with the [TestFixture] attribute. See FakeDbReadConsistency for example
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadConsistencyTests<T> where T: IReadWrite, new()
    {
        protected IReadWrite _newRw()
        {
            return new T();
        }

        [Test]
        public void RawReadReturnsCopiesOfDicts()
        {
            var db = _newRw();
            var row = new { A = 1, B = 2 };
            var id = db.Insert("Tbl", row);
            var fromDb = db.RawRead("Tbl", id).Single();
            // This shouldn't change the value in the DB
            fromDb["A"] = 7;
            Assert.AreEqual(1, db.RawRead("Tbl", id).Single()["A"]);
        }

        [Test]
        public void RawReadExecutesImmediately()
        {
            var db = _newRw();
            var row = new { A = 1, B = 2 };
            var id = db.Insert("Tbl", row);
            
            // This should execute immediately - it should be lazy evaluated
            // To test this, we'll modify the row before we attempt to look
            // at what we got back from the DB - it should still be the version we were
            // created the first insert, not the updated version
            var fromDb = db.RawRead("Tbl", id).Single();
            db.Update("Tbl", new { A = 7 }, id);
            Assert.AreEqual(1, fromDb["A"]);
        }

        [Test]
        public void RawReadAllReturnsCopiesOfDicts()
        {
            var db = _newRw();
            var row = new { A = 1, B = 2 };
            var id = db.Insert("Tbl", row);
            var fromDb = db.ReadAll("Tbl").Single();
            // This shouldn't change the value in the DB
            fromDb["A"] = 7;
            Assert.AreEqual(1, db.ReadAll("Tbl").Single()["A"]);
        }

        [Test]
        public void RawReadAllExecutesImmediately()
        {
            var db = _newRw();
            var row = new { A = 1, B = 2 };
            var id = db.Insert("Tbl", row);

            // This should execute immediately - it should be lazy evaluated
            // To test this, we'll modify the row before we attempt to look
            // at what we got back from the DB - it should still be the version we were
            // created the first insert, not the updated version
            var fromDb = db.ReadAll("Tbl").Single();
            db.Update("Tbl", new { A = 7 }, id);
            Assert.AreEqual(1, fromDb["A"]);
        }
    }
}
