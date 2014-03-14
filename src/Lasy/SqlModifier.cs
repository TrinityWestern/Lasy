using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;
using System.Reactive.Linq;
using System.Reactive;
using System.Data;

namespace Lasy
{
    public class SqlModifier : IDBModifier
    {
        public SqlModifier(string connectionString, SqlAnalyzer analyzer, ITypedDBAnalyzer taxonomy = null)
        {
            SqlAnalyzer = analyzer;
            _connectionString = connectionString;
            Taxonomy = taxonomy;
        }

        protected string _connectionString;
        public ITypedDBAnalyzer Taxonomy { get; set; }
        public SqlAnalyzer SqlAnalyzer { get; protected set; }
        public IDBAnalyzer Analyzer { get { return SqlAnalyzer; } }

        protected internal virtual IDbConnection _getConnection(string connectionString)
        {
            return new System.Data.SqlClient.SqlConnection(connectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="table"></param>
        /// <param name="fieldTypes">A mapping of the fieldname to .NET type of the fields</param>
        /// <returns></returns>
        public virtual string _getCreateTableSql(string schema, string table, Dictionary<string, SqlColumnType> fieldTypes)
        {
            // Strip off the primary key if it was supplied in fields - we'll make it ourselves
            var datafields = fieldTypes.Except(table + "Id");
            var fieldList = _fieldDefinitions(datafields);

            var sql = String.Format(@"CREATE TABLE [{0}].[{1}]
            (
                {1}Id int NOT NULL IDENTITY (1,1) PRIMARY KEY,
                {2}
            ) ON [PRIMARY]",
               schema, table, fieldList);

            return sql;
        }

        public virtual string _getCreateSchemaSql(string schema)
        {
            return string.Format("CREATE SCHEMA [{0}] AUTHORIZATION [dbo]", schema);
        }

        public virtual string _getDropSchemaSql(string schema)
        {
            return string.Format("drop schema [{0}]", schema);
        }

        public virtual string _getDropTableSql(string schema, string table)
        {
            return string.Format("drop table [{0}].[{1}]", schema, table);
        }

        public virtual string _getDropViewSql(string schema, string view)
        {
            return string.Format("drop view [{0}].[{1}]", schema, view);
        }

        public void CreateTable(string tablename, Dictionary<string, SqlColumnType> fieldTypes)
        {
            var table = SqlAnalyzer.TableName(tablename);
            var schema = SqlAnalyzer.SchemaName(tablename);
            var sql = _getCreateTableSql(schema, table, fieldTypes);
            var paras = new { table = table, schema = schema };

            if (!SqlAnalyzer.SchemaExists(schema))
                CreateSchema(schema);

            using (var conn = _getConnection(_connectionString))
                conn.Execute(sql, paras);

            SqlAnalyzer.InvalidateTableCache(tablename);
        }

        public void DropTable(string tablename)
        {
            var table = SqlAnalyzer.TableName(tablename);
            var schema = SqlAnalyzer.SchemaName(tablename);

            using (var conn = _getConnection(_connectionString))
                conn.Execute(_getDropTableSql(schema, table));

            SqlAnalyzer.InvalidateTableCache(tablename);
        }

        public void DropView(string viewname)
        {
            var view = SqlAnalyzer.TableName(viewname);
            var schema = SqlAnalyzer.SchemaName(viewname);

            using (var conn = _getConnection(_connectionString))
                conn.Execute(_getDropViewSql(schema, view));

            SqlAnalyzer.InvalidateTableCache(viewname);
        }

        public void DropSchema(string schema)
        {
            using (var conn = _getConnection(_connectionString))
                conn.Execute(_getDropSchemaSql(schema));
            SqlAnalyzer.InvalidateSchemaCache(schema);
        }

        public void CreateSchema(string schema)
        {
            var sql = _getCreateSchemaSql(schema);
            using (var conn = _getConnection(_connectionString))
            {
                conn.Execute(sql);
            }
            SqlAnalyzer.InvalidateSchemaCache(schema);
        }

        protected string _fieldDefinitions(Dictionary<string, SqlColumnType> fieldTypes)
        {
            var res = fieldTypes.Select(kv => kv.Key + " " + kv.Value.ToString()).Join(", ");
            return res;
        }

        public void KillSchema(string schema)
        {
            if (SqlAnalyzer.SchemaExists(schema))
                DropSchema(schema);
        }

        public void EnsureSchema(string schema)
        {
            if (!SqlAnalyzer.SchemaExists(schema))
                CreateSchema(schema);
        }

        public void KillView(string viewname)
        {
            if (SqlAnalyzer.TableExists(viewname))
                DropView(viewname);
        }
    }
}
