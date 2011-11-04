using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;


namespace Lasy
{
    public abstract class SqlMetaAnalyzer : SQLAnalyzer
    {
        public SqlMetaAnalyzer(string connectionString, TimeSpan cacheDuration = default(TimeSpan))
            : base(connectionString, cacheDuration)
        {
            if (cacheDuration == default(TimeSpan))
                cacheDuration = _defaultCacheDuration();

            // We'll need to change the base class' underlying _getPrimaryKeys, etc methods
            // Right now, they cache the results. However, if they cache en empty PK list for
            // a table that doesn't exist, and then we create the table, we want to clear that
            // cache so it gets the updated values.
            // We can do that by replacing the function implementation with out own.
            // We want to use the base class implementation unless we know that we've created
            // the table within the cache duration - if we have, we want to make sure we go
            // down to the database to get it


#error These don't work the way you want them to
            _getFields = new Func<string, bool>(_recentlyCreated).Dispatch(
                _getFieldsFromDB,
                new Func<string, ICollection<string>>(_getFieldsFromDB).Memoize(cacheDuration));

            _getAutonumberKey = new Func<string, bool>(_recentlyCreated).Dispatch(
                _getAutonumberKeyFromDB,
                new Func<string, string>(_getAutonumberKeyFromDB).Memoize(cacheDuration));

            _getPrimaryKeys = new Func<string,bool>(_recentlyCreated).Dispatch(
                _getPrimaryKeysFromDB,
                new Func<string,ICollection<string>>(_getPrimaryKeysFromDB).Memoize(cacheDuration));


            // Do the same caching with our own _tableExists function
            _tableExists = new Func<string, bool>(_recentlyCreated).Dispatch(
                _tableExistsFromDB,
                new Func<string,bool>(_tableExistsFromDB).Memoize(cacheDuration));



        }

        protected abstract string _getTableExistsSql(string schema, string table);
        protected abstract string _getCreateTableSql(string schema, string table);

        protected Func<string, bool> _tableExists;
        protected Action<string> _createTable;

        protected bool _recentlyCreated(string tablename)
        {
            return false;
        }

        public bool TableExists(string tablename)
        {
            return _tableExists(tablename);
        }

        protected bool _tableExistsFromDB(string tablename)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var table = _tableOnly(tablename);
                var schema = _schemaOnly(tablename);
                return conn.ExecuteSingleValue<bool>(_getTableExistsSql(schema, table), new { table = table, schema = schema });
            }
        }

        public void CreateTable(string tablename)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var table = _tableOnly(tablename);
                var schema = _schemaOnly(tablename);
                conn.Execute(_getCreateTableSql(schema, table), new { table = table, schema = schema });
            }
        }
        
    }
}
