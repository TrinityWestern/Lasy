using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    public class SqlNameQualifier : INameQualifier
    {
        public virtual string TableName(string tablename)
        {
            var res = tablename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last().ChopEnd("]").ChopStart("[");
            return res;
        }

        public virtual string SchemaName(string tablename)
        {
            var parts = tablename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ChopEnd("]").ChopStart("["));
            if (parts.Count() > 1)
                return parts.First();
            else
                return "";
        }

        public virtual bool SupportsSchemas
        {
            get { return true; }
        }
    }
}
