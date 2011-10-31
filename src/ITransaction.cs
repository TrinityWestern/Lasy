﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface ITransaction : IReadWrite, IDisposable
    {
        void Commit();

        void Rollback();
    }
}
