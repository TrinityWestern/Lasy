using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope.Reflection;
using Nvelope;

namespace Lasy
{
    public interface IDBModifier : IAnalyzable
    {
        /// <summary>
        /// This analyzer should be able to tell the DBModifier about the structure
        /// of any table that needs to be created
        /// </summary>
        ITypedDBAnalyzer Taxonomy { get; set; }
        void CreateTable(string tablename, Dictionary<string, SqlColumnType> fields);
        void DropTable(string tablename);
    }

    public static class IDBModifierExtensions
    {
        public static void CreateTable(this IDBModifier meta, string tablename, Dictionary<string, object> instance)
        {
            var taxonomyTypes = meta.Taxonomy == null ?
                new Dictionary<string, SqlColumnType>() :
                meta.Taxonomy.GetFieldTypes(tablename, instance);

            taxonomyTypes = taxonomyTypes ?? new Dictionary<string, SqlColumnType>();

            var missingTypes = instance.Except(taxonomyTypes.Keys)
                .SelectVals(v => SqlTypeConversion.GetSqlType(v));

            var fieldTypes = missingTypes.Union(taxonomyTypes);

            meta.CreateTable(tablename, fieldTypes);
        }

        public static void CreateTable(this IDBModifier meta, string tablename, object instance)
        {
            CreateTable(meta, tablename, instance._AsDictionary());
        }

        public static void EnsureTable(this IDBModifier meta, string tablename, Dictionary<string, object> instance)
        {
            if (!meta.Analyzer.TableExists(tablename))
                CreateTable(meta, tablename, instance);
        }

        public static void EnsureTable(this IDBModifier meta, string tablename, object instance)
        {
            EnsureTable(meta, tablename, instance._AsDictionary());
        }

        /// <summary>
        /// Drops the table if it exists, else does nothing
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="tablename"></param>
        public static void KillTable(this IDBModifier meta, string tablename)
        {
            if (meta.Analyzer.TableExists(tablename))
                meta.DropTable(tablename);
        }
    }
}
