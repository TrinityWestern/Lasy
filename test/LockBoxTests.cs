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
    public class LockBoxTests
    {
        protected object _criteria
        {
            get
            {
                return new { ShouldProcess = true };
            }
        }

        protected FakeDB _setup(FakeDB db = null)
        {
            db = db ?? new FakeDB();
            db.Insert("Tbl", new { ShouldProcess = true, LockId = DBNull.Value, LockDate = DBNull.Value });
            return db;
        }

        [Test]
        public void GeneratesUniqueLocks()
        {
            var db = _setup();
            var locks = 1.To(10).Select(i => new LockBox(db, "Tbl", new { }).LockId);

            Assert.False(locks.Any(l => l.IsNullOrEmpty()), "None of the locks should have been empty");
            Assert.False(locks.AreAllEqual(), "Locks should all have been different, but were: " + locks.Print());
        }

        [Test]
        public void LocksRows()
        {
            var db = _setup();

            var rows = new LockBox(db, "Tbl", _criteria);
            var rowLocks = rows.Select(r => r["LockId"].ToString());
            foreach (var rowLock in rowLocks)
                Assert.AreEqual(rows.LockId, rowLock, "Expected each row to have the lockId set");

            var fromDb = db.Read("Tbl", _criteria);
            var dbLocks = fromDb.Select(r => r["LockId"].ToString());
            foreach (var dbLock in dbLocks)
                Assert.AreEqual(rows.LockId, dbLock, "Expected each database row to have the lockId set");
        }

        [Test]
        public void SetsLockDate()
        {
            var db = _setup();

            var rows = new LockBox(db, "Tbl", _criteria);
            Assert.NotNull(rows.Single()["LockDate"]);

            var fromDb = db.Read("Tbl", _criteria);
            Assert.NotNull(fromDb.Single()["LockDate"]);
        }

        [Test]
        public void WontLockLockedRows()
        {
            var db = _setup();

            var rows = new LockBox(db, "Tbl", _criteria);
            Assert.AreNotEqual(0, rows.Count(), "We should have found rows on the first lock");

            var rows2 = new LockBox(db, "Tbl", _criteria);
            Assert.AreEqual(0, rows2.Count(), "We shouldn't have found any rows on the second lock");
        }

        [Test]
        public void UsingBlockClearsLock()
        {
            var db = _setup();

            using (var rows = new LockBox(db, "Tbl", _criteria))
            {
                Assert.AreNotEqual(0, rows.Count(), "We should have found rows");
            }

            var rows2 = new LockBox(db, "Tbl", _criteria);
            Assert.AreNotEqual(0, rows2.Count(), "The old lock went out of scope, so we should find rows here - they should be unlocked");
        }

        [Test(Description="Once the box has been unlocked, its contents should have escaped - there should be nothing inside anymore")]
        public void UnlockingEmptiesBox()
        {
            var db = _setup();
            var box = new LockBox(db, "Tbl", _criteria);
            Assert.True(box.Any(), "The box should not have been empty");
            box.Unlock();
            Assert.False(box.Any(), "The box should have been empty after unlocking it");
        }

        [Test]
        public void CachesContents()
        {
            var db = new UnreliableDb();
            _setup(db);
            var box = new LockBox(db, "Tbl", _criteria);
            Assert.True(box.Any(), "The box should not have been empty");
            // Now, we'll make the database blow up if it gets accessed again
            db.FailOnNextOp = true;
            // When we access the box again, it shouldn't make the db explode
            Assert.True(box.Any(), "This should not blow up the box");
        }
    }
}
