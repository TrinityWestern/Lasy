using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Configuration;
using Nvelope;
using NUnit.Framework;
using Lasy;

namespace LasyTests.Sql
{
    [TestFixture]
    public class SqlAnalyzerTests
    {
        protected string connStr
        {
            get
            {
                return Config.ConnectionString("testdb");
            }
        }

        [Test]
        public void TableExists()
        {
            var ana = new SqlAnalyzer(connStr);
            Assert.True(ana.TableExists("Person"));
        }

        [Test]
        public void TableExistsFindsViews()
        {
            var ana = new SqlAnalyzer(connStr);
            Assert.True(ana.TableExists("ID_NUMView"));
        }

        [Test]
        public void GetFields()
        {
            var ana = new SqlAnalyzer(connStr);
            var fields = ana.GetFields("Person");
            Assert.AreEqual("(PersonId,FirstName,LastName,Age)", fields.Print());
        }

        [Test]
        public void GetFieldsFromTable()
        {
            var ana = new SqlAnalyzer(connStr);
            var fields = ana.GetFields("ID_NUMView");
            Assert.AreEqual("(ID_NUM,PersonId,FirstName,LastName,Age)", fields.Print());
        }

    }
}
