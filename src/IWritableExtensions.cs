using System.Collections.Generic;
using System.Linq;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    public static class IWritableExtensions
    {
        public static Dictionary<string, object> Insert(this IWriteable writer, string tablename, object obj, ITransaction trans = null)
        {
            return writer.Insert(tablename, obj as Dictionary<string, object> ?? obj._AsDictionary(), trans);
        }

        public static int InsertAutoKey(this IWriteable writer, string tablename, object obj, ITransaction trans = null)
        {
            return writer.Insert(tablename, obj as Dictionary<string, object> ?? obj._AsDictionary(), trans).Single().Value.ConvertTo<int>();
        }

        public static void Update(this IWriteable writer, string tablename, object obj, ITransaction trans = null)
        {
            var values = obj as Dictionary<string, object> ?? obj._AsDictionary();

            var keynames = writer.Analyzer.GetPrimaryKeys(tablename);
            // Make sure they've supplied all the keys
            if (!values.Keys.ToSet().IsSupersetOf(keynames))
                throw new KeyNotSetException(tablename, keynames.Except(values.Keys));

            var keys = values.WhereKeys(k => keynames.Contains(k));

            writer.Update(tablename, values, keys, trans);
        }

        public static void Delete(this IWriteable writer, string tablename, object obj, ITransaction trans = null)
        {
            var dict = obj as Dictionary<string, object>;
            writer.Delete(tablename, dict ?? obj._AsDictionary(), trans);
        }
    }
}
