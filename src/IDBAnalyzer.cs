using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface IDBAnalyzer
    {
        ICollection<string> GetPrimaryKeys(string tableName);
        string GetAutoNumberKey(string tableName);
        /// <summary>
        /// Note: may return an empty list, which means it doesn't know
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        ICollection<string> GetFields(string tableName);
        bool TableExists(string tableName);
    }

    public interface ITypedDBAnalyzer : IDBAnalyzer
    {
        /// <summary>
        /// Gets the types of the fields for the given table
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        Dictionary<string, SqlColumnType> GetFieldTypes(string tablename, Dictionary<string, object> example);
    }
}
