using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;
using Nvelope;
using Nvelope.Reflection;

namespace LasyTests
{
    public abstract class EventTests
    {
        public abstract IRWEvented _getDb();

        [Test]
        public void OnInsert()
        {
            var db = TestEnv.SetupInsert(_getDb());
            // When we do an insert, it should increment the counter
            var count = 0;
            db.OnInsert += (x, y) => ++count;

            db.Insert(TestEnv.Table, TestEnv.Keys);

            Assert.AreEqual(1, count,
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnInsertTransaction()
        {
            var db = TestEnv.SetupInsert(_getDb());
            // When we do an insert, it should increment the counter
            var count = 0;
            db.OnInsert += (x, y) => ++count;

            if (!(db is ITransactable))
                return;
            
            var tdb = db as ITransactable;

            var trans = tdb.BeginTransaction();
            trans.Insert(TestEnv.Table, TestEnv.Keys);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }

        [Test]
        public void OnUpdate()
        {
            var db = TestEnv.SetupUpdate(_getDb());
            var count = 0;
            db.OnUpdate += (x, y, z) => ++count;

            db.Update(TestEnv.Table, TestEnv.Keys, TestEnv.UpdatedRow);

            Assert.AreEqual(1, count,
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnUpdateTransaction()
        {
            var db = TestEnv.SetupUpdate(_getDb());
            var count = 0;
            db.OnUpdate += (x, y, z) => ++count;

            if (!(db is ITransactable))
                return;

            var tdb = db as ITransactable;

            var trans = tdb.BeginTransaction();
            trans.Update(TestEnv.Table, TestEnv.Keys, TestEnv.UpdatedRow);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }

        [Test]
        public void OnDelete()
        {
            var db = TestEnv.SetupDelete(_getDb());
            var count = 0;
            db.OnDelete += (x,y) => ++count;

            db.Delete(TestEnv.Table, TestEnv.Keys);

            Assert.AreEqual(1, count,
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnDeleteTransaction()
        {
            var db = TestEnv.SetupDelete(_getDb());
            var count = 0;
            db.OnDelete += (x,y) => ++count;

            if (!(db is ITransactable))
                return;

            var tdb = db as ITransactable;

            var trans = tdb.BeginTransaction();
            trans.Delete(TestEnv.Table, TestEnv.Keys);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }

        [Test]
        public void OnRead()
        {
            var db = TestEnv.SetupUpdate(_getDb());
            var count = 0;
            db.OnRead += (x,y) => ++count;

            db.Read(TestEnv.Table, TestEnv.Keys);

            Assert.AreEqual(1, count, 
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnReadTransaction()
        {
            var db = TestEnv.SetupUpdate(_getDb());
            var count = 0;
            db.OnRead += (x, y) => ++count;

            if (!(db is ITransactable))
                return;

            var tdb = db as ITransactable;

            var trans = tdb.BeginTransaction();
            trans.Read(TestEnv.Table, TestEnv.Keys);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }
    }
}
