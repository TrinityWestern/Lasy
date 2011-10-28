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
    public class LockBox<T> : IEnumerable<T>, IDisposable where T : class, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tablename"></param>
        /// <param name="criteria">An object containing key-value pairs of the rows to lock in the database.
        /// For example {ShouldProcess = true}</param>
        /// <param name="lockDate">If not supplied, will be DateTime.Now</param>
        public LockBox(IReadWrite db, string tablename, object criteria, DateTime? lockDate = null)
        {
            Db = db;
            Tablename = tablename;
            LockId = Guid.NewGuid().ToString();
            Criteria = criteria._AsDictionary();
            LockDate = lockDate ?? DateTime.Now;
        }

        public string LockId { get; protected set; }
        public IReadWrite Db { get; protected set; }
        public string Tablename { get; protected set; }
        public object Criteria { get; protected set; }
        public DateTime LockDate { get; protected set; }

        private object _s_contentLock = new object();
        protected List<T> _s_contents;
        protected List<T> _contents
        {
            get
            {
                if (_s_contents == null)
                    lock (_s_contentLock)
                    {
                        _s_contents = _lockRead(Criteria, LockDate).ToList();
                    }

                return _s_contents;
            }
            set
            {
                _s_contents = value;
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
        protected IEnumerable<T> _lockRead(object criteria, DateTime? lockDate = null)
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
            var fromDb = _readLockedRows(Db, Tablename, lockCriteria.Assoc("LockId", LockId));

            return fromDb;
        }

        /// <summary>
        /// This function is responsible for reading back the rows that we've already locked in whatever
        /// format we need them in
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tablename"></param>
        /// <param name="lockCriteria"></param>
        /// <returns></returns>
        protected virtual IEnumerable<T> _readLockedRows(IReadWrite db, string tablename, Dictionary<string, object> lockCriteria)
        {
            return db.Read<T>(tablename, lockCriteria);
        }

        /// <summary>
        /// Clears the lock
        /// </summary>
        /// <remarks>The list of items that the box had locked</remarks>
        public IEnumerable<T> Unlock()
        {
            var res = _contents;

            // Clear the lockDate and LockId for any row that has this lockId
            var updateData = new { LockId = DBNull.Value, LockDate = DBNull.Value };
            var keys = new { LockId = LockId };
            Db.Update(Tablename, updateData, keys);
            
            // Clear out the cache, so that we report that the box is empty
            _contents = new List<T>();

            return res;
        }

        public void Dispose()
        {
            Unlock();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _contents)
                yield return item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Provides a disposable wrapper for locking rows in a database. When the object is 
    /// disposed, the rows are unlocked. Note: Assumes a LockId and LockDate field on 
    /// the table it's used on
    /// </summary>
    /// <remarks>Best practice is to use this object in a using block, so you release
    /// the locks as soon as your done your operations</remarks>
    public class LockBox : LockBox<Dictionary<string, object>>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tablename"></param>
        /// <param name="criteria">An object containing key-value pairs of the rows to lock in the database.
        /// For example {ShouldProcess = true}</param>
        /// <param name="lockDate">If not supplied, will be DateTime.Now</param>
        public LockBox(IReadWrite db, string tablename, object criteria, DateTime? lockDate = null)
            : base(db, tablename, criteria, lockDate)
        { }

        protected override IEnumerable<Dictionary<string, object>> _readLockedRows(IReadWrite db, string tablename, Dictionary<string, object> lockCriteria)
        {
            return db.Read(tablename, lockCriteria);
        }
    }
}
