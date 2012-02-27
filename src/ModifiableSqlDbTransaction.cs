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

        /// <summary>
        /// If true, this allows new tables to be created in the DB with no type information. This may lead
        /// to tables being created with unintended types. If false, throw an exception when we try to 
        /// create a table with no type information.
        /// </summary>
        bool AllowInferedTableStructure = false;

        public override Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            if (!ModifiableDb.Analyzer.TableExists(tableName))
                ModifiableDb.Modifier.CreateTable(tableName, row);

            return base.Insert(tableName, row);
        }
    }
}
