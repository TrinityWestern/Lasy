using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Nvelope;
using System.Text.RegularExpressions;

namespace Lasy
{
    public class MySqlDB : SqlDB
    {
        public MySqlDB(string connectionString, SqlAnalyzer analyzer, bool strictTables = true)
            : base(connectionString, analyzer, strictTables)
        { }

        protected internal override System.Data.IDbConnection _getConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public override string QualifiedTable(string tablename)
        {
            var schema = SqlAnalyzer.SchemaName(tablename);
            var table = SqlAnalyzer.TableName(tablename);

            if (schema.IsNullOrEmpty())
                return tablename;
            else
                return schema + "." + table;
        }

        public override string GetInsertedAutonumber()
        {
            return "; SELECT LAST_INSERT_ID();";
        }
    }
}
