using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    /// <summary>
    /// Indicates that a database supports being modfiable - ie tables can be added at runtime
    /// </summary>
    public interface IModifiable : IAnalyzable
    {
        IDBModifier Modifier { get; }
    }

    public static class IModifiableExtensions
    {
        public static void EnsureTable(this IModifiable db, string tablename, Dictionary<string, object> row)
        {
            if (!db.Analyzer.TableExists(tablename))
                db.Modifier.CreateTable(tablename, row);
        }

        public static void EnsureTable(this IModifiable db, string tablename, object rowObj)
        {
            EnsureTable(db, tablename, rowObj._AsDictionary());
        }
    }
}
