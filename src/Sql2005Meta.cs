using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;
using System.Text.RegularExpressions;

namespace Lasy
{
    public class Sql2005Meta : SqlModifier
    {
        public Sql2005Meta(string connectionString, TimeSpan cacheDuration = default(TimeSpan))
            : base(connectionString, cacheDuration)
        { }

        protected override string _getPrimaryKeySql()
        {
            return @"select isc.Column_name
                    from 
                    sys.columns c inner join sys.tables t on c.object_id = t.object_id 
                    inner join information_schema.columns isc 
                    on schema_id(isc.TABLE_SCHEMA) = t.schema_id and isc.TABLE_NAME = t.name and isc.COLUMN_NAME = c.name 
                    left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu
                    on cu.TABLE_SCHEMA = isc.TABLE_SCHEMA and cu.TABLE_NAME = isc.TABLE_NAME and cu.COLUMN_NAME = isc.COLUMN_NAME
                    left join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    on cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    where isc.TABLE_NAME = @table and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    order by ORDINAL_POSITION";
        }

        protected override string _getAutonumberKeySql()
        {
            return @"select isc.Column_name
                    from 
                    sys.columns c inner join sys.tables t on c.object_id = t.object_id 
                    inner join information_schema.columns isc 
                    on schema_id(isc.TABLE_SCHEMA) = t.schema_id and isc.TABLE_NAME = t.name and isc.COLUMN_NAME = c.name 
                    left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu
                    on cu.TABLE_SCHEMA = isc.TABLE_SCHEMA and cu.TABLE_NAME = isc.TABLE_NAME and cu.COLUMN_NAME = isc.COLUMN_NAME
                    left join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    on cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    where isc.TABLE_NAME = @table and is_identity = 1
                    order by ORDINAL_POSITION";
        }

        protected override string _getFieldsSql()
        {
            return @"select 
                isc.COLUMN_NAME as [Name]
                from 
                sys.columns c inner join sys.tables t on c.object_id = t.object_id 
                inner join information_schema.columns isc 
                on schema_id(isc.TABLE_SCHEMA) = t.schema_id and isc.TABLE_NAME = t.name and isc.COLUMN_NAME = c.name 
                LEFT OUTER JOIN sys.default_constraints def 
                ON def.parent_object_id = c.object_id AND def.parent_column_id = c.column_id
                left join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE cu
                on cu.TABLE_SCHEMA = isc.TABLE_SCHEMA and cu.TABLE_NAME = isc.TABLE_NAME and cu.COLUMN_NAME = isc.COLUMN_NAME
                left join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                on cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME and tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                where isc.TABLE_NAME = @table
                order by ORDINAL_POSITION";
        }

        protected override string _getTableExistsSql(string schema, string table)
        {
            return "select 1 from sys.tables where name = @table";
        }

        protected override string _getSchemaExistsSql()
        {
            return "select 1 from sys.schemas where name = @schema";
        }

        protected override string _getCreateSchemaSql(string schema)
        {
            return string.Format("CREATE SCHEMA [{0}] AUTHORIZATION [dbo]", schema);
        }
    }
}
