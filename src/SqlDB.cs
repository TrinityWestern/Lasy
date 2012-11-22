using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using System.Data;

namespace Lasy
{
    /// <summary>
    /// Provides an implementation of ITransactable for MS SQL Server
    /// </summary>
    public class SqlDB : AbstractSqlReadWrite, ITransactable
    {
        public SqlDB(string connectionString, SqlAnalyzer analyzer, bool strictTables = true)
            : base(connectionString, analyzer, strictTables)
        { }

        protected internal virtual IDbConnection _getConnection()
        {
            return new System.Data.SqlClient.SqlConnection(ConnectionString);
        }

        protected override IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = _getConnection())
            {
                return conn.Execute(sql, values);
            }
        }

        protected override int? sqlInsert(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = _getConnection())
            {
                return conn.ExecuteSingleValue<int?>(sql, values);
            }
        }

        protected override void sqlUpdate(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = _getConnection())
            {
                conn.Execute(sql, values);
            }
        }

        public virtual ITransaction BeginTransaction()
        {
            return new SqlDBTransaction(this);
        }
    }
}
