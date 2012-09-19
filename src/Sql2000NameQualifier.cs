using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public class Sql2000NameQualifier : SqlNameQualifier
    {
        public override string SchemaName(string tablename)
        {
            return "dbo";
        }
    }
}
