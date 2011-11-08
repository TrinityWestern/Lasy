using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nvelope.Configuration;
using Nvelope;
using Lasy;

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

        [Test]
        public void TableExists()
        {
            var ana = new SQL2005DBAnalyzer(connStr);
            Assert.True(ana.TableExists("Common.Person"));
            Assert.False(ana.TableExists("Common.Unicorn"));
        }

        [Test]
        public void CreatesTable()
        {
            Assert.Fail();
        }

        [Test]
        public void CreatesSchema()
        {
            Assert.Fail();
        }

        [Test(Description="If CreateTables mode is False, throw an exception if you try to write to a non-existant table")]
        public void InsertThrowsExceptionIfNoTable()
        {
            Assert.Fail();
        }

        [Test(Description="If EnforceTables mode is True, throw an exception if you try to read from a non-existant table")]
        public void ReadThrowsExceptionIfNoTable()
        {
            Assert.Fail();
        }

        [Test(Description = "If we ask for the PKs for a table that doesn't exist, then create the table, " +
            "then ask for the PKs again, we should get the right answer")]
        public void GetPrimaryKeyNotCached()
        {
            Assert.Fail();
        }

        [Test(Description = "If we ask for the autonumberss for a table that doesn't exist, then create the table, " +
            "then ask for the autonumbers again, we should get the right answer")]
        public void GetAutonumberNotCached()
        {
            Assert.Fail();
        }

        [Test(Description = "If we ask for the fields for a table that doesn't exist, then create the table, " +
            "then ask for the fields again, we should get the right answer")]
        public void GetFieldsNotCached()
        {
            Assert.Fail();
        }
    }
}
