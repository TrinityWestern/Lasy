using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    public class KeyNotSetException : Exception
    {
        public KeyNotSetException(string tableName, IEnumerable<string> keys) 
            : base("Could not get the keys for the table '" + tableName + "' because the following fields are null: " + keys.Print())
        { }
    }

    public class ThisSadlyHappenedException : Exception
    {
        public ThisSadlyHappenedException(string message)
            : base(message)
        { }
    }

    public class NotATableException : Exception
    {
        public NotATableException(string tablename, string message = null)
            : base(message ?? ("The table '" + tablename + "' was not found in the database"))
        {
            Tablename = tablename;
        }

        public string Tablename { get; private set; }
    }

    public class MockDBFailure : Exception
    {
        public MockDBFailure()
            : base()
        { }

        public MockDBFailure(string message)
            : base(message)
        { }
    }
}
