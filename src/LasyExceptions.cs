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
            : base("Could not save " + tableName + " because the following fields are null: " + keys.Print())
        { }
    }

    public class ThisSadlyHappenedException : Exception
    {
        public ThisSadlyHappenedException(string message)
            : base(message)
        { }
    }
}
