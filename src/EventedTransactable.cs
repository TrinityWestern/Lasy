using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    /// <summary>
    /// Provides a wrapper around an IReadWrite that allows you to add events to operations.
    /// Helpful for logging, mocks, and that sort of thing
    /// </summary>
    public class EventedTransactable : ITransactable
    {
        public EventedTransactable(ITransactable underlyingDb)
        {
            _underlyingDb = underlyingDb;
        }

        protected ITransactable _underlyingDb;

        /// <summary>
        /// Fires before a read operation
        /// </summary>
        public event Action<string, Dictionary<string, object>> OnRead;
        /// <summary>
        /// Fires before an insert
        /// </summary>
        public event Action<string, Dictionary<string, object>> OnInsert;
        /// <summary>
        /// Fires before a delete
        /// </summary>
        public event Action<string, Dictionary<string, object>> OnDelete;
        /// <summary>
        /// Firest before an update
        /// </summary>
        public event Action<string, Dictionary<string, object>, Dictionary<string, object>> OnUpdate;
        /// <summary>
        /// Fired for every insert, update, or delete
        /// </summary>
        public event Action<string, Dictionary<string, object>> OnWrite;
        /// <summary>
        /// Fired every time a transaction is created
        /// </summary>
        public event Action OnBeginTransaction;

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields)
        {
            if (OnRead != null)
                OnRead(tableName, keyFields);
            return _underlyingDb.RawRead(tableName, keyFields, fields);
        }

        public IDBAnalyzer Analyzer
        {
            get { return _underlyingDb.Analyzer; }
        }

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            if (OnInsert != null)
                OnInsert(tableName, row);
            if (OnWrite != null)
                OnWrite(tableName, row);
            return _underlyingDb.Insert(tableName, row);
        }

        public void Delete(string tableName, Dictionary<string, object> row)
        {
            if (OnDelete != null)
                OnDelete(tableName, row);
            if(OnWrite != null)
                OnWrite(tableName, row);
            _underlyingDb.Delete(tableName, row);
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            if (OnUpdate != null)
                OnUpdate(tableName, dataFields, keyFields);
            if (OnWrite != null)
                OnWrite(tableName, dataFields.Union(keyFields));
            _underlyingDb.Update(tableName, dataFields, keyFields);
        }

        public ITransaction BeginTransaction()
        {
            if (OnBeginTransaction != null)
                OnBeginTransaction();
            return _underlyingDb.BeginTransaction();
        }
    }
}
