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
            var subject = new JSonDB(_file);
            subject.Insert("foobar", _foobar);
            return TextFile.Slurp(_file).TrimEnd();
        }

        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}", "([A,1])", Result="")]
        [TestCase("foobar\r\n{\"A\":1,\"B\":\"str\"}\r\n{\"A\":2,\"B\":\"QQQ\"}", "([A,1])",
            Result = "foobar\r\n{\"A\":2,\"B\":\"QQQ\"}")]
        public string Deletes(string initialContents, string keys)
        {
            TextFile.Spit(_file, initialContents);
            var subject = new JSonDB(_file);

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
            var subject = new JSonDB(_file);

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
            var subject = new JSonDB(_file);

            var keyDict = Read.Dict(keys).SelectVals(TypeConversion.Infervert);
            var fields = Read.List(fieldsToUse);

            var res = subject.Read("foobar", keyDict, fields);
            return res.Print();
        }
    }
}
