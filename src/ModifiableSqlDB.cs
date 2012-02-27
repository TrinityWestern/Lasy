﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    /// <summary>
    /// A SqlDB that automatically creates tables (and schemas) as they are needed.
    /// </summary>
    /// <remarks>In order for the table/schema creation to work, the connection string
    /// needs to be for a dbo user on the database</remarks>
    public class ModifiableSqlDB : SqlDB, IModifiable
    {
        public ModifiableSqlDB(string connectionString, SqlModifier modifier)
            : base(connectionString, modifier.SqlAnalyzer, false)
        {
            SqlModifier = modifier;
        }

        /// <summary>
        /// If true, this allows new tables to be created in the DB with no type information. This may lead
        /// to tables being created with unintended types. If false, throw an exception when we try to 
        /// create a table with no type information.
        /// </summary>
        bool AllowInferedTableStructure = false;

        public SqlModifier SqlModifier { get; protected set; }

        public IDBModifier Modifier { get { return SqlModifier; } }

        public override Dictionary<string, object> Insert(string tableName, Dictionary<string, object> row)
        {
            // If the table doesn't exist when inserting, create it
            if (!Analyzer.TableExists(tableName))
                Modifier.CreateTable(tableName, row);

            return base.Insert(tableName, row);
        }

        public override ITransaction BeginTransaction()
        {
            return new ModifiableSqlDbTransaction(this);
        }
    }
}
