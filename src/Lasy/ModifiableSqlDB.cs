using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    /// <summary>
    /// A SqlDB that automatically creates tables (and schemas) as they are needed.
    /// </summary>
    /// <remarks>In order for the table/schema creation to work, the connection string
    /// needs to be for a dbo user on the database</remarks>
    public class ModifiableSqlDB : IReadWrite, ITransactable, IModifiable
    {
        public ModifiableSqlDB(SqlDB db, SqlModifier modifier)
        {
            SqlModifier = modifier;
            DB = db;
        }

        public SqlModifier SqlModifier { get; protected set; }
        public IDBModifier Modifier { get { return SqlModifier; } }
        public IDBAnalyzer Analyzer { get { return Modifier.Analyzer; } }
        public SqlAnalyzer SqlAnalyzer { get { return Analyzer as SqlAnalyzer; } }
        public SqlDB DB;


        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            // If the table doesn't exist when inserting, create it
            if (!Analyzer.TableExists(tableName))
                Modifier.CreateTable(tableName, row);

            return DB.Insert(tableName, row);
        }

        public ITransaction BeginTransaction()
        {
            return new ModifiableSqlDbTransaction(this);
        }


        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
        {
            return DB.RawRead(tableName, keyFields, fields);
        }


        public void Delete(string tableName, Dictionary<string, object> keyFields)
        {
            DB.Delete(tableName, keyFields);
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            DB.Update(tableName, dataFields, keyFields);
        }
    }
}
