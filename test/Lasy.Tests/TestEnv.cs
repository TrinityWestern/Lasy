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
    public static class TestEnv
    {
        /// <summary>
        /// The name of the table we'll be testing on
        /// </summary>
        public static string Table { get { return "Tbl"; } }
        /// <summary>
        /// The row we'll either insert or delete
        /// </summary>
        public static Dictionary<string, object> Row
        {
            get
            {
                return new { Foo = "bar", Val = 7 }._AsDictionary();
            }
        }
        /// <summary>
        /// The contents of the row after we do an update
        /// </summary>
        public static Dictionary<string, object> UpdatedRow
        {
            get
            {
                return Row.Assoc("Val", 42);
            }
        }

        /// <summary>
        /// Returns the keys to the row, used to retrieve it from the db. 
        /// Note: Doesn't need to be the primary key of the row, just some field
        /// that is unique to the test row and doesn't change between _row and 
        /// _updatedRow
        /// </summary>
        public static Dictionary<string, object> Keys
        {
            get
            {
                var diff = Row.Diff(UpdatedRow);
                return Row.Except(diff.Keys);
            }
        }

        /// <summary>
        /// Setup the db for inserts - just return a new db generally
        /// </summary>
        /// <returns></returns>
        public static T SetupInsert<T>(T db) where T: IWriteable
        {
            return db;
        }

        /// <summary>
        /// Setup the db for updates - ie insert _row into it
        /// </summary>
        /// <returns></returns>
        public static T SetupUpdate<T>(T db) where T: IWriteable
        {
            db.Insert(Table, Row);
            return db;
        }

        /// <summary>
        /// Setup the db for deletes - ie insert _row into it
        /// </summary>
        /// <returns></returns>
        public static T SetupDelete<T>(T db) where T: IWriteable
        {
            db.Insert(Table, Row);
            return db;
        }

        /// <summary>
        /// Make sure the db contains the row supplied
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trans">If supplied read using the transaction</param>
        /// <param name="contents"></param>
        public static void AssertHas(IReadWrite db, Dictionary<string, object> contents)
        {
            var rows = db.Read(Table, Keys);
            Assert.AreEqual(1, rows.Count(), "Expected the row to be in the database");
            var exceptAutokey = rows.Single().Except(db.Analyzer.GetAutoNumberKey(Table));
            Assert.AreEqual(contents.Print(), exceptAutokey.Print(), "The row was there, but not with the values we expected");
        }

        /// <summary>
        /// Make sure there's no row in the database 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trans"></param>
        public static void AssertGone(IReadWrite db)
        {
            Assert.False(db.Read(Table, Keys).Any(), "Expected there to be no row in the database");
        }
    }
}
