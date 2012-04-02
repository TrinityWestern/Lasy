using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Nvelope;
using Nvelope.IO;
using Nvelope.Reading;

namespace Lasy
{
    public class JSonDB : IReadWrite
    {
        public JSonDB(string filename, IDBAnalyzer analyzer = null)
        {
            Filename = filename;
            Analyzer = analyzer ?? new FakeDBMeta();
        }

        public string Filename { get; protected set; }

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields = null)
        {
            fields = fields ?? new string[] { };

            var rows = _getTable(tableName);
            var filtered = rows.Where(r => keyFields.IsSameAs(r));
            var selected = fields.Any() ?
                filtered.Select(r => r.Only(fields)) :
                filtered;

            return selected;
        }

        public IDBAnalyzer Analyzer { get; protected set; }

        public Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            var allRows = _getTable(tableName);
            var keys = _getKeys(tableName, row, allRows);
            var prepedRow = row.Union(keys);
            var toWrite = allRows.And(prepedRow);
            _writeTable(tableName, toWrite);
            return keys;
        }

        public void Delete(string tableName, Dictionary<string, object> keyFields)
        {
            var allRows = _getTable(tableName);
            var victims = allRows.Where(r => keyFields.IsSameAs(r)).ToList();
            var toWrite = allRows.Except(victims);
            _writeTable(tableName, toWrite);
        }

        public void Update(string tableName, Dictionary<string, object> dataFields, Dictionary<string, object> keyFields)
        {
            var allRows = _getTable(tableName);
            var victims = allRows.Where(r => keyFields.IsSameAs(keyFields)).ToList();
            var updated = victims.Select(r => r.Union(dataFields)).ToList();
            var toWrite = allRows.Except(victims).And(updated);
            _writeTable(tableName, toWrite);
        }

        protected Dictionary<string, object> _getKeys(string tablename, 
            Dictionary<string, object> row, IEnumerable<Dictionary<string, object>> existingRows)
        {
            var autoKey = Analyzer.GetAutoNumberKey(tablename);
            if (autoKey == null)
                return new Dictionary<string, object>();

            var existingKeys = existingRows.Select(r => r[autoKey].ConvertTo<int?>() ?? 0);
            var nextKey = existingKeys.Any() ? existingKeys.Max() + 1 : 1;
            return new Dictionary<string, object>() { { autoKey, nextKey } };
        }

        protected void _writeTable(string table, IEnumerable<Dictionary<string, object>> allRows)
        { 
            // Read everything and replace the file with it
            var fullDb = _getTablenames().MapIndex(_getTable);
            var toWrite = fullDb.Assoc(table, allRows);
            // Write everything back to the file
            var data = toWrite.Select(kv => _tableToS(kv.Key, kv.Value)).Flatten();
            TextFile.Spit(Filename, data.Join(Environment.NewLine));
        }

        protected IEnumerable<string> _tableToS(string tablename, IEnumerable<Dictionary<string, object>> allRows)
        {
            // If there's no rows, don't write anything
            if (!allRows.Any())
                yield break;

            yield return tablename;
            var rows = allRows.Select(r => JsonConvert.SerializeObject(r).Replace(Environment.NewLine, " "));
            foreach(var row in rows)
                yield return row;

            yield return ""; // Include an empty line afterwards to make it pretty
        }

        protected IEnumerable<string> _getTablenames()
        {
            return _getAllLines().Where(_isTablename).Select(s => s.Trim()).ToList();
        }

        protected IEnumerable<Dictionary<string, object>> _getTable(string tablename)
        {
            var allLines = _getAllLines();
            var tableStart = allLines.SkipWhile(l => l.Trim() != tablename)
                .SkipWhile(l => l.Trim() == tablename);
            var tableLines = tableStart.TakeUntil(_isTablename);
            var rows = tableLines.Select(_toDict).ToList();
            return rows;
        }

        protected Dictionary<string, object> _toDict(string line)
        {
            var sd = JsonConvert.DeserializeObject<Dictionary<string, string>>(line);
            var od = sd.SelectVals(TypeConversion.Infervert);
            return od;
        }

        protected IEnumerable<string> _getAllLines()
        {
            var lines = TextFile.Slurp(Filename).Split(Environment.NewLine).Where(s => !s.IsNullOrEmpty());
            return lines;

        }

        /// <summary>
        /// Is the supplied file line a table name?
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        protected bool _isTablename(string line)
        {
            return !line.Trim().StartsWith("{");
        }
    }
}
