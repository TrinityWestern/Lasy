using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Reflection;
using Nvelope;

namespace Lasy
{
    public static class IReadWriteExtensions
    {
        /// <summary>
        /// Inserts the row if it doesn't exist, update the row(s) if they do
        /// </summary>
        /// <param name="readWrite"></param>
        /// <param name="tablename"></param>
        /// <param name="dataFields"></param>
        /// <param name="keyFields"></param>
        /// <param name="trans"></param>
        public static void Ensure(this IReadWrite readWrite,
            string tablename,
            Dictionary<string, object> dataFields,
            Dictionary<string, object> keyFields,
            ITransaction trans = null)
        {
            // See if the keyFields exist
            // If so, update them, otherwise insert them
            var existing = readWrite.Read(tablename, keyFields, trans);
            if (existing.Any())
                readWrite.Update(tablename, dataFields, keyFields, trans);
            else
            {
                var newRow = dataFields.Union(keyFields);
                var newKeys = readWrite.Insert(tablename, newRow, trans);
            }
        }

        /// <summary>
        /// Inserts the row if it doesn't exist, update the row(s) if they do
        /// </summary>
        /// <param name="readWrite"></param>
        /// <param name="tablename"></param>
        /// <param name="dataObj"></param>
        /// <param name="keyObj"></param>
        /// <param name="trans"></param>
        public static void Ensure(this IReadWrite readWrite,
            string tablename,
            object dataObj,
            object keyObj,
            ITransaction trans = null)
        {
            Ensure(readWrite,
                tablename,
                dataObj._AsDictionary(),
                keyObj._AsDictionary(),
                trans);
        }

        /// <summary>
        /// Inserts the row if it doesn't exist, update the row(s) if they do
        /// </summary>
        /// <param name="readWrite"></param>
        /// <param name="tablename"></param>
        /// <param name="values"></param>
        /// <param name="trans"></param>
        public static void Ensure(this IReadWrite readWrite,
            string tablename,
            Dictionary<string, object> values,
            ITransaction trans = null)
        {
            var keyFields = readWrite.ExtractKeys(tablename, values);
            Ensure(readWrite, tablename, values, keyFields, trans);
        }

        /// <summary>
        /// Inserts the row if it doesn't exist, update the row(s) if they do
        /// </summary>
        /// <param name="readWrite"></param>
        /// <param name="tablename"></param>
        /// <param name="valueObj"></param>
        /// <param name="trans"></param>
        public static void Ensure(this IReadWrite readWrite,
            string tablename,
            object valueObj,
            ITransaction trans = null)
        {
            Ensure(readWrite, tablename, valueObj._AsDictionary(), trans);
        }
    }
}
