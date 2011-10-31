using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    /// <summary>
    /// Extension methods for modifying the database - wrappers, middleware, etc
    /// </summary>
    public static class MetaExtensions
    {
        public static EventedTransactable AddEvents(this ITransactable db)
        {
            return new EventedTransactable(db);
        }

        public static EventedReadable AddEvents(this IReadable reader)
        {
            return new EventedReadable(reader);
        }

        public static EventedWritable AddEvents(this IWriteable writer)
        {
            return new EventedWritable(writer);
        }
    }
}
