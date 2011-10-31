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
    public class EventTests<T> : AbstractTableTests<T> where T: ITransactable, new()
    {
        [Test]
        public void OnInsert()
        {
            var db = _setupInsert().AddEvents();
            // When we do an insert, it should increment the counter
            var count = 0;
            db.OnInsert += (x, y) => ++count;

            db.Insert(_table, _keys);

            Assert.AreEqual(1, count,
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnInsertTransaction()
        {
            var db = _setupInsert().AddEvents();
            // When we do an insert, it should increment the counter
            var count = 0;
            db.OnInsert += (x, y) => ++count;

            var trans = db.BeginTransaction();
            trans.Insert(_table, _keys);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }

        [Test]
        public void OnUpdate()
        {
            var db = _setupUpdate().AddEvents();
            var count = 0;
            db.OnUpdate += (x, y, z) => ++count;

            db.Update(_table, _keys, _updatedRow);

            Assert.AreEqual(1, count,
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnUpdateTransaction()
        {
            var db = _setupUpdate().AddEvents();
            var count = 0;
            db.OnUpdate += (x, y, z) => ++count;

            var trans = db.BeginTransaction();
            trans.Update(_table, _keys, _updatedRow);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }

        [Test]
        public void OnDelete()
        {
            var db = _setupDelete().AddEvents();
            var count = 0;
            db.OnDelete += (x,y) => ++count;

            db.Delete(_table, _keys);

            Assert.AreEqual(1, count,
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnDeleteTransaction()
        {
            var db = _setupDelete().AddEvents();
            var count = 0;
            db.OnDelete += (x,y) => ++count;

            var trans = db.BeginTransaction();
            trans.Delete(_table, _keys);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }

        [Test]
        public void OnRead()
        {
            var db = _setupUpdate().AddEvents();
            var count = 0;
            db.OnRead += (x,y) => ++count;

            db.Read(_table, _keys);

            Assert.AreEqual(1, count, 
                "The operation did not fire the event on the database");
        }

        [Test]
        public void OnReadTransaction()
        {
            var db = _setupUpdate().AddEvents();
            var count = 0;
            db.OnRead += (x, y) => ++count;

            var trans = db.BeginTransaction();
            trans.Read(_table, _keys);
            trans.Commit();

            Assert.AreEqual(1, count,
                "The transaction did not fire the event on the database");
        }

        [Test]
        public void OnBeginTransaction()
        {
            var db = _setupInsert().AddEvents();
            var count = 0;
            db.OnBeginTransaction += () => ++count;

            db.BeginTransaction().Commit();

            Assert.AreEqual(1, count,
                "Starting a transaction did not fire the event");
        }
    }
}
