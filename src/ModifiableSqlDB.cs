using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    /// <summary>
    /// A SqlDB that automatically creates tables (and schemas) as they are needed.
    /// </summary>
    /// <remarks>In order for the table/schema creation to work, the connection string
    /// needs to be for a dbo user on the database</remarks>
    public class ModifiableSqlDB : SqlDB, IModifiable
    {
        public ModifiableSqlDB(string connectionString, SqlModifier modifier)
            : base(connectionString, modifier.SqlAnalyzer, false)
        {
            SqlModifier = modifier;
        }

        public SqlModifier SqlModifier { get; protected set; }

        public IDBModifier Modifier { get { return SqlModifier; } }

        public override Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            // If the table doesn't exist when inserting, create it
            if (!Analyzer.TableExists(tableName))
                Modifier.CreateTable(tableName, row);

            return base.Insert(tableName, row);
        }
    }
}
