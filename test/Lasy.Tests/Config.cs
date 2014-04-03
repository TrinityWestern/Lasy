using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasyTests
{
    public static class Config
    {
        public const string TestDBConnectionString = "Data Source=.\\SQLEXPRESS; Initial Catalog=LasyTests;Integrated Security=SSPI;";
        public const string TestMySqlConnectionString = "Server=localhost;Database=lasytest;Uid=lasy;Pwd=abc123;";
    }
}
