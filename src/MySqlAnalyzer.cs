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

        protected internal override string _getTableExistsSql(string schema, string table)
        {
            // In MySQL, both tables and views show up in information_schema.tables, so we 
            // don't need to look at information_schema.views
            return @"select 1 from information_schema.tables where table_name = @table and table_schema = @schema";
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
    }
}
