using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
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
