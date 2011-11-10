using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;

namespace Lasy
{
    /// <summary>
    /// Provides an implementation of ITransactable for MS SQL Server
    /// </summary>
    public class SqlDB : AbstractSqlReadWrite, ITransactable
    {
        public SqlDB(string connectionString, IDBAnalyzer analyzer, bool strictTables = true)
            : base(connectionString, analyzer, strictTables)
        { }

        protected override IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                return conn.Execute(sql, values);
            }
        }

        protected override int? sqlInsert(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                return conn.ExecuteSingleValue<int?>(sql, values);
            }
        }

        protected override void sqlUpdate(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(sql, values);
            }
        }

        public ITransaction BeginTransaction()
        {
            return new SqlDBTransaction(this);
        }
    }
}
