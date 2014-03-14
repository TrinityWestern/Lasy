using System.Linq;
using Nvelope;
using Nvelope.Reflection;
using Lasy;
using NUnit.Framework;

namespace LasyTests
{
    [TestFixture]
    public class FakeDBTableTests
    {
        [Test]
        public void FindByValues()
        {
            var t = new FakeDBTable();
            var data = new {PNImportId = 1, Filename = "foosums"};
            t.Add(data._AsDictionary());

            var key = new {PNImportId = 1};
            var res = t.FindByFieldValues(key._AsDictionary());
            Assert.AreEqual(1, res.Count());

            Assert.AreEqual(data._Inspect(), res.First().Print());
        }
    }
}
