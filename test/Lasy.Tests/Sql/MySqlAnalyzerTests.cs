using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Nvelope.Configuration;
using Lasy;
using Nvelope;

namespace LasyTests.Sql
{
    [TestFixture]
    public class MySqlAnalyzerTests
    {
        protected string _connStr
        {
            get
            {
                return Config.ConnectionString("mysqldb");
            }
        }

        [TestCase("house", Result="([idHouse,int NOT NULL],[Number,int NULL],[Street,varchar(50) NULL])")]
        [TestCase("bigint_table", Result="([idbigint_table,int NOT NULL],[number,bigint NULL])")]
        [TestCase("longtext_table", Result="([idlongtext_table,int NOT NULL],[str,nvarchar NULL])")]
        public string FieldTypes(string table)
        {
            var an = new MySqlAnalyzer(_connStr);
            var res = an.GetFieldTypes(table);
            return res.Print();
        }
    }
}
