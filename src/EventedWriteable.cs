using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    public class EventedWritable : IWriteable
    {
        public EventedWritable(IWriteable underlying)
        {
            _underlying = underlying;
        }

        protected IWriteable _underlying;

        /// <summary>
        /// Fires before an insert
        /// </summary>
        public event Action<string, Dictionary<string, object>> OnInsert;
        /// <summary>
        /// Fires before a delete
        /// </summary>
        public event Action<string, Dictionary<string, object>> OnDelete;
        /// <summary>
        /// Fires before an update
        /// </summary>
        public event Action<string, Dictionary<string, object>, Dictionary<string, object>> OnUpdate;
        /// <summary>
        /// Fires before every insert, update, or delete
        /// </summary>
        public event Action<string, Dictionary<string, object>> OnWrite;

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            if (OnInsert != null)
                OnInsert(tableName, row);
            if (OnWrite != null)
                OnWrite(tableName, row);
            return _underlying.Insert(tableName, row);
        }

        public void Delete(string tableName, Dictionary<string, object> row)
        {
            if (OnDelete != null)
                OnDelete(tableName, row);
            if (OnWrite != null)
                OnWrite(tableName, row);
            _underlying.Delete(tableName, row);
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            if (OnUpdate != null)
                OnUpdate(tableName, dataFields, keyFields);
            if (OnWrite != null)
                OnWrite(tableName, dataFields.Union(keyFields));
            _underlying.Update(tableName, dataFields, keyFields);
        }

        public IDBAnalyzer Analyzer
        {
            get { return _underlying.Analyzer; }
        }
    }
}
