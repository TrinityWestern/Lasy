using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Lasy
{
    public class MySqlNameQualifier : SqlNameQualifier
    {
        public MySqlNameQualifier(string connectionString)
        {
            // Extract the database name from the connection string,
            // and always use that as the schema name, since it seems
            // like mySql calls SqlServer "databases" "schemas", and 
            // doesn't have the concept of SqlServer schemas
            var match = Regex.Match(connectionString, "Database=([^;]+);");
            if (!match.Success)
                throw new Exception("Can't figure out the mySql schema name from the connection string. " +
                    "Expected to find Database=XXXX; in this connection string, but didn't: " + connectionString);

            _schema = match.Groups[1].Value;
        }

        private string _schema;

        public override string SchemaName(string tablename)
        {
            return _schema;
        }
    }
}
