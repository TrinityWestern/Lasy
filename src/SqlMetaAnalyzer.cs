using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;
using Nvelope.Reactive;
using Nvelope.Reflection;
using System.Reactive.Linq;
using System.Reactive;

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
            // We can do that by replacing the function implementation with our own.
            // We want to use the base class implementation unless we know that we've created
            // the table - if we have, we want to make sure we clear the cache and go
            // down to the database to get it

            // Ok, this is some cool magic from the System.Reactive lib by MS
            // We create an IObservable<string> that encapsulates our stream of table
            // creation events. The, we can pass that stream to Memoize to allow it to 
            // invalidate the cache. 
            // Semantically, this is ... = Observable.FromEvent(TableCreated), but unfortunately
            // events aren't first-class in C#, so the syntax doesn't allow for this. The line below 
            // is as good as they could make it.
            var tableCreatedStream = Observable.FromEvent<string>(del => TableCreated += del, del => TableCreated -= del);

            _getFields = new Func<string, ICollection<string>>(_getFieldsFromDB).Memoize(cacheDuration, tableCreatedStream);
            _getAutonumberKey = new Func<string, string>(_getAutonumberKeyFromDB).Memoize(cacheDuration, tableCreatedStream);
            _getPrimaryKeys = new Func<string, ICollection<string>>(_getPrimaryKeysFromDB).Memoize(cacheDuration, tableCreatedStream);
            // Do the same for our own _tableExists function
            _tableExists = new Func<string, bool>(_tableExistsFromDB).Memoize(cacheDuration, tableCreatedStream);
        }

        protected abstract string _getTableExistsSql(string schema, string table);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="fields">A mapping of the fieldname to .NET type of the fields</param>
        /// <returns></returns>
        protected abstract string _getCreateTableSql(string schema, string table, Dictionary<string, Type> fields);

        protected Func<string, bool> _tableExists;
        protected Action<string> _createTable;

        /// <summary>
        /// Fired every time a table is created
        /// </summary>
        public event Action<string> TableCreated;

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
                var sql = _getTableExistsSql(schema, table);
                var paras = new { table = table, schema = schema };
                return conn.ExecuteSingleValueOr<bool>(false, sql, paras);
            }
        }

        public void CreateTable(string tablename, Dictionary<string, object> fields)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var table = _tableOnly(tablename);
                var schema = _schemaOnly(tablename);
                var fieldTypes = fields.SelectVals(v => v._AsType());
                var sql = _getCreateTableSql(schema, table, fieldTypes);
                var paras = new { table = table, schema = schema };
                conn.Execute(sql, paras);
                TableCreated(tablename);
            }
        }
        
    }
}
