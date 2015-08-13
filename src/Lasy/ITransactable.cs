using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface ITransactable : IReadWrite
    {
        ITransaction BeginTransaction();
    }
}
