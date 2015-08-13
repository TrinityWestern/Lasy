using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using NUnit.Framework;
using Lasy;

namespace LasyTests.Sql
{
    [TestFixture]
    public class SqlAnalyzerTests
    {
        [TestCase("Person", Result=true)]
        [TestCase("Bar", Result = false)]
        [TestCase("[Person]", Result = true)]
        [TestCase("[dbo].[Person]", Result = true)]
        [TestCase("SchemaA.Foo", Result = true)]
        [TestCase("SchemaB.Foo", Result = true)]
        [TestCase("Foo", Result=false)]
        public bool TableExists(string table)
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            return ana.TableExists(table);
        }

        [Test]
        public void TableExistsFindsViews()
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            Assert.True(ana.TableExists("ID_NUMView"));
        }

        [TestCase("Person", Result= "(PersonId,FirstName,LastName,Age)")]
        [TestCase("[Person]", Result = "(PersonId,FirstName,LastName,Age)")]
        [TestCase("[dbo].[Person]", Result = "(PersonId,FirstName,LastName,Age)")]
        [TestCase("SchemaA.Foo", Result="(FooId)", Description="Shouldn't crash if 2 tables in different schemas with same name")]
        [TestCase("SchemaA.Foo", Result = "(FooId)", Description = "Shouldn't crash if 2 tables in different schemas with same name")]
        public string GetFields(string table)
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            var fields = ana.GetFields(table);
            return fields.Print();
        }

        [Test]
        public void GetFieldsFromView()
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            var fields = ana.GetFields("ID_NUMView");
            Assert.AreEqual("(ID_NUM,PersonId,FirstName,LastName,Age)", fields.Print());
        }

        [Test]
        public void GetFieldTypes()
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            var fields = ana.GetFieldTypes("Person");
            Assert.AreEqual("([Age,int NULL],[FirstName,nvarchar(50) NULL],[LastName,nvarchar(50) NULL],[PersonId,int NOT NULL])", 
                fields.Print());
        }

        [Test]
        public void GetFieldTypesFromView()
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            var fields = ana.GetFieldTypes("ID_NUMView");
            Assert.AreEqual("([Age,int NULL],[FirstName,nvarchar(50) NULL],[ID_NUM,int NOT NULL],[LastName,nvarchar(50) NULL],[PersonId,int NOT NULL])", 
                fields.Print());
        }

        [TestCase("SchemaA.Baz", Result="BazId")]
        [TestCase("Organization", Result="OrganizationId")]
        public string GetAutoNumberKey(string table)
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            return ana.GetAutoNumberKey(table);
        }

        [TestCase("SchemaA.Baz", Result = "(BazId)")]
        [TestCase("Organization", Result = "(OrganizationId)")]
        public string GetPrimaryKeys(string table)
        {
            var ana = new SqlAnalyzer(Config.TestDBConnectionString);
            return ana.GetPrimaryKeys(table).Print();
        }


    }
}
