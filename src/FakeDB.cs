﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;
using Nvelope;

namespace Lasy
{
    public class FakeDB : IReadable, IWriteable, IReadWrite
    {
        public Dictionary<string, FakeDBTable> DataStore = new Dictionary<string, FakeDBTable>();

        public FakeDBTable Table(string tableName)
        {
            if (DataStore.ContainsKey(tableName))
                return DataStore[tableName];
            else
                return new FakeDBTable();
        }

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> id, ITransaction transaction = null)
        {
            if (!DataStore.ContainsKey(tableName))
                return new List<Dictionary<string, object>>();

            return DataStore[tableName].FindByFieldValues(id);
        }

        public IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> id, ITransaction transaction = null)
        {
            return DataStore[tableName].FindByFieldValues(id).Select(row => row.WhereKeys(key => fields.Contains(key)));
        }

        public IEnumerable<Dictionary<string, object>> RawReadAll(string tableName, ITransaction transaction = null)
        {
            if (!DataStore.ContainsKey(tableName))
                return new List<Dictionary<string, object>>();

            return DataStore[tableName];
        }

        public IEnumerable<Dictionary<string, object>> RawReadAllCustomFields(string tableName, IEnumerable<string> fields, ITransaction transaction = null)
        {
            return DataStore[tableName].Select(row => row.WhereKeys(key => fields.Contains(key)));
        }

        private IDBAnalyzer _analyzer = new FakeDBAnalyzer();

        public IDBAnalyzer Analyzer
        {
            get { return _analyzer; }
            set { _analyzer = value; }
        }

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row, ITransaction transaction = null)
        {
            if (!DataStore.ContainsKey(tableName))
                DataStore.Add(tableName, new FakeDBTable());

            var table = DataStore[tableName];

            var dictToUse = row.Copy();
            //var id = DataStore[tableName].Count + 1;
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

        public void Delete(string tableName, Dictionary<string, object> fieldValues, ITransaction transaction = null)
        {
            if (DataStore.ContainsKey(tableName))
            {
                var victims = DataStore[tableName].FindByFieldValues(fieldValues).ToList();
                victims.ForEach(x => DataStore[tableName].Remove(x));
            }
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields, ITransaction transaction = null)
        {
            if(!DataStore.ContainsKey(tableName))
                return;

            var victims = DataStore[tableName].Where(r => r.IsSameAs(keyFields, keyFields.Keys))
                .Where(r => r != dataFields && r != keyFields); // Don't update if we've passed in the object itself,
                // because at that point the change has already been made by a sneaky back-door reference change,
                // and if we try to modify it here, we'll modify the collection while iterating over it, causing an exception
                // The non-hacky fix would be to return copies of the rows from the Read methods. That way, the user couldn't
                // make sneaky back-door changes. However, this would be a substantial performance penalty. Grr, I want
                // Clojure's persistent collections here...
            foreach (var vic in victims)
                foreach (var key in dataFields.Keys)
                    vic[key] = dataFields[key];                
        }

        public ITransaction BeginTransaction()
        {
            return new FakeDBTransaction();
        }
    }
}
