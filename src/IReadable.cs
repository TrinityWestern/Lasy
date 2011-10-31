using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
