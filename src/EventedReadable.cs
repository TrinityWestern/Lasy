using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public class EventedReadable : IReadable
    {
        public EventedReadable(IReadable underlyingReadable)
        {
            _underlying = underlyingReadable;
        }

        protected IReadable _underlying;

        /// <summary>
        /// Fires before a Read operation
        /// </summary>
        public event Action<string, Dictionary<string, object>, ITransaction> OnRead;

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> id, ITransaction transaction = null)
        {
            if (OnRead != null)
                OnRead(tableName, id, transaction);
            return _underlying.RawRead(tableName, id, transaction);
        }

        public IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> id, ITransaction transaction = null)
        {
            if (OnRead != null)
                OnRead(tableName, id, transaction);
            return _underlying.RawReadCustomFields(tableName, fields, id, transaction);
        }

        public IDBAnalyzer Analyzer
        {
            get { return _underlying.Analyzer; }
        }
    }
}
