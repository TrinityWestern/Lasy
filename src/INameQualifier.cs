using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface INameQualifier
    {
        string TableName(string rawTablename);
        string SchemaName(string rawTablename);
    }
}
