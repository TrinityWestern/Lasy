using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nvelope.Configuration;
using Nvelope;
using Nvelope.Reflection;
using Lasy;
using System.Data.SqlClient;

namespace LasyTests
{
    [TestFixture]
    public class TableCreationTests
    {
        protected string connStr
        {
            get
            {
                return Config.ConnectionString("testdb");
            }
        }

        protected object fredTheUnicorn
        {
            get
            {
                return new Person() { Age = null, FirstName = "Fred", LastName = "Unicorn", PersonId = 7 };
            }
        }

        [SetUp]
        [TearDown]
        public void Cleanup()
        {
            var ana = new Sql2005Meta(connStr);

            if (ana.TableExists("dbo.Unicorn"))
                using (var conn = new SqlConnection(connStr))
                    conn.Execute("drop table dbo.Unicorn");

            if (ana.TableExists("TestSchema.Pegasus"))
                using (var conn = new SqlConnection(connStr))
                    conn.Execute("drop table TestSchema.Pegasus");

            if (ana.SchemaExists("TestSchema"))
                using (var conn = new SqlConnection(connStr))
                    conn.Execute("drop schema TestSchema");
        }

        [Test]
        public void TableExists()
        {
            var ana = new Sql2005Meta(connStr);
            Assert.True(ana.TableExists("dbo.Person"));
            Assert.False(ana.TableExists("dbo.Unicorn"));
        }

        [Test]
        public void CreatesTable()
        {
            var ana = new Sql2005Meta(connStr);

            Assert.False(ana.TableExists("dbo.Unicorn"));
            ana.CreateTable("dbo.Unicorn", fredTheUnicorn);
            Assert.True(ana.TableExists("dbo.Unicorn"));
        }

        [Test]
        public void CreatesCorrectColumns()
        {
            var ana = new Sql2005Meta(connStr);

            ana.CreateTable("dbo.Unicorn", fredTheUnicorn);
            var db = ConnectTo.Sql2005(connStr);
            db.Insert("dbo.Unicorn", fredTheUnicorn);
            var fromDb = db.RawReadAll("dbo.Unicorn");
            Assert.AreEqual(1, fromDb.Count());
            Assert.AreEqual(fredTheUnicorn._Inspect(), fromDb.First().Except("UnicornId").Print());
        }

        [Test]
        public void CreatesSchema()
        {
            var ana = new Sql2005Meta(connStr);
            Assert.False(ana.SchemaExists("TestSchema"));
            ana.CreateSchema("TestSchema");
            Assert.True(ana.SchemaExists("TestSchema"));
        }

        [Test]
        public void ImplicitlyCreatesSchema()
        {
            var ana = new Sql2005Meta(connStr);
            Assert.False(ana.SchemaExists("TestSchema"));
            ana.CreateTable("TestSchema.Pegasus", fredTheUnicorn);
            Assert.True(ana.TableExists("TestSchema.Pegasus"));

            var db = ConnectTo.Sql2005(connStr);
            db.Insert("TestSchema.Pegasus", fredTheUnicorn);
            var fromDb = db.RawReadAll("TestSchema.Pegasus");
            Assert.AreEqual(1, fromDb.Count());
            Assert.AreEqual(fredTheUnicorn._Inspect(), fromDb.First().Except("PegasusId").Print());
        }

        [Test]
        public void ImplicitlyCreateTable()
        {
            // In order to implicitly create tables, we need to use a db that supports that - 
            // ie ModifiableSqlDB
            var db = ConnectTo.ModifiableSql2005(connStr);
            Assert.False(db.Analyzer.TableExists("TestSchema.Pegasus"));
            db.Insert("TestSchema.Pegasus", fredTheUnicorn);
            Assert.True(db.Analyzer.TableExists("TestSchema.Pegasus"));
            var fromDb = db.RawReadAll("TestSchema.Pegasus");
            Assert.AreEqual(1, fromDb.Count());
            Assert.AreEqual(fredTheUnicorn._Inspect(), fromDb.First().Except("PegasusId").Print());
        }

        [Test(Description = "If not using a modifiable database, throw an exception if you try to write to a non-existant table")]
        public void InsertThrowsExceptionIfNoTable()
        {
            // If we're not using a ModifiableSqlDB, we should throw an exception when inserting into a table
            // that doesn't exist
            var db = ConnectTo.Sql2005(connStr);
            Assert.False(db.Analyzer.TableExists("dbo.Unicorn"));
            Assert.Throws<NotATableException>(() => db.Insert("dbo.Unicorn", fredTheUnicorn));
        }

        [Test(Description="If EnforceTables mode is True, throw an exception if you try to read from a non-existant table")]
        public void ReadThrowsExceptionIfNoTable()
        {
            var db = ConnectTo.Sql2005(connStr);
            Assert.False(db.Analyzer.TableExists("dbo.Unicorn"));
            Assert.Throws<NotATableException>(() => db.RawReadAll("dbo.Unicorn"));
        }

        [Test(Description = "If we ask for the PKs for a table that doesn't exist, then create the table, " +
            "then ask for the PKs again, we should get the right answer")]
        public void GetPrimaryKeyNotCached()
        {
            var db = ConnectTo.ModifiableSql2005(connStr);
            Assert.AreEqual("()", db.Analyzer.GetPrimaryKeys("dbo.Unicorn").Print());
            db.Insert("dbo.Unicorn", fredTheUnicorn);
            Assert.AreEqual("(UnicornId)", db.Analyzer.GetPrimaryKeys("dbo.Unicorn").Print());
        }

        [Test(Description = "If we ask for the autonumberss for a table that doesn't exist, then create the table, " +
            "then ask for the autonumbers again, we should get the right answer")]
        public void GetAutonumberNotCached()
        {
            var db = ConnectTo.ModifiableSql2005(connStr);
            Assert.Null(db.Analyzer.GetAutoNumberKey("dbo.Unicorn"));
            db.Insert("dbo.Unicorn", fredTheUnicorn);
            Assert.AreEqual("UnicornId", db.Analyzer.GetAutoNumberKey("dbo.Unicorn"));
        }

        [Test(Description = "If we ask for the fields for a table that doesn't exist, then create the table, " +
            "then ask for the fields again, we should get the right answer")]
        public void GetFieldsNotCached()
        {
            var db = ConnectTo.ModifiableSql2005(connStr);
            Assert.AreEqual("()", db.Analyzer.GetFields("dbo.Unicorn").Print());
            db.Insert("dbo.Unicorn", fredTheUnicorn);
            Assert.AreEqual(fredTheUnicorn._Fields().Print(), db.Analyzer.GetFields("dbo.Unicorn").Except("UnicornId").Print());
        }
    }
}
