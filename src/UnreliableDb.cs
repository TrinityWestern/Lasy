using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;
using Nvelope;

namespace Lasy
{
    /// <summary>
    /// A db class that we can set up to fail on the next read operation
    /// Useful for simulating DB failures in the middle of transactions
    /// </summary>
    public class UnreliableDb : FakeDB
    {
        public UnreliableDb()
            : base()
        {
            _setup();
        }

        public UnreliableDb(IDBAnalyzer analyzer)
            : base(analyzer)
        {
            _setup(); 
        }

        protected void _setup()
        {
            // Create event handlers for insert, update, delete
            // that just call our method to see if FailOnNextOp is set
            this.OnDelete += (a, b) => _failCheck();
            this.OnInsert += (a, b) => _failCheck();
            this.OnUpdate += (a, b, c) => _failCheck();
        }

        public bool FailOnNextOp = false;

        protected void _failCheck()
        {
            if (this.FailOnNextOp)
            {
                this.FailOnNextOp = false;
                throw new MockDBFailure();
            }
        }

    }
}
