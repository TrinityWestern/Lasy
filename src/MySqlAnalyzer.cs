using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace Lasy
{
    public class MySqlAnalyzer : SqlAnalyzer
    {
        public MySqlAnalyzer(string connectionString, TimeSpan cacheDuration = default(TimeSpan))
            : base(connectionString, cacheDuration)
        { }

        protected internal override IDbConnection _getConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public override string SchemaName(string tablename)
        {
            // Extract the database name from the connection string,
            // and always use that as the schema name, since it seems
            // like mySql calls SqlServer "databases" "schemas", and 
            // doesn't have the concept of SqlServer schemas
            var match = Regex.Match(_connectionString, "Database=([^;]+);");
            if(!match.Success)
                throw new Exception("Can't figure out the mySql schema name from the connection string. " + 
                    "Expected to find Database=XXXX; in this connection string, but didn't: " + _connectionString);

            return match.Groups[1].Value;
        }

        protected internal override string _getTableExistsSql(string schema, string table)
        {
            return @"select 1 from information_schema.tables where table_name = @table and table_schema = @schema union all 
                    select 1 from information_schema.views where table_name = @table and table_schema = @schema";
        }
    }
}
