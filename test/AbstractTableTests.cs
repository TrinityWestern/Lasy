using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;
using Nvelope;
using Nvelope.Reflection;
using NUnit.Framework;

namespace LasyTests
{
    public class AbstractTableTests<T> where T: ITransactable, new()
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
        protected void _assertHas(IReadWrite db, Dictionary<string, object> contents)
        {
            var rows = db.Read(_table, _keys);
            Assert.AreEqual(1, rows.Count(), "Expected the row to be in the database");
            var exceptAutokey = rows.Single().Except(db.Analyzer.GetAutoNumberKey(_table));
            Assert.AreEqual(contents.Print(), exceptAutokey.Print(), "The row was there, but not with the values we expected");
        }

        /// <summary>
        /// Make sure there's no row in the database 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trans"></param>
        protected void _assertGone(IReadWrite db)
        {
            Assert.False(db.Read(_table, _keys).Any(), "Expected there to be no row in the database");
        }
    }
}
