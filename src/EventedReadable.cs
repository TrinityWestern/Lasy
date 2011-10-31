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
        public event Action<string, Dictionary<string, object>> OnRead;

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
        {
            if (OnRead != null)
                OnRead(tableName, keyFields);
            return _underlying.RawRead(tableName, keyFields, fields);
        }
        
        public IDBAnalyzer Analyzer
        {
            get { return _underlying.Analyzer; }
        }
    }
}
