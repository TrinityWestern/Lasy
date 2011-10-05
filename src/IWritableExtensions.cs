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

        /// <summary>
        /// Gets all the primary keys for tablename from values. Throws an exception if any of the keys
        /// are not supplied
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ExtractKeys(this IWriteable writer, string tablename,
            Dictionary<string, object> values)
        {
            var keynames = writer.Analyzer.GetPrimaryKeys(tablename);
            // Make sure they supplied all the keys
            if (!values.Keys.ToSet().IsSupersetOf(keynames))
                throw new KeyNotSetException(tablename, keynames.Except(values.Keys));

            var keys = values.Only(keynames);
            return keys;
        }

        /// <summary>
        /// Update, and figure out the keys to use by analyzing the database for the primary key names. If the primary keys
        /// are not supplied in values, an exception will be thrown
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="values"></param>
        /// <param name="trans"></param>
        public static void Update(this IWriteable writer, string tablename, Dictionary<string, object> values, ITransaction trans = null)
        {
            var keys = writer.ExtractKeys(tablename, values);
            writer.Update(tablename, values, keys, trans);
        }

        /// <summary>
        /// Update, and figure out the keys to use by analyzing the database for the primary key names. If the primary keys
        /// are not supplied in obj, an exception will be thrown
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="obj"></param>
        /// <param name="trans"></param>
        public static void Update(this IWriteable writer, string tablename, object obj, ITransaction trans = null)
        {
            var values = obj as Dictionary<string, object> ?? obj._AsDictionary();

            Update(writer, tablename, values, trans);
        }

        /// <summary>
        /// Calls Update after converting dataObj and keysObj to Dictionaries
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="dataObj">If Dict[string,object] it will be passed through, otherwise converted</param>
        /// <param name="keysObj">If Dict[string,object] it will be passed through, otherwise converted</param>
        /// <param name="trans"></param>
        public static void Update(this IWriteable writer, string tablename, object dataObj, object keysObj, ITransaction trans = null)
        {
            Dictionary<string,object> data = dataObj as Dictionary<string, object> ?? dataObj._AsDictionary();
            Dictionary<string,object> keys = keysObj as Dictionary<string, object> ?? keysObj._AsDictionary();
            writer.Update(tablename, data, keys, trans);
        }

        public static void Delete(this IWriteable writer, string tablename, object obj, ITransaction trans = null)
        {
            var dict = obj as Dictionary<string, object>;
            writer.Delete(tablename, dict ?? obj._AsDictionary(), trans);
        }
    }
}
