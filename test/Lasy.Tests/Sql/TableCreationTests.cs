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

namespace LasyTests.Sql
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
            var mod = ConnectTo.ModifiableSql2005(connStr);

            if (mod.Analyzer.TableExists("dbo.Unicorn"))
                using (var conn = new SqlConnection(connStr))
                    conn.Execute("drop table dbo.Unicorn");

            if (mod.Analyzer.TableExists("TestSchema.Pegasus"))
                using (var conn = new SqlConnection(connStr))
                    conn.Execute("drop table TestSchema.Pegasus");

            if (mod.SqlAnalyzer.SchemaExists("TestSchema"))
                using (var conn = new SqlConnection(connStr))
                    conn.Execute("drop schema TestSchema");
        }

        [Test]
        public void TableExists()
        {
            var mod = ConnectTo.ModifiableSql2005(connStr);
            Assert.True(mod.Analyzer.TableExists("dbo.Person"));
            Assert.False(mod.Analyzer.TableExists("dbo.Unicorn"));
        }

        [Test]
        public void CreatesTable()
        {
            var db = ConnectTo.ModifiableSql2005(connStr);

            Assert.False(db.Analyzer.TableExists("dbo.Unicorn"));
            db.Modifier.CreateTable("dbo.Unicorn", fredTheUnicorn);
            Assert.True(db.Analyzer.TableExists("dbo.Unicorn"));
        }

        [Test]
        public void EnsureTable()
        {
            var db = ConnectTo.ModifiableSql2005(connStr);
            Assert.False(db.Analyzer.TableExists("dbo.Unicorn"));
            db.Modifier.EnsureTable("dbo.Unicorn", fredTheUnicorn);
            Assert.True(db.Analyzer.TableExists("dbo.Unicorn"));
            // If we call ensure table again, we shouldn't blow up or anything
            db.Modifier.EnsureTable("dbo.Unicorn", fredTheUnicorn);
        }

        [Test]
        public void KillTable()
        {
            var db = ConnectTo.ModifiableSql2005(connStr);
            Assert.False(db.Analyzer.TableExists("dbo.Unicorn"));
            db.Modifier.KillTable("dbo.Unicorn"); // This shouldn't throw an exception

            db.Modifier.EnsureTable("dbo.Unicorn", fredTheUnicorn);
            Assert.True(db.Analyzer.TableExists("dbo.Unicorn"));
            db.Modifier.KillTable("dbo.Unicorn"); // Should delete the table
            Assert.False(db.Analyzer.TableExists("dbo.Unicorn"));
        }

        [Test]
        public void CreatesCorrectColumns()
        {
            var moddb = ConnectTo.ModifiableSql2005(connStr);

            moddb.Modifier.CreateTable("dbo.Unicorn", fredTheUnicorn);
            var db = ConnectTo.Sql2005(connStr);
            db.Insert("dbo.Unicorn", fredTheUnicorn);
            var fromDb = db.ReadAll("dbo.Unicorn");
            Assert.AreEqual(1, fromDb.Count());
            Assert.AreEqual(fredTheUnicorn._Inspect(), fromDb.First().Except("UnicornId").Print());
        }

        [Test]
        public void CreatesSchema()
        {
            var db = ConnectTo.ModifiableSql2005(connStr);
            Assert.False(db.SqlAnalyzer.SchemaExists("TestSchema"));
            db.SqlModifier.CreateSchema("TestSchema");
            Assert.True(db.SqlAnalyzer.SchemaExists("TestSchema"));
        }

        [Test]
        public void ImplicitlyCreatesSchema()
        {
            var moddb = ConnectTo.ModifiableSql2005(connStr);
            Assert.False(moddb.SqlAnalyzer.SchemaExists("TestSchema"));
            moddb.Modifier.CreateTable("TestSchema.Pegasus", fredTheUnicorn);
            Assert.True(moddb.Analyzer.TableExists("TestSchema.Pegasus"));

            var db = ConnectTo.Sql2005(connStr);
            db.Insert("TestSchema.Pegasus", fredTheUnicorn);
            var fromDb = db.ReadAll("TestSchema.Pegasus");
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
            var fromDb = db.ReadAll("TestSchema.Pegasus");
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
            Assert.Throws<NotATableException>(() => db.ReadAll("dbo.Unicorn"));
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
