using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    public class ModifiableSqlDbTransaction : SqlDBTransaction
    {
        public ModifiableSqlDB ModifiableDb;
        public ModifiableSqlDbTransaction(ModifiableSqlDB db)
            : base(db)
        {
            ModifiableDb = db;
        }

        public override Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            if (!ModifiableDb.Analyzer.TableExists(tableName))
                ModifiableDb.Modifier.CreateTable(tableName, row);

            return base.Insert(tableName, row);
        }
    }
}
