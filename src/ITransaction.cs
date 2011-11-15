using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    /// <summary>
    /// Indicates that something is a transaction.
    /// </summary>
    public interface ITransaction : IReadWrite, IDisposable
    {
        void Commit();

        void Rollback();
    }
}
