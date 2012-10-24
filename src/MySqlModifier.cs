using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    public class MySqlModifier : SqlModifier
    {
        public MySqlModifier(string connectionString, SqlAnalyzer analyzer, ITypedDBAnalyzer taxonomy = null)
            : base(connectionString, analyzer, taxonomy)
        { }

        protected internal override System.Data.IDbConnection _getConnection(string connectionString)
        {
            return new MySql.Data.MySqlClient.MySqlConnection(connectionString);
        }

        public override string _getCreateSchemaSql(string schema)
        {
            throw new InvalidOperationException("You can't create schemas in MySql");
        }

        public override string _getDropSchemaSql(string schema)
        {
            throw new InvalidOperationException("You can't drop schemas in MySql");
        }

        public override string _getCreateTableSql(string schema, string table, Dictionary<string, SqlColumnType> fieldTypes)
        {
            // Strip off the primary key if it was supplied in fields - we'll make it ourselves
            var datafields = fieldTypes.Except(table + "Id");
            var fieldList = _fieldDefinitions(datafields);

            var sql = String.Format(@"CREATE TABLE `{0}`.`{1}`
            (
                {1}Id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
                {2}
            )",
               schema, table, fieldList);

            return sql;
        }

        public override string _getDropTableSql(string schema, string table)
        {
            return string.Format("drop table `{0}`.`{1}`", schema, table);
        }

        public override string _getDropViewSql(string schema, string view)
        {
            return string.Format("drop view `{0}`.`{1}`", schema, view);
        }
    }
}
