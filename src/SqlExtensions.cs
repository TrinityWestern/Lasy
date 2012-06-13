using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Nvelope;
using Nvelope.Data;
using Nvelope.Reflection;

namespace Lasy
{
    public static class SqlExtensions
    {
        /// <summary>
        /// Automatically construct a SQLParameter from the name and value and add it to the collection
        /// </summary>
        /// <remarks>You can have lazy-loaded values by passing a Func{object} in value - this will
        /// call Realize on the value, which will run the function and return the value</remarks>
        /// <param name="coll"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void AddParameter(this IDbCommand comm, string name, object value)
        {
            var realizedValue = value.Realize();
            var sqlType = SqlTypeConversion.GetSqlType(realizedValue);
            var para = comm.CreateParameter();
            para.ParameterName = name;
            para.DbType = sqlType.DbType;
            para.Value = SqlTypeConversion.ConvertToSqlValue(realizedValue);
            comm.Parameters.Add(para);
        }

        /// <summary>
        /// Create a bunch of SQLParameters from the dictionary and add them to the collection
        /// </summary>
        /// <remarks>You can have lazy-loaded values by having the values of the dictionary be
        /// Func{object} - this will call Realize on the values</remarks>
        /// <param name="coll"></param>
        /// <param name="paras"></param>
        public static void AddParameters(this IDbCommand comm, Dictionary<string, object> paras)
        {
            foreach (var kv in paras)
                comm.AddParameter(kv.Key, kv.Value);
        }

        /// <summary>
        /// Execute some sql and return the results as a list of dictionaries
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static ICollection<Dictionary<string, object>> Execute(this IDbConnection conn, string sql)
        {
            return Execute(conn, sql, new Dictionary<string, object>());
        }

        public static ICollection<Dictionary<string, object>> Execute(this IDbCommand comm, string sql)
        {
            return Execute(comm, sql, new Dictionary<string, object>());
        }

        /// <summary>
        /// Execute some sql and return the results as a list of objects
        /// (doing the appropriate type conversion)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static IEnumerable<T> Execute<T>(this IDbConnection conn, string sql) where T : class, new()
        {
            return Execute<T>(conn, sql, new Dictionary<string, object>());
        }

        public static IEnumerable<T> Execute<T>(this IDbCommand comm, string sql) where T : class, new()
        {
            return Execute<T>(comm, sql, new Dictionary<string, object>());
        }

        /// <summary>
        /// Execute some sql with the supplied parameters and return the results as a list of dictionaries
        /// </summary>
        /// <remarks>You can have lazy-loaded values by having the values of the dictionary be
        /// Func{object} - this will call Realize on the values</remarks>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static ICollection<Dictionary<string, object>> Execute(this IDbConnection conn, string sql,
            Dictionary<string, object> paras)
        {
            ICollection<Dictionary<string, object>> res = null;
            Execute(conn, sql, paras, r => res = r.AllRows());
            return res;
        }

        public static ICollection<Dictionary<string, object>> Execute(this IDbCommand comm, string sql,
            Dictionary<string, object> paras)
        {
            ICollection<Dictionary<string, object>> res = null;
            Execute(comm, sql, paras, r => res = r.AllRows());
            return res;
        }


