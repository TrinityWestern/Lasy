using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    /// <summary>
    /// Doesn't actually work. TODO: Implement this
    /// </summary>
    public class FakeDBTransaction : ITransaction
    {
        public void Commit()
        {
        }

        public void Rollback()
        {            
        }
    }
}
