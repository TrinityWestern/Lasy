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
    /// <summary>
    /// Makes sure transactions work properly on the database.
    /// To implement, create a class that instantiates this generic class
    /// and mark it with a [TestFixture] attribute. See FakeDBTransactionTests
    /// for an example
    /// </summary>
    /// <typeparam name="TDb"></typeparam>
    public class TransactionTests<T> where T: ITransactable, new()
    {
        /// <summary>
        /// Create a new db
        /// </summary>
        /// <returns></returns>
        protected virtual T _newDb()
        {
            return new T();
        }

        /// <summary>
        /// The name of the table we'll be testing on
        /// </summary>
        protected virtual string _table { get { return "Tbl"; } }
        /// <summary>
        /// The row we'll either insert or delete
        /// </summary>
        protected virtual Dictionary<string, object> _row
        {
            get
            {
                return new { Foo = "bar", Val = 7 }._AsDictionary();
            }
        }
        /// <summary>
        /// The contents of the row after we do an update
        /// </summary>
        protected virtual Dictionary<string, object> _updatedRow
        {
            get
            {
                return _row.Assoc("Val", 42);
            }
        }

        /// <summary>
        /// Returns the keys to the row, used to retrieve it from the db. 
        /// Note: Doesn't need to be the primary key of the row, just some field
        /// that is unique to the test row and doesn't change between _row and 
        /// _updatedRow
        /// </summary>
        protected virtual Dictionary<string, object> _keys
        {
            get
            {
                var diff = _row.Diff(_updatedRow);
                return _row.Except(diff.Keys);
            }
        }

        /// <summary>
        /// Setup the db for inserts - just return a new db generally
        /// </summary>
        /// <returns></returns>
        protected virtual T _setupInsert()
        {
            return _newDb();
        }

        /// <summary>
        /// Setup the db for updates - ie insert _row into it
        /// </summary>
        /// <returns></returns>
        protected virtual T _setupUpdate()
        {
            var db = _newDb();
            db.Insert(_table, _row);
            return db;
        }

        /// <summary>
        /// Setup the db for deletes - ie insert _row into it
        /// </summary>
        /// <returns></returns>
        protected virtual T _setupDelete()
        {
            var db = _newDb();
            db.Insert(_table, _row);
            return db;
        }

        /// <summary>
        /// Make sure the db contains the row supplied
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trans">If supplied read using the transaction</param>
        /// <param name="contents"></param>
        protected void _assertHas(IReadWrite db, Dictionary<string, object> contents, ITransaction trans = null)
        {
            var rows = db.Read(_table, _keys, trans);
            Assert.AreEqual(1, rows.Count(), "Expected the row to be in the database");
            var exceptAutokey = rows.Single().Except(db.Analyzer.GetAutoNumberKey(_table));
            Assert.AreEqual(contents.Print(), exceptAutokey.Print(), "The row was there, but not with the values we expected");
        }

        /// <summary>
        /// Make sure there's no row in the database 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trans"></param>
        protected void _assertGone(IReadWrite db, ITransaction trans = null)
        {
            Assert.False(db.Read(_table, _keys, trans).Any(), "Expected there to be no row in the database");
        }

        [Test]
        public void RollbackInsert()
        {
            var db = _setupInsert();
            var trans = db.BeginTransaction();
            db.Insert(_table, _row, trans);
            trans.Rollback();
            // The row shouldn't exist anymore
            _assertGone(db);
        }

        [Test]
        public void CommitInsert()
        {
            var db = _setupInsert();
            var trans = db.BeginTransaction();
            db.Insert(_table, _row, trans);
            trans.Commit();
            // The row should be in the db now
            _assertHas(db, _row);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsoloateInsert()
        {
            var db = _setupInsert();
            var trans = db.BeginTransaction();
            db.Insert(_table, _row, trans);
            // At this point, before we commit the transaction, the change should't be visible outside the transaction
            // But it should be visible inside the transaction
            _assertGone(db);
            _assertHas(db, _row, trans);

            trans.Commit();
        }

        [Test]
        public void RollbackUpdate()
        {
            var db = _setupUpdate();
            var trans = db.BeginTransaction();
            db.Update(_table, _updatedRow, _keys, trans);
            trans.Rollback();
            // The row be in it's original state
            _assertHas(db, _row);
        }

        [Test]
        public void CommitUpdate()
        {
            var db = _setupUpdate();
            var trans = db.BeginTransaction();
            db.Update(_table, _updatedRow, _keys, trans);
            trans.Commit();
            // The row should now be updated
            _assertHas(db, _updatedRow);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsolateUpdate()
        {
            var db = _setupUpdate();
            var trans = db.BeginTransaction();
            db.Update(_table, _updatedRow, _keys, trans);
            // At this point, the row should be updated in the transaction, but not outside of it
            _assertHas(db, _updatedRow, trans);
            _assertHas(db, _row);

            trans.Commit();
        }

        [Test]
        public void RollbackDelete()
        {
            var db = _setupDelete();
            var trans = db.BeginTransaction();
            db.Delete(_table, _keys, trans);
            trans.Rollback();
            // The row should still exist
            _assertHas(db, _row);
        }

        [Test]
        public void CommitDelete()
        {
            var db = _setupDelete();
            var trans = db.BeginTransaction();
            db.Delete(_table, _keys, trans);
            trans.Commit();
            // The row should be gone
            _assertGone(db);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsolateDelete()
        {
            var db = _setupDelete();
            var trans = db.BeginTransaction();
            db.Delete(_table, _keys, trans);
            // At this point, the row should be deleted in the transaction, but not outside it
            _assertGone(db, trans);
            _assertHas(db, _row);

            trans.Commit();
        }
    }
}
