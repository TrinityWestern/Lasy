using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nvelope;
using Nvelope.Reflection;

namespace Lasy
{
    /// <summary>
    /// Provides a disposable wrapper for locking rows in a database. When the object is 
    /// disposed, the rows are unlocked. Note: Assumes a LockId and LockDate field on 
    /// the table it's used on
    /// </summary>
    /// <remarks>Best practice is to use this object in a using block, so you release
    /// the locks as soon as your done your operations</remarks>
    public class LockBox : IDisposable
    {
        public LockBox(IReadWrite db, string tablename, object criteria, DateTime? lockDate = null)
        {
            Db = db;
            Tablename = tablename;
            LockId = Guid.NewGuid().ToString();
            Criteria = criteria;
            LockDate = lockDate ?? DateTime.Now;
        }

        public string LockId { get; protected set; }
        public IReadWrite Db { get; protected set; }
        public string Tablename { get; protected set; }
        public object Criteria { get; protected set; }
        public DateTime LockDate { get; protected set; }

        private object _contentLock = new object();
        protected IEnumerable<Dictionary<string, object>> _contents;
        public IEnumerable<Dictionary<string, object>> Contents
        {
            get
            {
                if(_contents == null)
                    lock (_contentLock)
                    {
                        _contents = _lockRead(Criteria, LockDate);
                    }

                return _contents;
            }
        }

        /// <summary>
        /// Read and lock some rows from the db. Only returns those rows that were successfully
        /// locked.
        /// </summary>
        /// <param name="criteria">An object containing key-value pairs indicating the rows we want to return.
        /// ie  {Processed = false}</param>
        /// <param name="lockDate"></param>
        /// <returns></returns>
        protected IEnumerable<Dictionary<string, object>> _lockRead(object criteria, DateTime? lockDate = null)
        {
            lockDate = lockDate ?? DateTime.Now;

            // Only lock things that are not currently locked,
            // otherwise we might end up whacking someone else's locks,
            // which would be a Bad Thing (tm)
            var lockCriteria = criteria._AsDictionary().Assoc("LockId", null);

            // Try to write the lock to the rows in the db
            Db.Update(Tablename, new { LockId = LockId, LockDate = lockDate.Value }, lockCriteria);

            // Get all the rows that we successfully locked
            // Don't use the lock date, just the lockId, because SQL Server will 
            // truncate datetimes, so the values won't match
            var fromDb = Db.Read(Tablename, lockCriteria.Assoc("LockId", LockId));

            return fromDb;
        }

        /// <summary>
        /// Clears the lock
        /// </summary>
        public void Unlock()
        {
            // Clear the lockDate and LockId for any row that has this lockId
            var updateData = new { LockId = DBNull.Value, LockDate = DBNull.Value };
            var keys = new { LockId = LockId };
            Db.Update(Tablename, updateData, keys);
        }


        public void Dispose()
        {
            Unlock();
        }
    }
}
