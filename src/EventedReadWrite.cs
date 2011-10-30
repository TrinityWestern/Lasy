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
    public class EventedReadWrite : IReadWrite
    {
        public EventedReadWrite(IReadWrite underlyingDb)
        {
            _underlyingDb = underlyingDb;
        }

        protected IReadWrite _underlyingDb;

        /// <summary>
        /// Fires before a read operation
        /// </summary>
        public event Action<string, Dictionary<string, object>, ITransaction> OnRead;
        /// <summary>
        /// Fires before an insert
        /// </summary>
        public event Action<string, Dictionary<string, object>, ITransaction> OnInsert;
        /// <summary>
        /// Fires before a delete
        /// </summary>
        public event Action<string, Dictionary<string, object>, ITransaction> OnDelete;
        /// <summary>
        /// Firest before an update
        /// </summary>
        public event Action<string, Dictionary<string, object>, Dictionary<string, object>, ITransaction> OnUpdate;
        /// <summary>
        /// Fired for every insert, update, or delete
        /// </summary>
        public event Action<string, Dictionary<string, object>, ITransaction> OnWrite;
        /// <summary>
        /// Fires before beginning a transaction
        /// </summary>
        public event Action OnBeginTransaction;

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> id, ITransaction transaction = null)
        {
            if (OnRead != null)
                OnRead(tableName, id, transaction);
            return _underlyingDb.RawRead(tableName, id, transaction);
        }

        public IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> id, ITransaction transaction = null)
        {
            if (OnRead != null)
                OnRead(tableName, id, transaction);
            return _underlyingDb.RawReadCustomFields(tableName, fields, id, transaction);
        }

        public IDBAnalyzer Analyzer
        {
            get { return _underlyingDb.Analyzer; }
        }

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row, ITransaction transaction = null)
        {
            if (OnInsert != null)
                OnInsert(tableName, row, transaction);
            if (OnWrite != null)
                OnWrite(tableName, row, transaction);
            return _underlyingDb.Insert(tableName, row, transaction);
        }

        public void Delete(string tableName, Dictionary<string, object> row, ITransaction transaction = null)
        {
            if (OnDelete != null)
                OnDelete(tableName, row, transaction);
            if(OnWrite != null)
                OnWrite(tableName, row, transaction);
            _underlyingDb.Delete(tableName, row, transaction);
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields, ITransaction transaction = null)
        {
            if (OnUpdate != null)
                OnUpdate(tableName, dataFields, keyFields, transaction);
            if (OnWrite != null)
                OnWrite(tableName, dataFields.Union(keyFields), transaction);
            _underlyingDb.Update(tableName, dataFields, keyFields, transaction);
        }

        public ITransaction BeginTransaction()
        {
            if (OnBeginTransaction != null)
                OnBeginTransaction();
            return _underlyingDb.BeginTransaction();
        }
    }
}
