using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Reflection;

namespace Lasy
{
    public interface IDBModifier
    {
        void CreateTable(string tablename, Dictionary<string, object> fields);
    }

    public static class IDBModifierExtensions
    {
        public static void CreateTable(this IDBModifier db, string tablename, object paramObject)
        {
            db.CreateTable(tablename, paramObject._AsDictionary());
        }
    }
}
