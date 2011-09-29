using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;

namespace Lasy
{
    public abstract class SQLAnalyzer : IDBAnalyzer
    {
        public SQLAnalyzer(string connectionString, TimeSpan cacheDuration = default(TimeSpan))
        {
            if(cacheDuration == default(TimeSpan))
                cacheDuration = new TimeSpan(0,10,0);

            _connectionString = connectionString;
            _getAutonumberKey = new Func<string, string>(_s_getAutonumberKey).Memoize(cacheDuration);
            _getFields = new Func<string, ICollection<string>>(_s_getFields).Memoize(cacheDuration);
            _getPrimaryKeys = new Func<string, ICollection<string>>(_s_getPrimaryKeys).Memoize(cacheDuration);

        }

        protected Func<string, ICollection<string>> _getPrimaryKeys;
        protected Func<string, string> _getAutonumberKey;
        protected Func<string, ICollection<string>> _getFields;
        protected string _connectionString;

        protected abstract string _getPrimaryKeySql();
        protected abstract string _getAutonumberKeySql();
        protected abstract string _getFieldsSql();

        public ICollection<string> GetPrimaryKeys(string tableName)
        {
            return _getPrimaryKeys(tableName);
        }

        private ICollection<string> _s_getPrimaryKeys(string tableName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                return conn.ExecuteSingleColumn<string>(_getPrimaryKeySql(), new { table = _stripSchema(tableName) });
            }
        }

        public string GetAutoNumberKey(string tableName)
        {
            return _getAutonumberKey(tableName);
        }

        private string _s_getAutonumberKey(string tableName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var res = conn.ExecuteSingleColumn<string>(_getAutonumberKeySql(), new { table = _stripSchema(tableName) });
                return res.FirstOr(null);
            }           
        }

        private string _stripSchema(string tablename)
        {
            var res = tablename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last().ChopEnd("]").ChopStart("[");
            return res;
        }

        public ICollection<string> GetFields(string tableName)
        {
            return _getFields(tableName);
        }

        protected ICollection<string> _s_getFields(string tableName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                return conn.ExecuteSingleColumn<string>(_getFieldsSql(), new { table = _stripSchema(tableName) });
            }
        }
    }
}
