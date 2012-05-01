using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Lasy;
using Nvelope.IO;
using Nvelope;
using System.IO;
using Nvelope.Reading;

namespace LasyTests
{   
    public class JsonDBTests
    {
        protected object _foobar
        {
            get
            {
                return new { A = 1, B = "str" };
            }
        }

        protected string _file = "test.json";

        [TestCase("", Result = "foobar\r\n{\"A\":1,\"B\":\"str\",\"foobarId\":1}")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\",\"foobarId\":1}", Result=
            "foobar\r\n{\"A\":1,\"B\":\"str\",\"foobarId\":1}\r\n{\"A\":1,\"B\":\"str\",\"foobarId\":2}")]
        public string Inserts(string initialContents)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JsonDB(_file);
            subject.Insert("foobar", _foobar);
            return TextFile.Slurp(_file).TrimEnd();
        }

        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}", "([A,1])", Result="")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}\r\n{\"A\":2,\"B\":\"QQQ\"}", "([A,1])",
            Result = "foobar\r\n{\"A\":2,\"B\":\"QQQ\"}")]
        public string Deletes(string initialContents, string keys)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JsonDB(_file);

            var keyDict = Read.Dict(keys).SelectVals(TypeConversion.Infervert);

            subject.Delete("foobar", keyDict);
            return TextFile.Slurp(_file).TrimEnd();
        }

        [TestCase("", "([B,QQQ])", "([A,1])", Result = "")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}", "([B,QQQ])", "([A,1])",
            Result = "foobar\r\n{\"A\":1,\"B\":\"QQQ\"}")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}", "([B,QQQ])", "([A,2])", 
            Result = "foobar\r\n{\"A\":1,\"B\":\"QQQ\"}")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}\r\n{\"A\":2,\"B\":\"str\"}", "([B,QQQ])", "([A,1])",
            Result = "foobar\r\n{\"A\":1,\"B\":\"QQQ\"}\r\n{\"A\":2,\"B\":\"QQQ\"}")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}\r\n{\"A\":2,\"B\":\"str\"}", "([B,QQQ])", "([B,str])", 
            Result = "foobar\r\n{\"A\":1,\"B\":\"QQQ\"}\r\n{\"A\":2,\"B\":\"QQQ\"}")]
        public string Updates(string initialContents, string data, string keys)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JsonDB(_file);

            var keyDict = Read.Dict(keys).SelectVals(TypeConversion.Infervert);
            var dataDict = Read.Dict(data).SelectVals(TypeConversion.Infervert);

            subject.Update("foobar", dataDict, keyDict);
            return TextFile.Slurp(_file).TrimEnd();
        }

        [TestCase("", "()", "", Result="()")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}", "([A,1])", "", Result="(([A,1],[B,str]))")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}", "([A,2])", "", Result = "()")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}", "([A,1])", "A", Result = "(([A,1]))")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}\r\n{\"A\":2,\"B\":\"QQQ\"}", "([A,1])", "", Result = "(([A,1],[B,str]))")]
        public string Reads(string initialContents, string keys, string fieldsToUse)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JsonDB(_file);

            var keyDict = Read.Dict(keys).SelectVals(TypeConversion.Infervert);
            var fields = Read.List(fieldsToUse);

            var res = subject.Read("foobar", keyDict, fields);
            return res.Print();
        }

        [TestCase("table1\r\n{\"A\":1}\r\n{\"A\":2}\r\ntable2\r\n{\"B\":42}", "notTable", Result = "()")]
        [TestCase("table1\r\n{\"A\":1}\r\n{\"A\":2}\r\ntable2\r\n{\"B\":42}", "table1", Result="(([A,1]),([A,2]))")]
        [TestCase("table1\r\n{\"A\":1}\r\n{\"A\":2}\r\ntable2\r\n{\"B\":42}", "table2", Result = "(([B,42]))")]
        public string Reads_MultipleTables(string initialContents, string table)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JsonDB(_file);

            var res = subject.ReadAll(table);
            return res.Print();
        }

        [TestCase("table1\r\n\r\n{\"A\":1}\r\n\r\n{\"A\":2}\r\ntable2\r\n\r\n{\"B\":42}", "notTable", Result = "()")]
        [TestCase("table1\r\n\r\n{\"A\":1}\r\n\r\n{\"A\":2}\r\ntable2\r\n\r\n{\"B\":42}", "table1", Result = "(([A,1]),([A,2]))")]
        [TestCase("table1\r\n\r\n{\"A\":1}\r\n\r\n{\"A\":2}\r\ntable2\r\n\r\n{\"B\":42}", "table2", Result = "(([B,42]))")]
        public string Reads_WithWhitespace(string initialContents, string table)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JsonDB(_file);

            var res = subject.ReadAll(table);
            return res.Print();
        }

        [TestCase("table1\r\n\r\n{\"A\":1,\"B\":3}\r\n\r\n{\"A\":2,\"B\":3}", "([X,2])", "([A,1])",
            Result = "(([A,2],[B,3]),([A,1],[B,3],[X,2]))")]
        public string EnsureOnlyUpdatesOneRow(string initialContents, string data, string keys)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JsonDB(_file);

            var ddata = Read.Dict<string,object>(data);
            var dkeys = Read.Dict<string,object>(keys);

            subject.Ensure("table1", ddata, dkeys);
            return subject.ReadAll("table1").Print();
        }
    }
}