        /// <summary>
        /// Execute some sql with the supplied parameters and return the results as a list of objects
        /// (doing the appropriate type conversion)
        /// </summary>
        /// <remarks>You can have lazy-loaded values by having the values of the dictionary be
        /// Func{object} - this will call Realize on the values</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static IEnumerable<T> Execute<T>(this IDbConnection conn, string sql, 
            Dictionary<string, object> paras) where T: class, new()
        {
            IEnumerable<T> res = null;
            Execute(conn, sql, paras, r => res = r.ReadAll<T>());
            return res;
        }

        public static IEnumerable<T> Execute<T>(this IDbCommand comm, string sql,
            Dictionary<string, object> paras) where T : class, new()
        {
            IEnumerable<T> res = null;
            Execute(comm, sql, paras, r => res = r.ReadAll<T>());
            return res;
        }

        /// <summary>
        /// Execute some sql with the supplied parameters and return the results as a list of dictionaries
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameterObject">Converted into a dictionary of field-value pairs, which are
        /// are used as the parameters of the query</param>
        /// <returns></returns>
        public static ICollection<Dictionary<string, object>> Execute(this IDbConnection conn, string sql, object parameterObject)
        {
            var dict = parameterObject as Dictionary<string, object>;
            return Execute(conn, sql, dict ?? parameterObject._AsDictionary());
        }

        public static ICollection<Dictionary<string, object>> Execute(this IDbCommand comm, string sql, object parameterObject)
        {
            var dict = parameterObject as Dictionary<string, object>;
            return Execute(comm, sql, dict ?? parameterObject._AsDictionary());
        }

        /// <summary>
        /// Execute some sql with the supplied parameters and return the results as a list of objects
        /// (doing the appropriate type conversion)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameterObject">Converted into a dictionary of field-value pairs, which are
        /// are used as the parameters of the query</param>
        /// <returns></returns>
        public static IEnumerable<T> Execute<T>(this IDbConnection conn, string sql, object parameterObject) where T: class, new()
        {
            var dict = parameterObject as Dictionary<string, object>;
            return Execute<T>(conn, sql, dict ?? parameterObject._AsDictionary());
        }
        public static IEnumerable<T> Execute<T>(this IDbCommand comm, string sql, object parameterObject) where T : class, new()
        {
            var dict = parameterObject as Dictionary<string, object>;
            return Execute<T>(comm, sql, dict ?? parameterObject._AsDictionary());
        }
            

        /// <summary>
        /// Execute the query and return the single value returned by the query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static T ExecuteSingleValue<T>(this IDbConnection conn, string sql)
        {
            return ExecuteSingleValue<T>(conn, sql, new Dictionary<string, object>());
        }

        public static T ExecuteSingleValue<T>(this IDbCommand comm, string sql)
        {
            return ExecuteSingleValue<T>(comm, sql, new Dictionary<string, object>());
        }

        /// <summary>
        /// Execute the query and return the single value returned by the query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public static T ExecuteSingleValue<T>(this IDbConnection conn, string sql, object parameterObject)
        {
            return ExecuteSingleValue<T>(conn, sql, parameterObject._AsDictionary());
        }

        public static T ExecuteSingleValue<T>(this IDbCommand comm, string sql, object parameterObject)
        {
            return ExecuteSingleValue<T>(comm, sql, parameterObject._AsDictionary());
        }

        /// <summary>
        /// Executes the query and returns the single value returned. If no value is returned from the db,
        /// return defaultVal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="defaultVal"></param>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public static T ExecuteSingleValueOr<T>(this IDbConnection conn, T defaultVal, string sql, object parameterObject)
        {
            return ExecuteSingleValueOr<T>(conn, defaultVal, sql, parameterObject._AsDictionary());
        }

        public static T ExecuteSingleValueOr<T>(this IDbConnection conn, T defaultVal, string sql, Dictionary<string, object> parameters)
        {
            return ExecuteSingleColumn<T>(conn, sql, parameters).SingleOr(defaultVal);
        }

        /// <summary>
        /// Execute the query and return the single value returned by the query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T ExecuteSingleValue<T>(this IDbConnection conn, string sql, Dictionary<string, object> parameters)
        {
            return ExecuteSingleColumn<T>(conn, sql, parameters).Single();
        }

        public static T ExecuteSingleValue<T>(this IDbCommand comm, string sql, Dictionary<string, object> parameters)
        {
            return ExecuteSingleColumn<T>(comm, sql, parameters).Single();
        }

        /// <summary>
        /// Execute the query and return the results as a single column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static ICollection<T> ExecuteSingleColumn<T>(this IDbConnection conn, string sql)
        {
            return ExecuteSingleColumn<T>(conn, sql, new Dictionary<string, object>());
        }

        public static ICollection<T> ExecuteSingleColumn<T>(this IDbCommand comm, string sql)
        {
            return ExecuteSingleColumn<T>(comm, sql, new Dictionary<string, object>());
        }

        /// <summary>
        /// Execute the query and return the results as a single column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public static ICollection<T> ExecuteSingleColumn<T>(this IDbConnection conn, string sql, object parameterObject)
        {
            var dict = parameterObject as Dictionary<string, object>;
            return ExecuteSingleColumn<T>(conn, sql, dict ?? parameterObject._AsDictionary());
        }

        public static ICollection<T> ExecuteSingleColumn<T>(this IDbCommand comm, string sql, object parameterObject)
        {
            var dict = parameterObject as Dictionary<string, object>;
            return ExecuteSingleColumn<T>(comm, sql, dict ?? parameterObject._AsDictionary());
        }

        /// <summary>
        /// Execute the query and return the results as a single column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static ICollection<T> ExecuteSingleColumn<T>(this IDbConnection conn, string sql, Dictionary<string, object> parameters)
        {
            var data = new List<T>();
            Execute(conn, sql, parameters, reader => data = reader.SingleColumn<T>().ToList());
            return new ReadOnlyCollection<T>(data);
        }

        public static ICollection<T> ExecuteSingleColumn<T>(this IDbCommand comm, string sql, Dictionary<string, object> parameters)
        {
            var data = new List<T>();
            Execute(comm, sql, parameters, reader => data = reader.SingleColumn<T>().ToList());
            return new ReadOnlyCollection<T>(data);
        }

        /// <summary>
        /// Execute the query and run the callback with the results
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <param name="callback"></param>
        public static void Execute(this IDbConnection conn, string sql,
            Dictionary<string, object> paras, Action<IDataReader> callback)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            using (var comm = conn.CreateCommand())
            {
                comm.CommandText = sql;
                comm.Execute(sql, paras, callback);
            }
        }

        public static void Execute(this IDbCommand comm, string sql,
            Dictionary<string, object> paras, Action<IDataReader> callback)
        {            
            comm.CommandType = CommandType.Text;
            comm.CommandText = sql;
            comm.AddParameters(paras);
            using (var reader = comm.ExecuteReader())
            {
                callback(reader);
            }
        }

        /// <summary>
        /// Execute the query and run the callback with the results
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="callback"></param>
        public static void Execute(this IDbConnection conn, string sql, Action<IDataReader> callback)
        {
            Execute(conn, sql, new Dictionary<string, object>(), callback);
        }

        public static void Execute(this IDbCommand comm, string sql, Action<IDataReader> callback)
        {
            Execute(comm, sql, new Dictionary<string, object>(), callback);
        }

        /// <summary>
        /// Execute the query and run the callback with the results
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="parameterObject"></param>
        /// <param name="callback"></param>
        public static void Execute(this IDbConnection conn, string sql, object parameterObject, Action<IDataReader> callback)
        {
            var dict = parameterObject as Dictionary<string, object>;
            Execute(conn, sql, dict ?? parameterObject._AsDictionary(), callback);
        }

        public static void Execute(this IDbCommand comm, string sql, object parameterObject, Action<IDataReader> callback)
        {
            var dict = parameterObject as Dictionary<string, object>;
            Execute(comm, sql, dict ?? parameterObject._AsDictionary(), callback);
        }
    }
}
