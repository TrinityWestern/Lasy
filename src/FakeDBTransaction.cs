using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    /// <summary>
    /// Fakes a transaction on a FakeDB
    /// </summary>
    /// <remarks>Oh, the lies upon lies....</remarks>
    public class FakeDBTransaction : ITransaction
    {
        public FakeDBTransaction(FakeDB db)
        {
            _db = db;
            _operations = new List<Op>();
        }

        protected abstract class Op
        {
            public Op(string table) { Table = table; }
            public string Table { get; set; }
            public abstract FakeDBTable Apply(FakeDBTable table);
        }

        protected class InsertOp : Op
        {
            /// <summary>
            /// Note: row should include any autokeys that the table defines as well,
            /// since InsertOp cannot determine them internally
            /// </summary>
            /// <param name="table"></param>
            /// <param name="row"></param>
            public InsertOp(string table, Dictionary<string, object> row)
                : base(table)
            {
                Row = row.Copy();
            }

            public Dictionary<string, object> Row;

            public override FakeDBTable Apply(FakeDBTable table)
            {
                // We don't increment the NextAutoKey because that was assumed to have been done 
                // already when we created the InsertOp in the first place
                return new FakeDBTable(table.And(Row), table.NextAutoKey);
            }
        }

        protected class UpdateOp : Op
        {
            public UpdateOp(string table, Dictionary<string, object> data, Dictionary<string, object> keys)
                : base(table)
            {
                Keys = keys.Copy();
                NewValues = data.Copy();
            }

            public Dictionary<string, object> Keys;
            public Dictionary<string, object> NewValues;

            public override FakeDBTable Apply(FakeDBTable table)
            {
                var victims = table.Where(r => Keys.IsSameAs(r)).ToList();
                var newVersions = victims.Select(r => r.Union(NewValues));
                return new FakeDBTable(table.Except(victims).And(newVersions), table.NextAutoKey);
            }
        }

        protected class DeleteOp : Op
        {
            public DeleteOp(string table, Dictionary<string, object> keys)
                : base(table)
            {
                Keys = keys.Copy();
            }

            public Dictionary<string, object> Keys;

            public override FakeDBTable Apply(FakeDBTable table)
            {
                var victims = table.Where(r => Keys.IsSameAs(r));
                return new FakeDBTable(table.Except(victims), table.NextAutoKey);
            }
        }

        protected List<Op> _operations;

        protected FakeDB _db;

        public void Commit()
        {
            // Apply every operation in the transaction against the base database
            foreach (var op in _operations)
                _db.DataStore.Ensure(op.Table, op.Apply(_db.DataStore[op.Table]));
        }

        public void Rollback()
        {
            // Don't need to do anything
        }

        /// <summary>
        /// Gets a filtered version of the table, having applied all of the operations of the transaction to it
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        protected FakeDBTable _getTable(string table)
        {
            var underlying = _db.Table(table);
            var opsForTable = _operations.Where(o => o.Table == table);
            // Apply each of the operations to the table in sequence to get the output
            var res = opsForTable.Aggregate(underlying, (source,op) => op.Apply(source));

            return res;
        }

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> id)
        {
            return _getTable(tableName).Read(id);
        }

        public IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> id)
        {
            return _getTable(tableName).Read(id, fields);
        }

        public IDBAnalyzer Analyzer
        {
            get { return _db.Analyzer; }
        }

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            var autoKeys = _db.NewAutokey(tableName);
            var inserted = row.ScrubNulls().Union(autoKeys);
            
            var pks = _db.ExtractKeys(tableName, inserted);
            _operations.Add(new InsertOp(tableName, inserted));
            return pks;
        }

        public void Delete(string tableName, Dictionary<string, object> row)
        {
            _operations.Add(new DeleteOp(tableName, row.ScrubNulls()));
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            _operations.Add(new UpdateOp(tableName, dataFields.ScrubNulls(), keyFields.ScrubNulls()));
        }

        public void Dispose()
        {
            // Don't need to do anything
        }
    }
}
