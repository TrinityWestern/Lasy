using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;
using Nvelope;

namespace Lasy
{
    public class FakeDB : ITransactable, IRWEvented
    {
        public FakeDB()
            : this(new FakeDBMeta())
        { }

        public FakeDB(IDBAnalyzer analyzer)
        {
            Analyzer = analyzer;
        }

        public Dictionary<string, FakeDBTable> DataStore = new Dictionary<string, FakeDBTable>();

        public FakeDBTable Table(string tableName)
        {
            if (DataStore.ContainsKey(tableName))
                return DataStore[tableName];
            else
                return new FakeDBTable();
        }

        public virtual void Wipe()
        {
            DataStore = new Dictionary<string, FakeDBTable>();
        }

        public virtual IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
        {
            FireOnRead(tableName, keyFields);
            
            if (!DataStore.ContainsKey(tableName))
                return new List<Dictionary<string, object>>();

            return DataStore[tableName].Read(keyFields, fields);
        }

        private IDBAnalyzer _analyzer = new FakeDBMeta();

        public IDBAnalyzer Analyzer
        {
            get { return _analyzer; }
            set { _analyzer = value; }
        }

        public Dictionary<string, object> NewAutokey(string tableName)
        {
            if (!DataStore.ContainsKey(tableName))
                DataStore.Add(tableName, new FakeDBTable());

            var table = DataStore[tableName];

            var autoKey = Analyzer.GetAutoNumberKey(tableName);
            if (autoKey == null)
                return new Dictionary<string, object>();
            else
                return new Dictionary<string, object>() { { autoKey, table.NextAutoKey++ } };
        }

        public bool CheckKeys(string tableName, Dictionary<string, object> row)
        {
            try
            {
                var res = this.ExtractKeys(tableName, row);
                return true;
            }
            catch (KeyNotSetException)
            {
                return false;
            }
        }

        public virtual Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            FireOnInsert(tableName, row);

            if (!DataStore.ContainsKey(tableName))
                DataStore.Add(tableName, new FakeDBTable());
            
            var table = DataStore[tableName];

            row = row.ScrubNulls();

            var autoKeys = NewAutokey(tableName);
            var dictToUse = row.Union(autoKeys);
            CheckKeys(tableName, dictToUse);
            table.Add(dictToUse);

            return this.ExtractKeys(tableName, dictToUse);
        }

        public virtual void Delete(string tableName, Dictionary<string, object> fieldValues)
        {
            FireOnDelete(tableName, fieldValues);

            if (DataStore.ContainsKey(tableName))
            {
                fieldValues = fieldValues.ScrubNulls();
                var victims = DataStore[tableName].FindByFieldValues(fieldValues).ToList();
                victims.ForEach(x => DataStore[tableName].Remove(x));
            }
        }

        public virtual void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            FireOnUpdate(tableName, dataFields, keyFields);

            if(!DataStore.ContainsKey(tableName))
                return;

            dataFields = dataFields.ScrubNulls();
            keyFields = keyFields.ScrubNulls();

            var victims = DataStore[tableName].Where(r => r.IsSameAs(keyFields, keyFields.Keys, null));
            foreach (var vic in victims)
                foreach (var key in dataFields.Keys)
                    vic[key] = dataFields[key];                
        }

        public virtual  ITransaction BeginTransaction()
        {
            return new FakeDBTransaction(this);
        }

        public void FireOnInsert(string tableName, Dictionary<string, object> keyFields)
        {
            if (OnInsert != null)
                OnInsert(tableName, keyFields);
            if (OnWrite != null)
                OnWrite(tableName, keyFields);
        }

        public void FireOnDelete(string tableName, Dictionary<string, object> fieldValues)
        {
            if (OnDelete != null)
                OnDelete(tableName, fieldValues);
            if (OnWrite != null)
                OnWrite(tableName, fieldValues);
        }

        public void FireOnUpdate(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            if (OnUpdate != null)
                OnUpdate(tableName, dataFields, keyFields);
            if (OnWrite != null)
                OnWrite(tableName, dataFields.Union(keyFields));
        }

        public void FireOnRead(string tableName, Dictionary<string, object> keyFields)
        {
            if (OnRead != null)
                OnRead(tableName, keyFields);
        }

        public event Action<string, Dictionary<string, object>> OnInsert;

        public event Action<string, Dictionary<string, object>> OnDelete;

        public event Action<string, Dictionary<string, object>, Dictionary<string, object>> OnUpdate;

        public event Action<string, Dictionary<string, object>> OnWrite;

        public event Action<string, Dictionary<string, object>> OnRead;
    }
}
