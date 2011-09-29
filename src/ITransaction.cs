using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface ITransaction
    {
        void Commit();

        void Rollback();
    }
}
