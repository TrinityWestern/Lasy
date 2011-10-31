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
    [TestFixture]
    public class FileDBTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            _db = new FileDB("FileDBData");
        }

        private FileDB _db;

        [Test]
        public void ReadsData()
        {
            var vals = _db.RawReadAll("Simple");
            Assert.AreEqual(
                "(([Deprecated,False],[ID,1],[Name,Foo],[Value,Val]),([Deprecated,True],[ID,2],[Name,Bar],[Value,Bal]))", 
                vals.Print());
        }

        [Test]
        public void VariableWidthColumns()
        {
            var vals = _db.RawReadAll("VariableWidth");
            Assert.AreEqual(
                "(([Deprecated,False],[ID,1],[Name,Foobar],[Value,Foovalue]),([Deprecated,True],[ID,2],[Name,Bar],[Value,Bal]))",
                vals.Print());
        }

        [Test]
        public void ConvertsNulls()
        {
            var vals = _db.RawReadAll("WithNull");
            Assert.AreEqual(
                "(([Deprecated,],[ID,1],[Name,Foo],[Value,Val]),([Deprecated,True],[ID,2],[Name,Bar],[Value,]))",
                vals.Print());

            Assert.IsNull(vals.First()["Deprecated"]);
            Assert.IsNull(vals.Second()["Value"]);
        }

        [Test]
        public void ReadCustomFields()
        {
            var vals = _db.RawRead("Simple", new { ID = 1 }._AsDictionary(), "ID".And("Name"));
            Assert.AreEqual("(([ID,1],[Name,Foo]))", vals.Print());
        }

        [Test]
        public void Read()
        {
            var vals = _db.RawRead("Simple", new { ID = 1 }._AsDictionary());
            Assert.AreEqual("(([Deprecated,False],[ID,1],[Name,Foo],[Value,Val]))", vals.Print());
        }

        [Test]
        public void ReadAllCustomFields()
        {
            var vals = _db.RawRead("Simple", null, "ID".And("Name"));
            Assert.AreEqual("(([ID,1],[Name,Foo]),([ID,2],[Name,Bar]))", vals.Print());
        }

        [Test]
        public void InfervertPerformsAdequately()
        {
            var testVals = new Dictionary<string, Type>()
            {
                {"abc", typeof(string)},
                {"3.01", typeof(decimal)},
                {"197", typeof(int)},
                {"True", typeof(bool)},
                {"2010-10-31 00:00:00.000", typeof(DateTime)}
            };

            var db = new FileDB("");
            Action<string,Type> fn = (str, type) => Assert.AreEqual(type, db.Infervert(str).GetType());

            var time = fn.Benchmark(testVals.Repeat(5000));
            // We should be able to do this in under 200ms (arbitrary number)
            // On my machine, runtimes are 18-25ms
            // The old version was ~850ms
            Assert.Less(time, 200, "Infervert is too slow!");

        }
    }
}
