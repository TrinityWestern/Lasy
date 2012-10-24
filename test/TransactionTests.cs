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
    public class TransactionTests<T> : AbstractTableTests<T> where T: ITransactable, new()
    {
        [Test]
        public void RollbackInsert()
        {
            var db = _setupInsert();
            var trans = db.BeginTransaction();
            trans.Insert(_table, _row);
            trans.Rollback();
            // The row shouldn't exist anymore
            _assertGone(db);
        }

        [Test]
        public void CommitInsert()
        {
            var db = _setupInsert();
            var trans = db.BeginTransaction();
            trans.Insert(_table, _row);
            trans.Commit();
            // The row should be in the db now
            _assertHas(db, _row);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsoloateInsert()
        {
            var db = _setupInsert();
            var trans = db.BeginTransaction();
            trans.Insert(_table, _row);
            // At this point, before we commit the transaction, the change should't be visible outside the transaction
            // But it should be visible inside the transaction
            _assertGone(db);
            _assertHas(trans, _row);

            trans.Commit();
        }

        [Test]
        public void RollbackUpdate()
        {
            var db = _setupUpdate();
            var trans = db.BeginTransaction();
            trans.Update(_table, _updatedRow, _keys);
            trans.Rollback();
            // The row be in it's original state
            _assertHas(db, _row);
        }

        [Test]
        public void CommitUpdate()
        {
            var db = _setupUpdate();
            var trans = db.BeginTransaction();
            trans.Update(_table, _updatedRow, _keys);
            trans.Commit();
            // The row should now be updated
            _assertHas(db, _updatedRow);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsolateUpdate()
        {
            var db = _setupUpdate();
            var trans = db.BeginTransaction();
            trans.Update(_table, _updatedRow, _keys);
            // At this point, the row should be updated in the transaction, but not outside of it
            _assertHas(trans, _updatedRow);
            _assertHas(db, _row);

            trans.Commit();
        }

        [Test]
        public void RollbackDelete()
        {
            var db = _setupDelete();
            var trans = db.BeginTransaction();
            trans.Delete(_table, _keys);
            trans.Rollback();
            // The row should still exist
            _assertHas(db, _row);
        }

        [Test]
        public void CommitDelete()
        {
            var db = _setupDelete();
            var trans = db.BeginTransaction();
            trans.Delete(_table, _keys);
            trans.Commit();
            // The row should be gone
            _assertGone(db);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsolateDelete()
        {
            var db = _setupDelete();
            var trans = db.BeginTransaction();
            trans.Delete(_table, _keys);
            // At this point, the row should be deleted in the transaction, but not outside it
            _assertGone(trans);
            _assertHas(db, _row);

            trans.Commit();
        }

    }
}
