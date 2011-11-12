using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    /// <summary>
    /// A bunch of helper methods to instantiate various database types
    /// </summary>
    public static class ConnectTo
    {
        public static SqlDB Sql2000(string connString, bool strictTables = true)
        {
            return new SqlDB(connString, new Sql2000Analyzer(connString), strictTables);
        }

        public static SqlDB Sql2005(string connString, bool strictTables = true)
        {
            return new SqlDB(connString, new SqlAnalyzer(connString), strictTables);
        }

        public static ModifiableSqlDB ModifiableSql2000(string connString)
        {
            var analyzer = new Sql2000Analyzer(connString);
            var modifier = new SqlModifier(connString, analyzer);
            return new ModifiableSqlDB(connString, modifier);
        }

        public static ModifiableSqlDB ModifiableSql2005(string connString)
        {
            var analyzer = new SqlAnalyzer(connString);
            var modifier = new SqlModifier(connString, analyzer);
            return new ModifiableSqlDB(connString, modifier);
        }

        public static FileDB File(string directory, string fileExtension = ".rpt")
        {
            return new FileDB(directory, fileExtension);
        }

        public static FakeDB Memory()
        {
            return new FakeDB();
        }

        public static UnreliableDb Unreliable()
        {
            return new UnreliableDb();
        }
    }
}
