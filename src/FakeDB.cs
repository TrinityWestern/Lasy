using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;
using Nvelope;

namespace Lasy
{
    public class FakeDB : IReadable, IWriteable, IReadWrite
    {
        public FakeDB()
        {
        }

        public FakeDB(IDBAnalyzer analyzer) : this()
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

        public virtual IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> id, ITransaction transaction = null)
        {
            if (!DataStore.ContainsKey(tableName))
                return new List<Dictionary<string, object>>();

            id = id.ScrubNulls();

            return DataStore[tableName].FindByFieldValues(id)
                .Select(d => d.Copy());
        }

        public virtual IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> id, ITransaction transaction = null)
        {
            id = id.ScrubNulls();
            return DataStore[tableName].FindByFieldValues(id).Select(row => row.WhereKeys(key => fields.Contains(key)))
                .Select(d => d.Copy());
        }

        public virtual IEnumerable<Dictionary<string, object>> RawReadAll(string tableName, ITransaction transaction = null)
        {
            if (!DataStore.ContainsKey(tableName))
                return new List<Dictionary<string, object>>();

            return DataStore[tableName].Select(d => d.Copy());
        }

        public virtual IEnumerable<Dictionary<string, object>> RawReadAllCustomFields(string tableName, IEnumerable<string> fields, ITransaction transaction = null)
        {
            return DataStore[tableName].Select(row => row.WhereKeys(key => fields.Contains(key)));
        }

        private IDBAnalyzer _analyzer = new FakeDBAnalyzer();

        public IDBAnalyzer Analyzer
        {
            get { return _analyzer; }
            set { _analyzer = value; }
        }

        public virtual Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row, ITransaction transaction = null)
        {
            if (!DataStore.ContainsKey(tableName))
                DataStore.Add(tableName, new FakeDBTable());

            row = row.ScrubNulls();

            var table = DataStore[tableName];

            var dictToUse = row.Copy();
            var primaryKeys = Analyzer.GetPrimaryKeys(tableName);
            var autoKey = Analyzer.GetAutoNumberKey(tableName);

            if (autoKey != null)
            {
                if (!dictToUse.ContainsKey(autoKey))
                    dictToUse.Add(autoKey, table.NextAutoKey++);
                else
                    dictToUse[autoKey] = table.NextAutoKey++;
            }

            var invalid = primaryKeys.Where(key => dictToUse[key] == null);
            if (invalid.Any())
                throw new KeyNotSetException(tableName, invalid);

            table.Add(dictToUse);
            return dictToUse.WhereKeys(key => primaryKeys.Contains(key));
        }

        public virtual void Delete(string tableName, Dictionary<string, object> fieldValues, ITransaction transaction = null)
        {
            if (DataStore.ContainsKey(tableName))
            {
                fieldValues = fieldValues.ScrubNulls();
                var victims = DataStore[tableName].FindByFieldValues(fieldValues).ToList();
                victims.ForEach(x => DataStore[tableName].Remove(x));
            }
        }

        public virtual void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields, ITransaction transaction = null)
        {
            if(!DataStore.ContainsKey(tableName))
                return;

            dataFields = dataFields.ScrubNulls();
            keyFields = keyFields.ScrubNulls();

            var victims = DataStore[tableName].Where(r => r.IsSameAs(keyFields, keyFields.Keys));
            foreach (var vic in victims)
                foreach (var key in dataFields.Keys)
                    vic[key] = dataFields[key];                
        }

        public virtual  ITransaction BeginTransaction()
        {
            return new FakeDBTransaction();
        }
    }
}
