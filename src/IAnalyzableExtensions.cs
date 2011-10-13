using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
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
    }
}
