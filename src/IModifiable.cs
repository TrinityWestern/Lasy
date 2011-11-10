using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    /// <summary>
    /// Indicates that a database supports being modfiable - ie tables can be added at runtime
    /// </summary>
    public interface IModifiable : IAnalyzable
    {
        IDBModifier Modifier { get; }
    }
}
