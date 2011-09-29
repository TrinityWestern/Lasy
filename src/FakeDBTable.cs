using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;

namespace Lasy
{
    public class FakeDBTable : List<Dictionary<string, object>>
    {
        public IEnumerable<Dictionary<string, object>> FindByFieldValues(Dictionary<string, object> values)
        {
            return this.Where(x => filter(x, values));
        }

        private bool filter(Dictionary<string, object> row, Dictionary<string, object> values)
        {
            return values.Keys.All(field => row.ContainsKey(field) && row[field].Eq(values[field]));
        }

        public int NextAutoKey = 1;
    }
}
