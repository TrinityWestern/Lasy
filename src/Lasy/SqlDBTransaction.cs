using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Lasy
{
    public class SqlDBTransaction : AbstractSqlReadWrite, ITransaction
    {   
        protected IDbConnection _conn;
        protected IDbTransaction _transaction;

        public SqlDBTransaction(SqlDB db)
            : base(db.ConnectionString, db.SqlAnalyzer, db.StrictTables)
        {
            _conn = db._getConnection();
            _conn.Open();
            _transaction = _conn.BeginTransaction();
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        protected IDbCommand _getCommand(string sql)
        {
            var command = _conn.CreateCommand();
            command.CommandText = sql;
            command.Transaction = _transaction;
            return command;
        }

        protected override IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            var command = _getCommand(sql);
            return command.Execute(sql, values);
        }

        protected override int? sqlInsert(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            var command = _getCommand(sql);
            return command.ExecuteSingleValue<int?>(sql, values);
        }

        protected override void sqlUpdate(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            var command = _getCommand(sql);
            command.Execute(sql, values);
           
        }

        public void Dispose()
        {
            _conn.Dispose();
        }
    }
}
