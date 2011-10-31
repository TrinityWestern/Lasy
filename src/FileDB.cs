using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.IO;
using System.Text.RegularExpressions;
using System.IO;

namespace Lasy
{
    /// <summary>
    /// An implementation of IReadable that reads the data from the filesystem. Very useful for writing tests
    /// against a particular data case.
    /// </summary>
    /// <remarks>FileDB understands data files that use SQL Management Studio's .rpt format. So in order to
    /// make a new FileDB, all you have to do is execute a SQL query in Management Studio and output the results
    /// to file.
    /// 
    /// FileDB assumes that each file is a table, with the filename being the name of the table. So if you
    /// request data from the table 'Foosums', FileDB will try to read from Foosums.rpt Thus, you can
    /// have as many tables in your FileDB database as you want, to simulate complicated data cases</remarks>
    public class FileDB : IReadable
    {
        public FileDB(string directory, string fileExtension = ".rpt")
        {
            Directory = directory;
            FileExtension = fileExtension;
        }

        public string Directory { get; private set; }

        public string FileExtension { get; private set; }

        /// <summary>
        /// Reads in data from SQL Management Studio's .rpt format
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private IEnumerable<Dictionary<string, string>> readRptFile(string filePath)
        {
            // We expect this format:
            // COLNAME  COLNAME2
            // -------- --------
            // Row1Val  Row1Val2
            // Row2Val  Row2Val2

            var lines = TextFile.Slurp(filePath).Split(Environment.NewLine);
            // If there's no data lines, return nothing
            if (!lines.AtLeast(3))
                yield break;

            // The widths of the columns should be equal to the number of dashes
            // in each column in the second line
            var columnWidths = lines.Second().Tokenize().Select(s => s.Length + 1);
            // Column names come from the first line, and should be left-justified over their columns
            var columnNames = lines.First().Partition(columnWidths)
                // Convert back into strings from list of chars
                .Select(charlist => charlist.Read().TrimEnd()).ToList();

            var dataLines = lines.Skip(2).TakeWhile(l => !l.IsNullOrEmpty());
            
            foreach (var dl in dataLines)
            {
                // Chop up the data line on the column boundaries
                var values = dl.Partition(columnWidths).Select(s => s.Read().TrimEnd());
                // Match the column names with the values to get the result
                yield return columnNames.ZipToDict(values);
            }
        }

        private Dictionary<string, IEnumerable<Dictionary<string, string>>> _tableCache = 
            new Dictionary<string, IEnumerable<Dictionary<string, string>>>();

        private Dictionary<string, DateTime> _tableCacheTimes = new Dictionary<string, DateTime>();

        private IEnumerable<Dictionary<string, string>> getTable(string tableName)
        {
            tableName = tableName.ToLower();
            var filename = tableName + FileExtension;
            var path = Folder.ComposePath(Directory, filename);
            var modificationTime = File.GetLastAccessTime(path);

            if (!_tableCache.ContainsKey(tableName) || _tableCacheTimes[tableName] != modificationTime)
            {
                _tableCache.SetVal(tableName, readRptFile(path).ToList());
                _tableCacheTimes.SetVal(tableName, modificationTime);
            }

            return _tableCache[tableName];
        }

        private Dictionary<string, object> convertRow(Dictionary<string, string> row)
        {
            return row.SelectVals(val => Infervert(val));
        }

        /// <summary>
        /// Used by Infervert, this is how we guess what type data is supposed to be
        /// </summary>
        private static Dictionary<Regex,Type> _infervertConversions = new Dictionary<Regex,Type>()
        {
            {new Regex("^[0-9]+$", RegexOptions.Compiled), typeof(int)},
            {new Regex("^[0-9]+\\.[0-9]+$", RegexOptions.Compiled), typeof(decimal)},
            {new Regex("^[tT]rue|[Ff]alse$", RegexOptions.Compiled), typeof(bool)},
            {new Regex("^[0-9]{4}\\-[0-9]{2}\\-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}\\.[0-9]{3}$", 
                RegexOptions.Compiled), typeof(DateTime)},
            {new Regex(".*", RegexOptions.Compiled), typeof(string)}
        };

        /// <summary>
        /// Based on a string representation, try to convert the value to the "appropriate" type
        /// Largely guesswork
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Infervert(string value)
        {
            if (value == "NULL")
                return null;

            var outputType = _infervertConversions.First(kv => kv.Key.IsMatch(value)).Value;
            return value.ConvertTo(outputType);
        }

        public IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> keyFields, IEnumerable<string> fields)
        {
            var table = getTable(tableName);
            var rows = table.Select(d => convertRow(d)).ToList();
            var results = rows.Where(row => keyFields.IsSameAs(row));
            return results;
        }

        public IDBAnalyzer Analyzer
        {
            get { throw new NotImplementedException(); }
        }
    }
}
