using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using System.Data;
using System.Reactive.Linq;
using System.Data.Common;

namespace Lasy
{
    public class SqlAnalyzer : ITypedDBAnalyzer
    {
        public SqlAnalyzer(string connectionString, INameQualifier nameQualifier = null, TimeSpan cacheDuration = default(TimeSpan))
        {
            nameQualifier = nameQualifier ?? new SqlNameQualifier();

            if(cacheDuration == default(TimeSpan))
                cacheDuration = _defaultCacheDuration();

            _connectionString = connectionString;
            NameQualifier = nameQualifier;

            // We use function references instead of directly exposing the functions so 
            // that we can build in caching without much work.
            // We'll do caching using Memoize - it'll cache the results of the function
            // for as long as cacheDuration.
            // Also, in some subclasses (ie, SqlMetaAnalyzer), we implement a different
            // caching scheme - using these function references lets us do that without 
            // having to change anything here.

            // Why didn't we just do this through polymorphism, you ask?
            // Well, if we did, we wouldn't be able to compose our functions easily - we can't 
            // override a function and just say "hey, use a memoized version of this function instead
            // of the base-class version" - we'd have to implement memoization from scratch in each
            // method. That's just a silly waste of time. Also, when we subclass, we'd have to implement
            // all of the memoization and cache invalidation we do in SqlMetaAnalyzer for each of these
            // methods again! Polymorphism doesn't allow us to do any manipulation of our functions - all you
            // can do is reimplement them, you can't get at the underlying binding and change it. That is to say,
            // there's no way using override to say "replace this method with a memoized version of it" - all you 
            // can do is implement the guts of memoize inside your function, and repeat it for every function
            // you want to do the same thing to.
            var schemaEvents = Observable.FromEvent<string>(d => OnInvalidateSchemaCache += d, d => OnInvalidateSchemaCache -= d);
            var tableEvents = Observable.FromEvent<string>(d => OnInvalidateTableCache += d, d => OnInvalidateTableCache -= d);

            _getAutonumberKey = new Func<string, string>(_getAutonumberKeyFromDB).Memoize(cacheDuration, tableEvents);
            _getFieldTypes = new Func<string, Dictionary<string,SqlColumnType>>(_getFieldTypesFromDB).Memoize(cacheDuration, tableEvents);
            _getPrimaryKeys = new Func<string, ICollection<string>>(_getPrimaryKeysFromDB).Memoize(cacheDuration, tableEvents);
            _tableExists = new Func<string, bool>(_tableExistsFromDB).Memoize(cacheDuration, tableEvents);
            _schemaExists = new Func<string, bool>(_schemaExistsFromDb).Memoize(cacheDuration, schemaEvents);
        }

        protected virtual TimeSpan _defaultCacheDuration()
        {
            return new TimeSpan(0, 10, 0);
        }

        protected Func<string, ICollection<string>> _getPrimaryKeys;
        protected Func<string, string> _getAutonumberKey;
        protected Func<string, Dictionary<string,SqlColumnType>> _getFieldTypes;
        protected Func<string, bool> _tableExists;
        protected Func<string, bool> _schemaExists;
        protected string _connectionString;
        public INameQualifier NameQualifier { get; private set; }

        protected event Action<string> OnInvalidateTableCache;
        protected event Action<string> OnInvalidateSchemaCache;
        /// <summary>
        /// Call this to indicate that information for a cached table is no longer valid
        /// </summary>
        /// <param name="tablename"></param>
        public void InvalidateTableCache(string tablename)
        {
            if (OnInvalidateTableCache != null)
                OnInvalidateTableCache(tablename);
        }
        /// <summary>
        /// Call this to indicate that information for a cached schema is no longer valid
        /// </summary>
        /// <param name="schema"></param>
        public void InvalidateSchemaCache(string schema)
        {
            if (OnInvalidateSchemaCache != null)
                OnInvalidateSchemaCache(schema);
        }

        protected internal virtual IDbConnection _getConnection(string connectionString)
        {
            return new System.Data.SqlClient.SqlConnection(connectionString);
        }

        protected internal virtual string _getPrimaryKeySql()
        {
            return @"select isc.Column_name
                    from 
                    sys.columns c inner join sys.tables t on c.object_id = t.object_id 
                    inner join information_schema.columns isc 
                    on schema_id(isc.TABLE_SCHEMA) = t.schema_id and isc.TABLE_NAME = t.name and isc.COLUMN_NAME = c.name 
                    left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu
                    on cu.TABLE_SCHEMA = isc.TABLE_SCHEMA and cu.TABLE_NAME = isc.TABLE_NAME and cu.COLUMN_NAME = isc.COLUMN_NAME
                    left join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    on cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    where isc.TABLE_NAME = @table and isc.TABLE_SCHEMA = @schema and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    order by ORDINAL_POSITION";
        }

        protected internal virtual string _getAutonumberKeySql()
        {
            return @"select isc.Column_name
                    from 
                    sys.columns c inner join sys.tables t on c.object_id = t.object_id 
                    inner join information_schema.columns isc 
                    on schema_id(isc.TABLE_SCHEMA) = t.schema_id and isc.TABLE_NAME = t.name and isc.COLUMN_NAME = c.name 
                    left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu
                    on cu.TABLE_SCHEMA = isc.TABLE_SCHEMA and cu.TABLE_NAME = isc.TABLE_NAME and cu.COLUMN_NAME = isc.COLUMN_NAME
                    left join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    on cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    where isc.TABLE_NAME = @table and isc.TABLE_SCHEMA = @schema and is_identity = 1
                    order by ORDINAL_POSITION";
        }

