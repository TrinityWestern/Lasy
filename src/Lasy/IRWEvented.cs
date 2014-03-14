using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface IRWEvented : IReadWrite, IWriteEvented, IReadEvented
    { }

    public interface IWriteEvented : IWriteable
    {
        /// <summary>
        /// Fires before an insert
        /// </summary>
        event Action<string, Dictionary<string, object>> OnInsert;
        /// <summary>
        /// Fires before a delete
        /// </summary>
        event Action<string, Dictionary<string, object>> OnDelete;
        /// <summary>
        /// Fires before an update
        /// </summary>
        event Action<string, Dictionary<string, object>, Dictionary<string, object>> OnUpdate;
        /// <summary>
        /// Fires before every insert, update, or delete
        /// </summary>
        event Action<string, Dictionary<string, object>> OnWrite;
    }

    public interface IReadEvented : IReadable
    {
        /// <summary>
        /// Fires before a Read operation
        /// </summary>
        event Action<string, Dictionary<string, object>> OnRead;
    }
}
