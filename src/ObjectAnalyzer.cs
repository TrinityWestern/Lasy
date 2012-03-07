using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    /// <summary>
    /// An ITypedDBAnalyzer that reads structure from objects
    /// </summary>
    public class ObjectAnalyzer : ITypedDBAnalyzer
    {
        protected class ObjAnalyzerItem
        {
            public string Name;
            public Dictionary<string, SqlColumnType> Types;
        }

        protected List<ObjAnalyzerItem> _items = new List<ObjAnalyzerItem>();

        public void Add(string name, object obj)
        {
            var types = obj._SqlFieldTypes();
            _items.Add(new ObjAnalyzerItem() { Name = name, Types = types });
        }

        public Dictionary<string, SqlColumnType> GetFieldTypes(string tablename, Dictionary<string, object> example)
        {
            if (!TableExists(tablename))
                return null;

            var types = _items.First(i => i.Name == tablename).Types;
            var extras = example.Except(types.Keys).SelectVals(v => SqlTypeConversion.GetSqlType(v));
            var res = extras.Union(types);
            return res;
        }

        public ICollection<string> GetPrimaryKeys(string tableName)
        {
            return new string[] { };
        }

        public string GetAutoNumberKey(string tableName)
        {
            return null;
        }

        public ICollection<string> GetFields(string tableName)
        {
            if (TableExists(tableName))
                return _items.First(i => i.Name == tableName).Types.Keys;
            else
                return new string[] { };
        }

        public bool TableExists(string tableName)
        {
            return _items.Any(i => i.Name == tableName);
        }
    }
}
