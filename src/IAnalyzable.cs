using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    public interface IAnalyzable
    {
        /// <summary>
        /// Get the analyzer for the DB
        /// </summary>
        IDBAnalyzer Analyzer { get; }
    }

    public static class IAnalyzableExtensions
    {
        /// <summary>
        /// Gets all the primary keys for tablename from values. Throws an exception if any of the keys
        /// are not supplied
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ExtractKeys(this IAnalyzable writer, string tablename,
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
        /// Make sure that values contains all the keys needed to insert into tablename. If not, 
        /// a KeyNotSetException will be thrown
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tablename"></param>
        /// <param name="values"></param>
        public static void AssertInsertKeys(this IAnalyzable writer, string tablename, Dictionary<string, object> values)
        {
            var keys = writer.Analyzer.GetPrimaryKeys(tablename).Except(writer.Analyzer.GetAutoNumberKey(tablename));
            if (!values.Keys.ToSet().IsSupersetOf(keys))
                throw new KeyNotSetException(tablename, keys.Except(values.Keys));
        }
    }
}
