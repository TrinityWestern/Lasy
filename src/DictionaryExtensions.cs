using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Make sure that DBNull.Value is converted to null, so that we treat DBNull and null the same
        /// on the backend
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ScrubNulls(this Dictionary<string, object> values)
        {
            var fields = values.Where(kv => kv.Value == DBNull.Value).Select(kv => kv.Key);
            if (!fields.Any())
                return values;
            var res = values.Copy();
            fields.Each(f => res[f] = null);
            return res;
        }
    }
}
