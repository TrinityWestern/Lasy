using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;
using Nvelope.Configuration;

namespace LasyTests.Sql
{
    [TestFixture]
    public class MySqlModifierTests
    {
        protected string _connStr
        {
            get
            {
                return Config.ConnectionString("mysqldb");
            }
        }

        [Test]
        public void CreateTable()
        {
            var mod = new MySqlModifier(_connStr, new MySqlAnalyzer(_connStr));
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
