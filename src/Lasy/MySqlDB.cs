using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Nvelope;
using System.Text.RegularExpressions;

namespace Lasy
{
    public class MySqlDB : SqlDB
    {
        public MySqlDB(string connectionString, SqlAnalyzer analyzer, bool strictTables = true)
            : base(connectionString, analyzer, strictTables)
        { }

        protected internal override System.Data.IDbConnection _getConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public override string QualifiedTable(string tablename)
        {
            var schema = SqlAnalyzer.SchemaName(tablename);
            var table = SqlAnalyzer.TableName(tablename);

            if (schema.IsNullOrEmpty())
                return tablename;
            else
                return "`" + schema + "`.`" + table + "`";
        }

        public override string GetInsertedAutonumber()
        {
            return "; SELECT LAST_INSERT_ID();";
        }

        #region Temporary Transaction fix
#warning Remove this once nested transaction issue fixed

        public override ITransaction BeginTransaction()
        {
            return new NonTransaction(this);
        }

        public class NonTransaction : ITransaction
        {
            public NonTransaction(MySqlDB db)
            {
                _db = db;
            }

            public MySqlDB _db;

            public void Commit() { }

            public void Rollback() { }

            public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
            {
                return _db.RawRead(tableName, keyFields, fields);
            }

            public IDBAnalyzer Analyzer
            {
                get { return _db.Analyzer; }
            }

            public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
            {
                return _db.Insert(tableName, row);
            }

            public void Delete(string tableName, Dictionary<string, object> keyFields)
            {
                _db.Delete(tableName, keyFields);
            }

            public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
            {
                _db.Update(tableName, dataFields, keyFields);
            }

            public void Dispose() { }
        }

        #endregion
    }
}
