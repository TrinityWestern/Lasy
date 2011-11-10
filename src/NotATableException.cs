using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public class NotATableException : Exception
    {
        public NotATableException(string tablename, string message = null)
            : base(message ?? (tablename + " was not found in the database"))
        {
            Tablename = tablename;
        }

        public string Tablename { get; private set; }
    }
}
