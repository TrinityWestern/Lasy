using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using Nvelope;

namespace Lasy
{
    public class MySqlAnalyzer : SqlAnalyzer
    {
        public MySqlAnalyzer(string connectionString, TimeSpan cacheDuration = default(TimeSpan))
            : base(connectionString, new MySqlNameQualifier(connectionString), cacheDuration)
        { }

        protected internal override IDbConnection _getConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        protected string _dbName
        {
            get
            {
                var match = Regex.Match(_connectionString, "[dD]atabase=([^;]+);");
                if (match.Success)
                    return match.Groups[1].Value;
                else
                    throw new ApplicationException("Couldn't extract the database name from the connection string!");
            }
        }

        protected internal override string _getTableExistsSql(string schema, string table)
        {
            // In MySQL, both tables and views show up in information_schema.tables, so we 
            // don't need to look at information_schema.views
            // Note that "schema" in mySql means "database" in MS-SQL. They don't have schemas, so 
            // we just always use "" as the schema. HOWEVER, where it asks for schema here, we
            // should be passing in the database name
            return @"select 1 from information_schema.tables where table_name = @table and table_schema = '" + _dbName + "'";
        }

        protected internal override string _getPrimaryKeySql()
        {
            return @"SELECT k.COLUMN_NAME
                    FROM information_schema.table_constraints t
                    LEFT JOIN information_schema.key_column_usage k
                    USING(constraint_name,table_schema,table_name)
                    WHERE t.constraint_type='PRIMARY KEY'
                        AND t.table_schema = DATABASE()
                        AND t.table_name = @table";
        }

        protected internal override string _getAutonumberKeySql()
        {
            return @"select column_name from information_schema.columns 
                where table_name = @table and table_schema = Database()
                and extra like '%auto_increment%'";
        }

        protected internal override string _getFieldTypeSql()
        {
            return @"select * from information_schema.columns 
                    where table_name = @table and table_schema = Database()";
        }

        public override bool SchemaExists(string schema)
        {
            return schema.IsNullOrEmpty() || schema == SchemaName("");
        }

        public override string SchemaName(string tablename)
        {
            // MySql doesn't have schemas. The things it calls schemas
            // are really databases
            return "";
        }
    }
}
