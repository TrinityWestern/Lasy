﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lasy
{
    public interface IReadable
    {
        /// <summary>
        /// Read all that match on id from the table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IEnumerable<Dictionary<string, object>> RawRead(string tableName, Dictionary<string, object> id, ITransaction transaction = null);
        /// <summary>
        /// Read just the specified fields from the table, filtering down to just the rows that match on id
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fields"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IEnumerable<Dictionary<string, object>> RawReadCustomFields(string tableName, IEnumerable<string> fields, Dictionary<string, object> id, ITransaction transaction = null);
        /// <summary>
        /// Get the analyzer for the DB
        /// </summary>
        IDBAnalyzer Analyzer { get; }
        /// <summary>
        /// Read all the rows from a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IEnumerable<Dictionary<string, object>> RawReadAll(string tableName, ITransaction transaction = null);
        /// <summary>
        /// Read just the specified fields from the table, but for all rows
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fields"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IEnumerable<Dictionary<string, object>> RawReadAllCustomFields(string tableName, IEnumerable<string> fields, ITransaction transaction = null);
    }
}
