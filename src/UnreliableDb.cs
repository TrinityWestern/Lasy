using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;
using Nvelope;

namespace Lasy
{
    public class UnreliableDb : EventedReadWrite
    {
        public UnreliableDb()
            : base(new FakeDB())
        {
            _setup();
        }

        public UnreliableDb(IDBAnalyzer analyzer)
            : base(new FakeDB(analyzer))
        {
            _setup(); 
        }

        protected void _setup()
        {
            // Create event handlers for insert, update, delete
            // that just call our method to see if FailOnNextOp is set
            this.OnDelete += (a, b, c) => _failCheck();
            this.OnInsert += (a, b, c) => _failCheck();
            this.OnUpdate += (a, b, c, d) => _failCheck();
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
