using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    public static class IReadableExtensions
    {
        [Obsolete("Use RawRead instead")]
        public static IEnumerable<Dictionary<string, object>> RawReadCustomFields(this IReadable reader, string tableName, IEnumerable<string> fields, object id)
        {
            return reader.RawRead(tableName, id as Dictionary<string, object> ?? id._AsDictionary(), fields);
        }

        public static IEnumerable<Dictionary<string, object>> RawRead(this IReadable reader, string tableName, Dictionary<string, object> id)
        {
            return reader.RawRead(tableName, id as Dictionary<string, object> ?? id._AsDictionary());
        }

        public static Dictionary<string, object> ReadPK(this IReadable reader, string tablename, int primaryKey)
        {
            var keys = new Dictionary<string, object>();
            var keynames = reader.Analyzer.GetPrimaryKeys(tablename);

            if (keynames.Count > 1)
                throw new Exception("This table " + tablename + " has too many Primary Keys");

            var paras = new Dictionary<string,object>(){{ keynames.First(), primaryKey}};

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

        public static T ReadPK<T>(this IReadable reader, string tablename, int primaryKey) where T:class, new()
        {
            var results = reader.ReadPK(tablename, primaryKey);
            var output = new T();
            return output._SetFrom(results);
        }

        //Whatever we use for T needs to have a zero-parameter constructor
        public static IEnumerable<T> ReadAll<T>(this IReadable reader, string tableName = null) where T:class, new()
        {
            if(tableName.IsNullOrEmpty())
                tableName = typeof(T).Name;

            var results = reader.RawRead(tableName, new Dictionary<string,object>());

            return results.Select(x => new T()._SetFrom(x) );
        }

        /// <summary>
        /// Reads values from a table
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="tableName">The name of the table</param>
        /// <param name="values">An object with values that are the "where" clause in sql. Example: new {col1 = 6, col2 = 'myString'}</param>
        /// <param name="trans">A SQL transaction that can be passed in if this Read is to be part of a greater transaction</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, object>> Read(this IReadable reader, string tableName, object values)
        {
            return reader.RawRead(tableName, values as Dictionary<string, object> ?? values._AsDictionary());
        }

        public static IEnumerable<T> Read<T>(this IReadable reader, string tableName, object values) where T: class, new()
        {
            return Read<T>(reader, tableName, values as Dictionary<string, object> ?? values._AsDictionary());
        }

        public static IEnumerable<T> Read<T>(this IReadable reader, string tableName, Dictionary<string, object> values) where T:class, new()
        {
            var res = Read(reader, tableName, values);
            ObjectReader<T> converter = new ObjectReader<T>();
            return converter.ReadAll(res);
        }

        public static IEnumerable<Dictionary<string, object>> RawReadAll(this IReadable reader, string tableName)
        {
            return reader.RawRead(tableName, new Dictionary<string, object>());
        }

        public static IEnumerable<Dictionary<string, object>> RawReadAllCustomFields(this IReadable reader, string tableName, IEnumerable<string> fields)
        {
            return reader.RawReadCustomFields(tableName, fields, new Dictionary<string, object>());
        }
    }
}