        protected internal virtual string _getTableExistsSql(string schema, string table)
        {
            //return "select 1 from sys.tables where name = @table union all select 1 from sys.views where name = @table";
            return @"SELECT 1
                    FROM sys.tables 
	                    LEFT JOIN sys.schemas ON tables.schema_id = schemas.schema_id
                    WHERE tables.name = @table 
	                    AND schemas.name = @schema
                    UNION ALL 
                    SELECT 1 
                    FROM sys.views 
	                    LEFT JOIN sys.schemas ON views.schema_id = schemas.schema_id
                    WHERE views.name = @table
	                    AND schemas.name = @schema";
        }

        protected internal virtual string _getFieldTypeSql()
        {
            return @"SELECT     
                    isc.*
                FROM 
					sys.objects tbl
					inner join sys.schemas schemas
					on tbl.schema_id = schemas.schema_id
                    inner join sys.columns c
                    on tbl.object_id = c.object_id
                    inner join information_schema.columns isc
                    on isc.column_name = c.name and isc.table_name = tbl.name and isc.table_schema = schemas.name
                    left outer join information_schema.key_column_usage k
                    on k.table_name = tbl.name and objectproperty(object_id(constraint_name), 'IsPrimaryKey') = 1
                        and k.column_name = c.name
                WHERE 
                    tbl.name = @table
					and schemas.name = @schema 
                order by isc.ORDINAL_POSITION";
        }

        protected internal virtual string _getSchemaExistsSql()
        {
            return "select 1 from sys.schemas where name = @schema";
        }

        public ICollection<string> GetPrimaryKeys(string tableName)
        {
            return _getPrimaryKeys(tableName);
        }

        protected ICollection<string> _getPrimaryKeysFromDB(string tableName)
        {
            using (var conn = _getConnection(_connectionString))
            {
                return conn.ExecuteSingleColumn<string>(_getPrimaryKeySql(), new { table = TableName(tableName), schema = SchemaName(tableName) });
            }
        }

        public string GetAutoNumberKey(string tableName)
        {
            return _getAutonumberKey(tableName);
        }

        protected string _getAutonumberKeyFromDB(string tableName)
        {
            using (var conn = _getConnection(_connectionString))
            {
                var res = conn.ExecuteSingleColumn<string>(_getAutonumberKeySql(), new { table = TableName(tableName), schema = SchemaName(tableName) });
                return res.SingleOr(null);
            }           
        }

        public Dictionary<string, SqlColumnType> GetFieldTypes(string tablename, Dictionary<string,object> example = null)
        {
            example = example ?? new Dictionary<string, object>();

            var exampleFields = example.SelectVals(v => SqlTypeConversion.GetSqlType(v));
            var sqlFields = _getFieldTypes(tablename);
            var res = exampleFields.Union(sqlFields);
            return res;
        }

        public ICollection<string> GetFields(string tableName)
        {
            return GetFieldTypes(tableName).Keys;
        }

        protected Dictionary<string, SqlColumnType> _getFieldTypesFromDB(string tableName)
        {
            using (var conn = _getConnection(_connectionString))
                return _convertTypes(conn.Execute(_getFieldTypeSql(), new { table = TableName(tableName), schema = SchemaName(tableName) }));
        }

        protected Dictionary<string, SqlColumnType> _convertTypes(ICollection<Dictionary<string, object>> sysobjectsInfos)
        {
            return sysobjectsInfos.ToDictionary(
                row => row["COLUMN_NAME"].ToString(),
                row => _determineType(row));
        }

        protected SqlColumnType _determineType(Dictionary<string, object> sysobjectInfo)
        {
            // Fun fact - for longtext fields, MySql returns a ludicrously large value here.
            // It's so big, it overflows integer, making an exception
            int? length = null;
            if(sysobjectInfo["CHARACTER_MAXIMUM_LENGTH"].CanConvertTo<int?>())
                length = sysobjectInfo["CHARACTER_MAXIMUM_LENGTH"].ConvertTo<int?>();
            // Hack - Sql throws a hissy fit if you try to specify a length beyond 8000, but some field types
            // (ie, ntext, return a massive value from the system tables
            if (length > 8000)
                length = null;

            return new SqlColumnType(
                SqlTypeConversion.ParseDbType(sysobjectInfo["DATA_TYPE"].ConvertTo<string>()),
                sysobjectInfo["IS_NULLABLE"].ConvertTo<bool>(),
                length,
                sysobjectInfo["NUMERIC_PRECISION"].ConvertTo<int?>(),
                sysobjectInfo["NUMERIC_SCALE"].ConvertTo<int?>());
        }

        public bool TableExists(string tablename)
        {
            return _tableExists(tablename);
        }

        protected bool _tableExistsFromDB(string tablename)
        {
            using (var conn = _getConnection(_connectionString))
            {
                var table = TableName(tablename);
                var schema = SchemaName(tablename);
                var sql = _getTableExistsSql(schema, table);
                var paras = new { table = table, schema = schema };
                return conn.ExecuteSingleValueOr<bool>(false, sql, paras);
            }
        }

        public virtual bool SchemaExists(string schema)
        {
            return _schemaExists(schema);
        }

        protected bool _schemaExistsFromDb(string schema)
        {
            var paras = new { schema = schema };
            var sql = _getSchemaExistsSql();
            using (var conn = _getConnection(_connectionString))
            {
                return conn.ExecuteSingleValueOr(false, sql, paras);
            }
        }

        public string TableName(string tablename)
        {
            return NameQualifier.TableName(tablename);
        }

        public virtual string SchemaName(string tablename)
        {
            var res = NameQualifier.SchemaName(tablename);
            if (res == "")
                res = "dbo"; // If we don't specify a schema, use dbo

            return res;
        }
    }
}
