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
        public AbstractSqlReadWrite(string connectionString, SqlAnalyzer analyzer, bool strictTables = true)
        {
            SqlAnalyzer = analyzer;
            ConnectionString = connectionString;
            StrictTables = strictTables;
        }

        public string ConnectionString { get; protected set; }

        protected abstract IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values);
        protected abstract int? sqlInsert(string sql, Dictionary<string, object> values);
        protected abstract void sqlUpdate(string sql, Dictionary<string, object> values);

        public virtual string QualifiedTable(string tablename)
        {
            var schema = SqlAnalyzer.SchemaName(tablename);
            var table = SqlAnalyzer.TableName(tablename);

            if (schema.IsNullOrEmpty())
                return "[" + tablename + "]";
            else
                return "[" + schema + "].[" + table + "]";
        }

        /// <summary>
        /// Warning: You should greatly prefer using SQL parameters instead of using literals.
        /// Literals are vulnerable to SQL injection attacks
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string SqlLiteral(object o)
        {
            if (o is string || o is DateTime)
                return "'" + o.ToString().Replace("'", "''") + "'";
            else
                return o.ToString();
        }

        /// <summary>
        /// If true, throw an exception when referencing tables that don't exist.
        /// If false, do something intelligent instead - Reads return nothing, updates and 
        /// deletes do nothing, but inserts still throw exceptions
        /// </summary>
        public virtual bool StrictTables { get; set; }
      
        public virtual string MakeWhereClause(Dictionary<string, object> keyFields, string paramPrefix = "", bool useParameters = true)
        {
            keyFields = keyFields ?? new Dictionary<string, object>();

            // TODO: Figure out how to pass null values as parameters
            // instead of hardcoding them in here
            var nullFields = keyFields.WhereValues(v => v == DBNull.Value || v == null).Keys;
            var nullFieldParts = nullFields.Select(x => x + " is null");

            var nonNullFields = keyFields.Except(nullFields).Keys;
            var nonNullFieldParts =
                useParameters ?
                    nonNullFields.Select(x => x + " = @" + paramPrefix + x) :
                    nonNullFields.Select(x => x + " = " + SqlLiteral(keyFields[x]));

            var whereClause = "";
            if (keyFields.Any())
                whereClause = " WHERE " + nullFieldParts.And(nonNullFieldParts).Join(" AND ");

            return whereClause;
        }

        public virtual string MakeReadSql(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null, bool useParameters = true)
        {   
            fields = fields ?? new string[]{};

            var fieldClause = "*";
            if (fields.Any())
                fieldClause = fields.Join(", ");

            var whereClause = MakeWhereClause(keyFields, "", useParameters);

            var sql = "SELECT " + fieldClause + " FROM " + QualifiedTable(tableName) + whereClause;

            return sql;
        }

        public virtual string MakeInsertSql(string tableName, Dictionary<string, object> row, bool useParameters = true)
        {
            //Retrieve the AutoNumbered key name if there is one
            var autoNumberKeyName = Analyzer.GetAutoNumberKey(tableName);

            // Keep in mind that GetFields might return an empty list, which means that it doesn't know
            var dbFields = Analyzer.GetFields(tableName).Or(row.Keys);
            // Take out the autonumber keys, and take out any supplied data fields
            // that aren't actually fields in the DB
            var fieldNames = row.Keys.Except(autoNumberKeyName)
                .Intersect(dbFields);

            var valList = useParameters ?
                fieldNames.Select(x => "@" + x) :
                fieldNames.Select(x => SqlLiteral(row[x]));

            var sql = "INSERT INTO " + QualifiedTable(tableName) + " (" + fieldNames.Join(", ") + ") " + 
                "VALUES (" + valList.Join(", ") + ")\n";
            sql += "SELECT SCOPE_IDENTITY()";

            return sql;
        }

        public virtual string MakeUpdateSql(string tableName, Dictionary<string,object> dataFields, Dictionary<string,object> keyFields, bool useParameters = true)
        {
            var autoKey = Analyzer.GetAutoNumberKey(tableName);

            var setFields = dataFields.Keys.Except(autoKey);
            var dbFields = Analyzer.GetFields(tableName);
            // Don't try to set fields that don't exist in the database
            if (dbFields.Any()) // If we don't get anything back, that means we don't know what the DB fields are
                setFields = setFields.Intersect(dbFields);

            var whereClause = MakeWhereClause(keyFields, "key", useParameters);

            var valFields =
                useParameters ?
                    setFields.Select(x => x + " = @data" + x) :
                    setFields.Select(x => x + " = " + SqlLiteral(dataFields[x]));

            var sql = "UPDATE " + QualifiedTable(tableName) + " SET " + valFields.Join(", ") + "\n" + whereClause;
            return sql;
        }

        public virtual string MakeDeleteSql(string tableName, Dictionary<string, object> keyFields, bool useParameters = true)
        {
            var whereClause = MakeWhereClause(keyFields, "", useParameters);
            return "DELETE FROM " + QualifiedTable(tableName) + whereClause;
        }

        public virtual IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
        {
            // If the table doesn't exist, we probably don't want to run any sql
            if (!Analyzer.TableExists(tableName))
                if (StrictTables)
                    throw new NotATableException(tableName);
                else
                    return new List<Dictionary<string, object>>();

            var sql = MakeReadSql(tableName, keyFields, fields);
            return sqlRead(sql, keyFields);
        }

        public IDBAnalyzer Analyzer { get { return SqlAnalyzer; } }
        public SqlAnalyzer SqlAnalyzer { get; protected set; }

        public virtual Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            if (StrictTables && !Analyzer.TableExists(tableName))
                throw new NotATableException(tableName);

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

        public virtual void Delete(string tableName, Dictionary<string, object> keyFields)
        {
            if (!Analyzer.TableExists(tableName))
                if (StrictTables)
                    throw new NotATableException(tableName);
                else
                    return;

            sqlUpdate(MakeDeleteSql(tableName, keyFields), keyFields);
        }

        public virtual void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            if (!Analyzer.TableExists(tableName))
                if (StrictTables)
                    throw new NotATableException(tableName);
                else
                    return;

            var sql = MakeUpdateSql(tableName, dataFields, keyFields);

            var data = dataFields.SelectKeys(key => "data" + key);
            var keys = keyFields.SelectKeys(key => "key" + key);

            sqlUpdate(sql, data.Union(keys));
        }
    }
}
