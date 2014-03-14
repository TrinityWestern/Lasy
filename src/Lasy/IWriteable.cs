using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Reflection;
using Nvelope;

namespace Lasy
{
    public interface IWriteable : IAnalyzable
    {
        Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row);

        void Delete(string tableName, Dictionary<string, object> keyFields);

        void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields);
    }

    public static class IWriteableExtensions
    {
        public static Dictionary<string, object> Insert(this IWriteable writer, string tablename, object obj)
        {
            return writer.Insert(tablename, obj as Dictionary<string, object> ?? obj._AsDictionary());
        }

        public static int InsertAutoKey(this IWriteable writer, string tablename, object obj)
        {
            return writer.Insert(tablename, obj as Dictionary<string, object> ?? obj._AsDictionary()).Single().Value.ConvertTo<int>();
        }

        /// <summary>
        /// Update, and figure out the keys to use by analyzing the database for the primary key names. If the primary keys
        /// are not supplied in values, an exception will be thrown
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="values"></param>
        /// <param name="trans"></param>
        public static void Update(this IWriteable writer, string tablename, Dictionary<string, object> values)
        {
            var keys = writer.ExtractKeys(tablename, values);
            writer.Update(tablename, values, keys);
        }

        /// <summary>
        /// Update, and figure out the keys to use by analyzing the database for the primary key names. If the primary keys
        /// are not supplied in obj, an exception will be thrown
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="obj"></param>
        /// <param name="trans"></param>
        public static void Update(this IWriteable writer, string tablename, object obj)
        {
            var values = obj as Dictionary<string, object> ?? obj._AsDictionary();

            Update(writer, tablename, values);
        }

        /// <summary>
        /// Calls Update after converting dataObj and keysObj to Dictionaries
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="dataObj">If Dict[string,object] it will be passed through, otherwise converted</param>
        /// <param name="keysObj">If Dict[string,object] it will be passed through, otherwise converted</param>
        /// <param name="trans"></param>
        public static void Update(this IWriteable writer, string tablename, object dataObj, object keysObj)
        {
            Dictionary<string, object> data = dataObj as Dictionary<string, object> ?? dataObj._AsDictionary();
            Dictionary<string, object> keys = keysObj as Dictionary<string, object> ?? keysObj._AsDictionary();
            writer.Update(tablename, data, keys);
        }

        public static void Delete(this IWriteable writer, string tablename, object obj)
        {
            var dict = obj as Dictionary<string, object>;
            writer.Delete(tablename, dict ?? obj._AsDictionary());
        }
    }
}
