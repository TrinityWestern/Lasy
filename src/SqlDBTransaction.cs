using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Lasy
{
    public class SqlDBTransaction : AbstractSqlReadWrite, ITransaction
    {
        public SqlDB UnderlyingDB;
        protected SqlConnection _conn;
        protected SqlTransaction _transaction;

        public SqlDBTransaction(SqlDB db)
            : base(db.ConnectionString, db.Analyzer)
        {
            UnderlyingDB = db;
            _conn = new SqlConnection(db.ConnectionString);
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

        protected override IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            var command = new SqlCommand(sql, _conn, _transaction);
            return command.Execute(sql, values);

        }

        protected override int? sqlInsert(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            var command = new SqlCommand(sql, _conn, _transaction);
            return command.ExecuteSingleValue<int?>(sql, values);
        }

        protected override void sqlUpdate(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            var command = new SqlCommand(sql, _conn, _transaction);
            command.Execute(sql, values);
           
        }

        public void Dispose()
        {
            _conn.Dispose();
        }
    }
}
