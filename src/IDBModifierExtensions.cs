using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Reflection;
using Nvelope;

namespace Lasy
{
    public static class IDBModifierExtensions
    {
        public static void CreateTable(this IDBModifier db, string tablename, object paramObject)
        {
            db.CreateTable(tablename, paramObject._AsDictionary());
        }
    }
}
