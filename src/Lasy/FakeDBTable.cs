using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    public class FakeDBTable : List<Dictionary<string, object>>
    {
        public FakeDBTable()
            : base()
        { }

        public FakeDBTable(FakeDBTable coll)
            : base(coll)
        {
            NextAutoKey = coll.NextAutoKey;
        }

        public FakeDBTable(IEnumerable<Dictionary<string, object>> rows, int nextAutokey)
            : base(rows.ToList()) // Break any lazy evaluation so we don't end up re-evaluating the sequence
        {
            NextAutoKey = nextAutokey;
        }

        public IEnumerable<Dictionary<string, object>> FindByFieldValues(Dictionary<string, object> values)
        {
            return this.Where(x => filter(x, values));
        }

        private bool filter(Dictionary<string, object> row, Dictionary<string, object> values)
        {
            return values.Keys.All(field => row.ContainsKey(field) && row[field].Eq(values[field]));
        }

        public IEnumerable<Dictionary<string, object>> Read(Dictionary<string, object> values = null, IEnumerable<string> fields = null)
        {
            fields = (fields ?? new List<string>()).ToList();
            values = values ?? new Dictionary<string, object>();

            var rows = FindByFieldValues(values.ScrubNulls());
            if (!fields.Any())
                return rows.Select(r => r.Copy()).ToList();
            else
                return rows.Select(row => row.WhereKeys(col => fields.Contains(col))).ToList();
        }

        public int NextAutoKey = 1;
    }
}
