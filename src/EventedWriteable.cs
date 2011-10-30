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
        public event Action<string, Dictionary<string, object>, ITransaction> OnInsert;
        /// <summary>
        /// Fires before a delete
        /// </summary>
        public event Action<string, Dictionary<string, object>, ITransaction> OnDelete;
        /// <summary>
        /// Fires before an update
        /// </summary>
        public event Action<string, Dictionary<string, object>, Dictionary<string, object>, ITransaction> OnUpdate;
        /// <summary>
        /// Fires before every insert, update, or delete
        /// </summary>
        public event Action<string, Dictionary<string, object>, ITransaction> OnWrite;
        /// <summary>
        /// Fires before beginning a transaction
        /// </summary>
        public event Action OnBeginTransaction;

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row, ITransaction transaction = null)
        {
            if (OnInsert != null)
                OnInsert(tableName, row, transaction);
            if (OnWrite != null)
                OnWrite(tableName, row, transaction);
            return _underlying.Insert(tableName, row, transaction);
        }

        public void Delete(string tableName, Dictionary<string, object> row, ITransaction transaction = null)
        {
            if (OnDelete != null)
                OnDelete(tableName, row, transaction);
            if (OnWrite != null)
                OnWrite(tableName, row, transaction);
            _underlying.Delete(tableName, row, transaction);
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields, ITransaction transaction = null)
        {
            if (OnUpdate != null)
                OnUpdate(tableName, dataFields, keyFields, transaction);
            if (OnWrite != null)
                OnWrite(tableName, dataFields.Union(keyFields), transaction);
            _underlying.Update(tableName, dataFields, keyFields, transaction);
        }

        public ITransaction BeginTransaction()
        {
            if (OnBeginTransaction != null)
                OnBeginTransaction();
            return _underlying.BeginTransaction();
        }

        public IDBAnalyzer Analyzer
        {
            get { return _underlying.Analyzer; }
        }
    }
}
