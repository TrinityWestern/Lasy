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
            return new SqlDB(connString, new Sql2000Meta(connString), strictTables);
        }

        public static SqlDB Sql2005(string connString, bool strictTables = true)
        {
            return new SqlDB(connString, new Sql2005Meta(connString), strictTables);
        }

        public static ModifiableSqlDB ModifiableSql2000(string connString)
        {
            return ModifiableSqlDB.New(connString, new Sql2000Meta(connString));
        }

        public static ModifiableSqlDB ModifiableSql2005(string connString)
        {
            return ModifiableSqlDB.New(connString, new Sql2005Meta(connString));
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
