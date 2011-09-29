using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface IWriteable
    {
        Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row, ITransaction transaction = null);

        void Delete(string tableName, Dictionary<string, object> row, ITransaction transaction = null);

        void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields, ITransaction transaction = null);

        IDBAnalyzer Analyzer { get; }

        ITransaction BeginTransaction();
    }
}
