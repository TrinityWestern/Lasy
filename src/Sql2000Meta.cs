using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Nvelope;

namespace Lasy
{
    public class Sql2000Meta : SqlModifier
    {
        public Sql2000Meta(string connectionString, TimeSpan cacheDuration = default(TimeSpan))
            : base(connectionString, cacheDuration)
        { }
        

        protected override string _getPrimaryKeySql()
        {
            return @"SELECT     
                isc.COLUMN_NAME as [Name]
            FROM 
                sysobjects tbl
                inner join syscolumns c
                on tbl.id = c.id
                inner join information_schema.columns isc
                on isc.column_name = c.name and isc.table_name = tbl.name
                left outer join information_schema.key_column_usage k
                on k.table_name = tbl.name and objectproperty(object_id(constraint_name), 'IsPrimaryKey') = 1
		            and k.column_name = c.name
            WHERE 
                tbl.xtype = 'U'
                and tbl.name = @table
                AND objectproperty(object_id(constraint_name), 'IsPrimaryKey') = 1
            order by isc.ORDINAL_POSITION";
        }

        protected override string _getAutonumberKeySql()
        {
            return @"SELECT     
                isc.COLUMN_NAME as [Name]
            FROM 
                sysobjects tbl
                inner join syscolumns c
                on tbl.id = c.id
                inner join information_schema.columns isc
                on isc.column_name = c.name and isc.table_name = tbl.name
            WHERE 
                tbl.xtype = 'U'
                and tbl.name = @table
                AND c.status & 0x80 = 0x80
            order by isc.ORDINAL_POSITION";
        }

        protected override string _getFieldsSql()
        {
            return @"SELECT     
                isc.COLUMN_NAME as [Name]
            FROM 
                sysobjects tbl
                inner join syscolumns c
                on tbl.id = c.id
                inner join information_schema.columns isc
                on isc.column_name = c.name and isc.table_name = tbl.name
                left outer join information_schema.key_column_usage k
                on k.table_name = tbl.name and objectproperty(object_id(constraint_name), 'IsPrimaryKey') = 1
		            and k.column_name = c.name
            WHERE 
                tbl.xtype = 'U'
                and tbl.name = @table
            order by isc.ORDINAL_POSITION";
        }

        protected override string _getCreateTableSql(string schema, string table, Dictionary<string, object> fields)
        {
            // TODO: Actually test this - I've just stolen it verbatim from the 2005 implementation
            // I think it will work, but don't have a Sql 2000 db to test against

            // Strip off the primary key if it was supplied in fields - we'll make it ourselves
            var datafields = fields.Except(table + "Id");
            var fieldList = _fieldDefinitions(datafields);

            var sql = String.Format(@"CREATE TABLE @1.@2
            (
                @2Id int NOT NULL IDENTITY (1,1),
                @3
            ) ON [PRIMARY]
            GO
            ALTER TABLE @1.@2 ADD CONSTRAINT
            PK_@2 PRIMARY KEY CLUSTERED
            ( @2Id ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
            GO",
               schema, table, fieldList);

            return sql;
        }

        protected override string _getTableExistsSql(string schema, string table)
        {
            return @"SELECT 1 FROM sysobjects tbl 
                WHERE tbl.xtype = 'U' and tbl.name = @table";
        }

        public override void CreateSchema(string schema)
        {
            // Do nothing - SQL 2000 doesn't support schemas
        }

        public override bool SchemaExists(string schema)
        {
            // Sql 2000 doesn't support schemas, everythings's good
            return true;
        }

        protected override string _getSchemaExistsSql()
        {
            // Do nothing - SQL 2000 doesn't support schemas
            // Overriding SchemaExists in this class should prevent this from being called
            throw new NotImplementedException();
        }

        protected override string _getCreateSchemaSql(string schema)
        {
            // Do nothing - SQL 2000 doesn't support schemas
            // Overriding CreateSchema in this class should prevent this from being called
            throw new NotImplementedException();
        }
    }
}
