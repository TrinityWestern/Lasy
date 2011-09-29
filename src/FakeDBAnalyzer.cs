using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nvelope;

namespace Lasy
{
    public class FakeDBAnalyzer : IDBAnalyzer
    {
        public Dictionary<string, ICollection<string>> PrimaryKeys = new Dictionary<string, ICollection<string>>();

        public Dictionary<string, string> AutoNumberKeys = new Dictionary<string, string>();

        /// <summary>
        /// If true, the analyzer will assume that there's a single autonumber PK for every table,
        /// that has the name [tableName]Id. Any additions to PrimaryKeys or AutoNumberKeys will override this
        /// </summary>
        public bool AssumeStandardKeys = true;

        #region IDBAnalyzer Members

        public ICollection<string> GetPrimaryKeys(string tableName)
        {
            if (PrimaryKeys.ContainsKey(tableName))
                return PrimaryKeys[tableName];
            else if (AssumeStandardKeys)
                return (_unschemadTablename(tableName) + "Id").List().ToList();
            else
                throw new NotImplementedException("Dont know what the primary keys for " + tableName + " would be. Either add that table's keys to the PrimaryKeys collection, or set AssumeStandardKeys to true for the default key behavior");

        }

        public string GetAutoNumberKey(string tableName)
        {
            if (AutoNumberKeys.ContainsKey(tableName))
                return AutoNumberKeys[tableName];
            else if (AssumeStandardKeys)
                return _unschemadTablename(tableName) + "Id";
            else
                throw new NotImplementedException("Dont know what the autonumbers for " + tableName + " would be. Either add that table's autonumbers to the AutoNumberKeys collection, or set AssumeStandardKeys to true for the default autonumber behavior");
        }

        private string _unschemadTablename(string tablename)
        {
            // Turn [Schema].[Table] into Table
            return tablename.Split(new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries).Last().ChopStart("[").ChopEnd("]");
        }

        public ICollection<string> GetFields(string tableName)
        {
            // We don't know what the actual structure is, so send back an empty
            // list to indicate that we don't actually know
            return new ReadOnlyCollection<string>(new List<string> { });
        }

        #endregion
    }
}
