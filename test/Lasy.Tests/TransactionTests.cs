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
    public abstract class TransactionTests
    {
        public abstract ITransactable _getDb();

        [Test]
        public void RollbackInsert()
        {
            var db = TestEnv.SetupInsert(_getDb());
            var trans = db.BeginTransaction();
            trans.Insert(TestEnv.Table, TestEnv.Row);
            trans.Rollback();
            // The row shouldn't exist anymore
            TestEnv.AssertGone(db);
        }

        [Test]
        public void CommitInsert()
        {
            var db = TestEnv.SetupInsert(_getDb());
            var trans = db.BeginTransaction();
            trans.Insert(TestEnv.Table, TestEnv.Row);
            trans.Commit();
            // The row should be in the db now
            TestEnv.AssertHas(db, TestEnv.Row);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsoloateInsert()
        {
            var db = TestEnv.SetupInsert(_getDb());
            var trans = db.BeginTransaction();
            trans.Insert(TestEnv.Table, TestEnv.Row);
            // At this point, before we commit the transaction, the change should't be visible outside the transaction
            // But it should be visible inside the transaction
            TestEnv.AssertGone(db);
            TestEnv.AssertHas(trans, TestEnv.Row);

            trans.Commit();
        }

        [Test]
        public void RollbackUpdate()
        {
            var db = TestEnv.SetupUpdate(_getDb());
            var trans = db.BeginTransaction();
            trans.Update(TestEnv.Table, TestEnv.UpdatedRow, TestEnv.Keys);
            trans.Rollback();
            // The row be in it's original state
            TestEnv.AssertHas(db, TestEnv.Row);
        }

        [Test]
        public void CommitUpdate()
        {
            var db = TestEnv.SetupUpdate(_getDb());
            var trans = db.BeginTransaction();
            trans.Update(TestEnv.Table, TestEnv.UpdatedRow, TestEnv.Keys);
            trans.Commit();
            // The row should now be updated
            TestEnv.AssertHas(db, TestEnv.UpdatedRow);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsolateUpdate()
        {
            var db = TestEnv.SetupUpdate(_getDb());
            var trans = db.BeginTransaction();
            trans.Update(TestEnv.Table, TestEnv.UpdatedRow, TestEnv.Keys);
            // At this point, the row should be updated in the transaction, but not outside of it
            TestEnv.AssertHas(trans, TestEnv.UpdatedRow);
            TestEnv.AssertHas(db, TestEnv.Row);

            trans.Commit();
        }

        [Test]
        public void RollbackDelete()
        {
            var db = TestEnv.SetupDelete(_getDb());
            var trans = db.BeginTransaction();
            trans.Delete(TestEnv.Table, TestEnv.Keys);
            trans.Rollback();
            // The row should still exist
            TestEnv.AssertHas(db, TestEnv.Row);
        }

        [Test]
        public void CommitDelete()
        {
            var db = TestEnv.SetupDelete(_getDb());
            var trans = db.BeginTransaction();
            trans.Delete(TestEnv.Table, TestEnv.Keys);
            trans.Commit();
            // The row should be gone
            TestEnv.AssertGone(db);
        }

        [Test(Description="Make sure the operation isn't visible outside the scope of the transaction until it is commited")]
        public void IsolateDelete()
        {
            var db = TestEnv.SetupDelete(_getDb());
            var trans = db.BeginTransaction();
            trans.Delete(TestEnv.Table, TestEnv.Keys);
            // At this point, the row should be deleted in the transaction, but not outside it
            TestEnv.AssertGone(trans);
            TestEnv.AssertHas(db, TestEnv.Row);

            trans.Commit();
        }

    }
}
