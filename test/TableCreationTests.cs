using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LasyTests
{
    [TestFixture]
    public class TableCreationTests
    {
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


    }
}
