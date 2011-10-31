using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface IWriteable : IAnalyzable
    {
        Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row);

        void Delete(string tableName, Dictionary<string, object> keyFields);

        void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields);
    }
}
