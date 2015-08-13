using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Reflection;
using Nvelope;

namespace Lasy
{
    public interface IReadable : IAnalyzable
    {
        /// <summary>
        /// Read all that match on id from the table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyFields"></param>
        /// <param name="fields">If supplied, read just these fields</param>
        /// <returns></returns>
        IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null);
    }

    public static class IReadableExtensions
    {
        public static Dictionary<string, object> ReadPK(this IReadable reader, string tablename, int primaryKey)
        {
            var keys = new Dictionary<string, object>();
            var keynames = reader.Analyzer.GetPrimaryKeys(tablename);

            if (keynames.Count > 1)
                throw new Exception("This table " + tablename + " has too many Primary Keys");

            var paras = new Dictionary<string, object>() { { keynames.First(), primaryKey } };

            return ReadPK(reader, tablename, paras);
        }

        public static Dictionary<string, object> ReadPK(this IReadable reader, string tablename, Dictionary<string, object> primaryKeys)
        {
            var results = reader.RawRead(tablename, primaryKeys);
            if (results.Count() > 1)
                throw new Exception("This table " + tablename + " has more than one row with the primary key " + primaryKeys.Print());
            else if (results.Count() == 0)
                return null;
            else
                return results.First();
        }

        public static T ReadPK<T>(this IReadable reader, string tablename, int primaryKey) where T : class, new()
        {
            var results = reader.ReadPK(tablename, primaryKey);
            var output = new T();
            return output._SetFrom(results);
        }

        //Whatever we use for T needs to have a zero-parameter constructor
        public static IEnumerable<T> ReadAll<T>(this IReadable reader, string tableName = null) where T : class, new()
        {
            if (tableName.IsNullOrEmpty())
                tableName = typeof(T).Name;

            var results = reader.RawRead(tableName, new Dictionary<string, object>());

            return results.Select(x => new T()._SetFrom(x));
        }

        public static IEnumerable<Dictionary<string, object>> ReadAll(this IReadable reader, string tableName, IEnumerable<string> fields = null)
        {
            return reader.RawRead(tableName, new Dictionary<string,object>(), fields);
        }

        /// <summary>
        /// Reads values from a table
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="tableName">The name of the table</param>
        /// <param name="values">An object with values that are the "where" clause in sql. Example: new {col1 = 6, col2 = 'myString'}. You can also pass a dictionarys</param>
        /// <param name="trans">A SQL transaction that can be passed in if this Read is to be part of a greater transaction</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, object>> Read(this IReadable reader, string tableName, object values, IEnumerable<string> fields = null)
        {
            return reader.RawRead(tableName, values as Dictionary<string, object> ?? values._AsDictionary(), fields);
        }

        public static IEnumerable<T> Read<T>(this IReadable reader, string tableName, object values) where T : class, new()
        {
            return Read<T>(reader, tableName, values as Dictionary<string, object> ?? values._AsDictionary());
        }

        public static IEnumerable<T> Read<T>(this IReadable reader, string tableName, Dictionary<string, object> values) where T : class, new()
        {
            var results = Read(reader, tableName, values);
            return results.Select(x => new T()._SetFrom(x));
        }
    }
}
