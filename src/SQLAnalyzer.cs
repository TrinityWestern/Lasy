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
                cacheDuration = _defaultCacheDuration();

            _connectionString = connectionString;
            _getAutonumberKey = new Func<string, string>(_getAutonumberKeyFromDB).Memoize(cacheDuration);
            _getFields = new Func<string, ICollection<string>>(_getFieldsFromDB).Memoize(cacheDuration);
            _getPrimaryKeys = new Func<string, ICollection<string>>(_getPrimaryKeysFromDB).Memoize(cacheDuration);
        }

        protected virtual TimeSpan _defaultCacheDuration()
        {
            return new TimeSpan(0, 10, 0);
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

        protected ICollection<string> _getPrimaryKeysFromDB(string tableName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                return conn.ExecuteSingleColumn<string>(_getPrimaryKeySql(), new { table = _tableOnly(tableName) });
            }
        }

        public string GetAutoNumberKey(string tableName)
        {
            return _getAutonumberKey(tableName);
        }

        protected string _getAutonumberKeyFromDB(string tableName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var res = conn.ExecuteSingleColumn<string>(_getAutonumberKeySql(), new { table = _tableOnly(tableName) });
                return res.FirstOr(null);
            }           
        }

        protected string _tableOnly(string tablename)
        {
            var res = tablename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last().ChopEnd("]").ChopStart("[");
            return res;
        }

        protected string _schemaOnly(string tablename)
        {
            var parts = tablename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ChopEnd("]").ChopStart("["));
            if (parts.Count() > 1)
                return parts.First();
            else
                return "";
        }

        public ICollection<string> GetFields(string tableName)
        {
            return _getFields(tableName);
        }

        protected ICollection<string> _getFieldsFromDB(string tableName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                return conn.ExecuteSingleColumn<string>(_getFieldsSql(), new { table = _tableOnly(tableName) });
            }
        }
    }
}
