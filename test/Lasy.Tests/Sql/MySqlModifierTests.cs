using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;

namespace LasyTests.Sql
{
    [TestFixture]
    public class MySqlModifierTests
    {
        [Test]
        public void CreateTable()
        {
            var mod = new MySqlModifier(Config.TestMySqlConnectionString, new MySqlAnalyzer(Config.TestMySqlConnectionString));
            var cols = new Dictionary<string, SqlColumnType>();
            cols.Add("FoosumId", new SqlColumnType(System.Data.SqlDbType.Int, false));
            cols.Add("Name", new SqlColumnType(System.Data.SqlDbType.NVarChar, true, 42));
            cols.Add("IsReal", new SqlColumnType(System.Data.SqlDbType.Bit, false));
            mod.CreateTable("Foosum", cols);

            Assert.True(mod.Analyzer.TableExists("Foosum"));

            mod.DropTable("Foosum");
        }
    }
}
