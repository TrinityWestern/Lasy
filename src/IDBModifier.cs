using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface IDBModifier
    {
        void CreateTable(string tablename, Dictionary<string, object> fields);
    }
}
