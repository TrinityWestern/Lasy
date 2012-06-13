﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using System.Data;
using System.Reactive.Linq;
using Nvelope.Reactive;
using System.Data.Common;

namespace Lasy
{
    public class SqlAnalyzer : ITypedDBAnalyzer
    {
        public SqlAnalyzer(string connectionString, TimeSpan cacheDuration = default(TimeSpan))
        {
            if(cacheDuration == default(TimeSpan))
                cacheDuration = _defaultCacheDuration();

            _connectionString = connectionString;

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
                    where isc.TABLE_NAME = @table and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
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
                    where isc.TABLE_NAME = @table and is_identity = 1
                    order by ORDINAL_POSITION";
        }

        protected internal virtual string _getTableExistsSql(string schema, string table)
        {
            return "select 1 from sys.tables where name = @table union all select 1 from sys.views where name = @table";
        }

        protected internal virtual string _getFieldTypeSql()
        {
            return @"SELECT     
                    isc.*
                FROM 
                    sysobjects tbl
                    inner join syscolumns c
                    on tbl.id = c.id
                    inner join information_schema.columns isc
                    on isc.column_name = c.name and isc.table_name = tbl.name
                    left outer join information_schema.key_column_usage k
                    on k.table_name = tbl.name and objectproperty(object_id(constraint_name), 'IsPrimaryKey') = 1
                        and k.column_name = c.name
                WHERE 
                    tbl.xtype in ('U','V')
                    and tbl.name = @table
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
                return conn.ExecuteSingleColumn<string>(_getPrimaryKeySql(), new { table = TableName(tableName) });
            }
        }

        public string GetAutoNumberKey(string tableName)
        {
            return _getAutonumberKey(tableName);
        }

        protected string _getAutonumberKeyFromDB(string tableName)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                var res = conn.ExecuteSingleColumn<string>(_getAutonumberKeySql(), new { table = TableName(tableName) });
                return res.FirstOr(null);
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
                return _convertTypes(conn.Execute(_getFieldTypeSql(), new { table = TableName(tableName) }));
        }

        protected Dictionary<string, SqlColumnType> _convertTypes(ICollection<Dictionary<string, object>> sysobjectsInfos)
        {
            return sysobjectsInfos.ToDictionary(
                row => row["COLUMN_NAME"].ToString(),
                row => _determineType(row));
        }

        protected SqlColumnType _determineType(Dictionary<string, object> sysobjectInfo)
        {
            return new SqlColumnType(
                SqlTypeConversion.ParseDbType(sysobjectInfo["DATA_TYPE"].ConvertTo<string>()),
                sysobjectInfo["IS_NULLABLE"].ConvertTo<bool>(),
                sysobjectInfo["CHARACTER_MAXIMUM_LENGTH"].ConvertTo<int?>(),
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

        public bool SchemaExists(string schema)
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

        public virtual string TableName(string tablename)
        {
            var res = tablename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Last().ChopEnd("]").ChopStart("[");
            return res;
        }

        public virtual string SchemaName(string tablename)
        {
            var parts = tablename.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ChopEnd("]").ChopStart("["));
            if (parts.Count() > 1)
                return parts.First();
            else
                return "";
        }
    }
}
