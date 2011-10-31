using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;

namespace Lasy
{
    public class RealDB : AbstractSqlReadWrite, ITransactable
    {
        public RealDB(string connectionString, IDBAnalyzer analyzer)
            : base(connectionString, analyzer)
        { }
        

        protected override IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                return conn.Execute(sql, values);
            }
        }

        protected override int? sqlInsert(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                return conn.ExecuteSingleValue<int?>(sql, values);
            }
        }

        protected override void sqlUpdate(string sql, Dictionary<string, object> values = null)
        {
            if (values == null)
                values = new Dictionary<string, object>();

            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Execute(sql, values);
            }
        }

        public ITransaction BeginTransaction()
        {
            return new RealDBTransaction(this);
        }




        //protected IDBAnalyzer _dbAnalyzer;
        //public string ConnectionString;
        //protected SqlConnection _conn;

        //public RealDB(string connection, IDBAnalyzer dbAnalyzer)
        //{
        //    _dbAnalyzer = dbAnalyzer;
        //    connectionString = connection;
        //    _conn = new SqlConnection(connection);
        //    _conn.Open();
        //}

        //#region IReadable Members

        //public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keys)
        //{
        //    var whereClause = "";
        //    if (keys.Any())
        //        whereClause = " WHERE " + keys.Select(x => x.Key + " = @" + x.Key).Join(" AND ");
        //    var sql = "SELECT * FROM " + tableName + whereClause;

        //    return sqlRead(sql, keys);
        //}

        ///// <summary>Retrieves only specified columns from a table</summary>
        ///// <param name="fields">IEnumerable(string) of column names that you want from a table</param>
        ///// <param name="keys">The keys and values of a SQL where clause to let you find specific table rows</param>
        ///// <returns>An IEnumerable of a Dictionary with each dictionary representing the column names and values associated with them.</returns>
        //public IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> keys)
        //{
        //    var whereClause = "";
        //    if (keys.Any())
        //        whereClause = " WHERE " + keys.Select(x => x.Key + " = @" + x.Key).Join(" AND ");
        //    var sql = "SELECT " + fields.Join(", ") + " FROM " + tableName + whereClause;

        //    return sqlRead(sql, keys);
        //}

        //#endregion

        //#region Helpers

        //private IEnumerable<Dictionary<string, object>> sqlRead(string sql, Dictionary<string, object> values = null)
        //{
        //    if (values == null)
        //        values = new Dictionary<string, object>();


        //    using (var conn = new SqlConnection(connectionString))
        //    {
        //        return conn.Execute(sql, values);
        //    }

        //    ////If a transaction is passed in we will use the global connection
        //    //else
        //    //{
        //    //    var command = new SqlCommand(sql, _conn, (transaction as RealDBTransaction).UnderlyingTransaction);

        //    //    return command.Execute(sql, values);
        //    //}
        //}

        //private int? sqlInsert(string sql, Dictionary<string, object> values = null)
        //{
        //    if (values == null)
        //        values = new Dictionary<string, object>();

        //    using (var conn = new SqlConnection(connectionString))
        //    {
        //        return conn.ExecuteSingleValue<int?>(sql, values);
        //    }

        //    ////If a transaction is passed in we will use the global connection
        //    //else
        //    //{
        //    //    var command = new SqlCommand(sql, _conn, (transaction as RealDBTransaction).UnderlyingTransaction);

        //    //    return command.ExecuteSingleValue<int?>(sql, values);
        //    //}
        //}

        //private void sqlUpdate(string sql, Dictionary<string, object> values = null)
        //{
        //    if (values == null)
        //        values = new Dictionary<string, object>();
            
        //    using (var conn = new SqlConnection(connectionString))
        //    {
        //        conn.Execute(sql, values);
        //    }

        //    ////If a transaction is passed in we will use the global connection
        //    //else
        //    //{
        //    //    var command = new SqlCommand(sql, _conn, (transaction as RealDBTransaction).UnderlyingTransaction);

        //    //    command.Execute(sql, values);
        //    //}
        //}

        //public IDBAnalyzer Analyzer
        //{
        //    get { return _dbAnalyzer; }
        //}
        //#endregion

        //#region IWriteable Members

        ///// <summary>
        ///// Does an insert and returns the keys of the row just inserted
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <param name="dataFields"></param>
        ///// <param name="transaction"></param>
        ///// <returns></returns>
        //public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> dataFields)
        //{
        //    //Retrieve the AutoNumbered key name if there is one
        //    var autoNumberKeyName = Analyzer.GetAutoNumberKey(tableName);

        //    //Retrieve all of the primary keys to be sure they are all set, except we don't want the autonumbered key since the DB will set it
        //    var requiredKeys = Analyzer.GetPrimaryKeys(tableName).Except(autoNumberKeyName);

        //    //Check that all of the required keys are actually set
        //    var invalid = requiredKeys.Where(key => dataFields[key] == null);
        //    if (invalid.Any())
        //        throw new KeyNotSetException(tableName, invalid);

        //    // Keep in mind that GetFields might return an empty list, which means that it doesn't know
        //    var dbFields = Analyzer.GetFields(tableName).Or(dataFields.Keys);
        //    // Take out the autonumber keys, and take out any supplied data fields
        //    // that aren't actually fields in the DB
        //    var fieldNames = dataFields.Keys.Except(autoNumberKeyName)
        //        .Intersect(dbFields);

        //    var sql = "INSERT INTO " + tableName + " (" + fieldNames.Join(", ") + ") VALUES (" + fieldNames.Select(x => "@" + x).Join(", ") + ")\n";
        //    sql += "SELECT SCOPE_IDENTITY()";

        //    var autoKey =  sqlInsert(sql, dataFields);
            
        //    if (autoNumberKeyName != null && autoKey == null)
        //        throw new ThisSadlyHappenedException("The SQL ran beautifully, but you were expecting an autogenerated number and you did not get it");

        //    if (autoNumberKeyName != null)
        //    {
        //        var autoKeyDict = new Dictionary<string, object>() { { autoNumberKeyName, autoKey } };
        //        dataFields = dataFields.Union(autoKeyDict);
        //    }

        //    return dataFields.WhereKeys(key => Analyzer.GetPrimaryKeys(tableName).Contains(key));
        //}

        //public void Delete(string tableName, Dictionary<string, object> keyFields)
        //{
        //    var sql = "DELETE FROM " + tableName + " WHERE " + keyFields.Select(x => x.Key + " = @" + x.Key).Join(" AND ");

        //    sqlUpdate(sql, keyFields);
        //}

        ///// <param name="dataFields">The fields with data to be udpated in the affected records</param>
        ///// <param name="keyFields">The identifying fields for which records to update</param>
        //public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        //{
        //    var autoKey = _dbAnalyzer.GetAutoNumberKey(tableName);
            
        //    var dataFieldNames = dataFields.Keys.Where(key => key != autoKey);
        //    var dbFields = _dbAnalyzer.GetFields(tableName);
        //    // Don't try to set fields that don't exist in the database
        //    if (dbFields.Any()) // If we don't get anything back, that means we don't know what the DB fields are
        //        dataFieldNames = dataFieldNames.Intersect(dbFields);

        //    var sql = "UPDATE " + tableName + " SET " + dataFieldNames.Select(x => x + " = @data" + x).Join(", ") + "\n";
        //    sql += "WHERE " + keyFields.Select(x => x.Key + " = @key" + x.Key).Join(" AND ");

        //    var data = dataFields.SelectKeys(key => "data" + key);
        //    var keys = keyFields.SelectKeys(key => "key" + key);

        //    sqlUpdate(sql, data.Union(keys));
        //}

        //[Obsolete("Why is this here - we already have Update")]
        ///// <summary>Like Update, but less magic. Does not require an autokey prime number.</summary>
        ///// <param name="dataFields">The fields with data to be udpated in the affected records</param>
        ///// <param name="keyFields">The identifying fields for which records to update</param>
        //public void RealUpdate(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields, ITransaction transaction = null)
        //{
        //    var sql = "UPDATE " + tableName + " SET " + dataFields.Keys.Select(x => x + " = @data" + x).Join(", ") + "\n";
        //    sql += "WHERE " + keyFields.Select(x => x.Key + " = @key" + x.Key).Join(" AND ");

        //    var data = dataFields.SelectKeys(key => "data" + key);
        //    var keys = keyFields.SelectKeys(key => "key" + key);

        //    sqlUpdate(sql, data.Union(keys));
        //}

        ///// <summary>
        ///// Get a new SqlTransaction wrapped in an ITransaction
        ///// </summary>
        //public ITransaction BeginTransaction()
        //{
        //    var transaction = _conn.BeginTransaction();

        //    return new RealDBTransaction(transaction);
        //}
        //#endregion

        //#region IDisposable Members

        //public void Dispose()
        //{
        //    _conn.Dispose();
        //}

        //#endregion




    }
}
