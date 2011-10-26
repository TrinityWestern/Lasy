using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lasy;

namespace Lasy
{
    public class UnreliableDb : FakeDB
    {
        public UnreliableDb()
            : base()
        { }

        public UnreliableDb(IDBAnalyzer analyzer)
            : base(analyzer)
        { }

        public bool FailOnNextOp = false;

        protected void _checkFail()
        {
            if (FailOnNextOp)
            {
                FailOnNextOp = false;
                throw new MockDBFailure();
            }
        }

        public override void Delete(string tableName, Dictionary<string, object> fieldValues, ITransaction transaction = null)
        {
            _checkFail();
            base.Delete(tableName, fieldValues, transaction);
        }

        public override Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row, ITransaction transaction = null)
        {
            _checkFail();
            return base.Insert(tableName, row, transaction);
        }

        public override IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> id, ITransaction transaction = null)
        {
            _checkFail();
            return base.RawRead(tableName, id, transaction);
        }

        public override IEnumerable<Dictionary<string, object>> RawReadAll(string tableName, ITransaction transaction = null)
        {
            _checkFail();
            return base.RawReadAll(tableName, transaction);
        }

        public override IEnumerable<Dictionary<string, object>> RawReadAllCustomFields(string tableName, IEnumerable<string> fields, ITransaction transaction = null)
        {
            _checkFail();
            return base.RawReadAllCustomFields(tableName, fields, transaction);
        }

        public override IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> id, ITransaction transaction = null)
        {
            _checkFail();
            return base.RawReadCustomFields(tableName, fields, id, transaction);
        }

        public override void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields, ITransaction transaction = null)
        {
            _checkFail();
            base.Update(tableName, dataFields, keyFields, transaction);
        }
    }
}
