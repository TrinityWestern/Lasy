using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Reflection;

namespace Lasy
{
    public interface IDBModifier : IDBAnalyzer
    {
        void CreateTable(string tablename, Dictionary<string, object> fields);
        void DropTable(string tablename);
    }

    public static class IDBModifierExtensions
    {
        public static void CreateTable(this IDBModifier meta, string tablename, object paramObject)
        {
            meta.CreateTable(tablename, paramObject._AsDictionary());
        }

        public static void EnsureTable(this IDBModifier meta, string tablename, Dictionary<string, object> instance)
        {
            if (!meta.TableExists(tablename))
                meta.CreateTable(tablename, instance);
        }

        public static void EnsureTable(this IDBModifier meta, string tablename, object instance)
        {
            EnsureTable(meta, tablename, instance._AsDictionary());
        }

        /// <summary>
        /// Drops the table if it exists, else does nothing
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="tablename"></param>
        public static void KillTable(this IDBModifier meta, string tablename)
        {
            if (meta.TableExists(tablename))
                meta.DropTable(tablename);
        }
    }
}
