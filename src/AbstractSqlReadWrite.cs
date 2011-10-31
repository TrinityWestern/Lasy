using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;

namespace Lasy
{
    public abstract class AbstractSqlReadWrite : IReadWrite
    {
        public AbstractSqlReadWrite(string connectionString, IDBAnalyzer analyzer)
        {
            Analyzer = analyzer;
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; protected set; }
        
        protected abstract IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values);
        protected abstract int? sqlInsert(string sql, Dictionary<string, object> values);
        protected abstract void sqlUpdate(string sql, Dictionary<string, object> values);
      
        public virtual string MakeWhereClause(Dictionary<string, object> keyFields, string paramPrefix = "")
        {
            keyFields = keyFields ?? new Dictionary<string, object>();

            // TODO: Figure out how to pass null values as parameters
            // instead of hardcoding them in here
            var nullFields = keyFields.WhereValues(v => v == DBNull.Value || v == null).Keys;
            var nullFieldParts = nullFields.Select(x => x + " is null");

            var nonNullFields = keyFields.Except(nullFields).Keys;
            var nonNullFieldParts = nonNullFields.Select(x => x + " = @" + paramPrefix + x);

            var whereClause = "";
            if (keyFields.Any())
                whereClause = " WHERE " + nullFieldParts.And(nonNullFieldParts).Join(" AND ");

            return whereClause;
        }

        public virtual string MakeReadSql(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
        {   
            fields = fields ?? new string[]{};

            var fieldClause = "*";
            if (fields.Any())
                fieldClause = fields.Join(", ");

            var whereClause = MakeWhereClause(keyFields);

            var sql = "SELECT " + fieldClause + " FROM " + tableName + whereClause;

            return sql;
        }

        public virtual string MakeInsertSql(string tableName, Dictionary<string, object> row)
        {
            //Retrieve the AutoNumbered key name if there is one
            var autoNumberKeyName = Analyzer.GetAutoNumberKey(tableName);

            // Keep in mind that GetFields might return an empty list, which means that it doesn't know
            var dbFields = Analyzer.GetFields(tableName).Or(row.Keys);
            // Take out the autonumber keys, and take out any supplied data fields
            // that aren't actually fields in the DB
            var fieldNames = row.Keys.Except(autoNumberKeyName)
                .Intersect(dbFields);

            var sql = "INSERT INTO " + tableName + " (" + fieldNames.Join(", ") + ") VALUES (" + fieldNames.Select(x => "@" + x).Join(", ") + ")\n";
            sql += "SELECT SCOPE_IDENTITY()";

            return sql;
        }

        public virtual string MakeUpdateSql(string tableName, Dictionary<string,object> dataFields, Dictionary<string,object> keyFields)
        {
            var autoKey = Analyzer.GetAutoNumberKey(tableName);

            var setFields = dataFields.Keys.Except(autoKey);
            var dbFields = Analyzer.GetFields(tableName);
            // Don't try to set fields that don't exist in the database
            if (dbFields.Any()) // If we don't get anything back, that means we don't know what the DB fields are
                setFields = setFields.Intersect(dbFields);

            var whereClause = MakeWhereClause(keyFields, "key");

            var sql = "UPDATE " + tableName + " SET " + setFields.Select(x => x + " = @data" + x).Join(", ") + "\n" + whereClause;
            return sql;
        }

        public virtual string MakeDeleteSql(string tableName, Dictionary<string, object> keyFields)
        {
            var whereClause = MakeWhereClause(keyFields);
            return "DELETE FROM " + tableName + whereClause;
        }

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
        {
            var sql = MakeReadSql(tableName, keyFields, fields);
            return sqlRead(sql, keyFields);
        }

        public IDBAnalyzer Analyzer
        {
            get;
            protected set;
        }

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            // Make sure all the required keys are supplied
            this.AssertInsertKeys(tableName, row);

            var sql = MakeInsertSql(tableName, row);
            var autoKey = sqlInsert(sql, row);

            // If there's an autonumber, make sure we add it to the result
            var autoNumberKeyName = Analyzer.GetAutoNumberKey(tableName);
            if (autoNumberKeyName != null && autoKey == null)
                throw new ThisSadlyHappenedException("The SQL ran beautifully, but you were expecting an autogenerated number and you did not get it");
            if(autoNumberKeyName != null)
                row = row.Assoc(autoNumberKeyName, autoKey.Value);

            // Return the set of primary keys from the insert operation
            var primaryKeys = Analyzer.GetPrimaryKeys(tableName);
            return row.Only(primaryKeys);
        }

        public void Delete(string tableName, Dictionary<string, object> keyFields)
        {
            sqlUpdate(MakeDeleteSql(tableName, keyFields), keyFields);
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            var sql = MakeUpdateSql(tableName, dataFields, keyFields);

            var data = dataFields.SelectKeys(key => "data" + key);
            var keys = keyFields.SelectKeys(key => "key" + key);

            sqlUpdate(sql, data.Union(keys));
        }
    }
}
