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

        protected FakeDB _setup()
        {
            var db = new FakeDB();
            db.Insert("Tbl", new { ShouldProcess = true, LockId = DBNull.Value, LockDate = DBNull.Value });
            return db;
        }

        [Test]
        public void GeneratesUniqueLocks()
        {
            var db = _setup();
            var locks = 1.To(10).Select(i => new LockBox(db, "Tbl").LockId);

            Assert.False(locks.Any(l => l.IsNullOrEmpty()), "None of the locks should have been empty");
            Assert.False(locks.AreAllEqual(), "Locks should all have been different, but were: " + locks.Print());
        }

        [Test]
        public void LocksRows()
        {
            var db = _setup();


            var lb = new LockBox(db, "Tbl");
            var rows = lb.LockRead(_criteria);
            var rowLocks = rows.Select(r => r["LockId"].ToString());
            foreach (var rowLock in rowLocks)
                Assert.AreEqual(lb.LockId, rowLock, "Expected each row to have the lockId set");

            var fromDb = db.Read("Tbl", _criteria);
            var dbLocks = fromDb.Select(r => r["LockId"].ToString());
            foreach (var dbLock in dbLocks)
                Assert.AreEqual(lb.LockId, dbLock, "Expected each database row to have the lockId set");
        }

        [Test]
        public void SetsLockDate()
        {
            var db = _setup();

            var lb = new LockBox(db, "Tbl");
            var rows = lb.LockRead(_criteria);
            Assert.NotNull(rows.Single()["LockDate"]);

            var fromDb = db.Read("Tbl", _criteria);
            Assert.NotNull(fromDb.Single()["LockDate"]);
        }


        [Test]
        public void WontLockLockedRows()
        {
            var db = _setup();

            var lb = new LockBox(db, "Tbl");
            var rows = lb.LockRead(_criteria);
            Assert.AreNotEqual(0, rows.Count(), "We should have found rows on the first lock");

            var lb2 = new LockBox(db, "Tbl");
            var rows2 = lb2.LockRead(_criteria);
            Assert.AreEqual(0, rows2.Count(), "We shouldn't have found any rows on the second lock");
        }

        [Test]
        public void UsingBlockClearsLock()
        {
            var db = _setup();

            using (var lb = new LockBox(db, "Tbl"))
            {
                var rows = lb.LockRead(_criteria);
                Assert.AreNotEqual(0, rows.Count(), "We should have found rows");
            }

            var lb2 = new LockBox(db, "Tbl");
            var rows2 = lb2.LockRead(_criteria);
            Assert.AreNotEqual(0, rows2.Count(), "The old lock went out of scope, so we should find rows here - they should be unlocked");
        }
    }
}
