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
        public void GetFieldsFromView()
        {
            var ana = new SqlAnalyzer(connStr);
            var fields = ana.GetFields("ID_NUMView");
            Assert.AreEqual("(ID_NUM,PersonId,FirstName,LastName,Age)", fields.Print());
        }

        [Test]
        public void GetFieldTypes()
        {
            var ana = new SqlAnalyzer(connStr);
            var fields = ana.GetFieldTypes("Person");
            Assert.AreEqual("([Age,int NULL],[FirstName,nvarchar(50) NULL],[LastName,nvarchar(50) NULL],[PersonId,int NOT NULL])", 
                fields.Print());
        }

        [Test]
        public void GetFieldTypesFromView()
        {
            var ana = new SqlAnalyzer(connStr);
            var fields = ana.GetFieldTypes("ID_NUMView");
            Assert.AreEqual("([Age,int NULL],[FirstName,nvarchar(50) NULL],[ID_NUM,int NOT NULL],[LastName,nvarchar(50) NULL],[PersonId,int NOT NULL])", 
                fields.Print());
        }

    }
}
