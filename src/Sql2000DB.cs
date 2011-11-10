using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public class Sql2000DB : SqlDB
    {
        public Sql2000DB(string connStr, bool strictTables = true)
            : base(connStr, new Sql2000DBModifier(connStr), strictTables)
        { }
    }
}
