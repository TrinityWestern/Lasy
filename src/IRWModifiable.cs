using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    /// <summary>
    /// Defines a database that is IReadWrite, as well as IModifiable (ie, can create tables at runtime)
    /// </summary>
    public interface IRWModifiable : IReadWrite, IModifiable
    {
    }
}
