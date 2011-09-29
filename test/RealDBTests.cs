using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Nvelope.Reflection;
using Lasy;
using NUnit.Framework;

namespace LasyTests
{
    [TestFixture]
    public class RealDBTests
    {
        private const string connString = "Data Source=db3.twu.dev; Initial Catalog=Lasy; User Id=jbabbitt; Password=superCupcake$$;";

        [Test]
        public void CanConnectToDB()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            Assert.IsNull(db.ReadPK("Person", -1));
        }

        [Test]
        public void ReadAll()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            var results = db.ReadAll<Person>();

            int actualCount = int.MinValue;
            using (var conn = new SqlConnection(connString))
            {
                actualCount = conn.ExecuteSingleValue<int>("select count(*) from Person");
            }

            Assert.AreEqual(results.Count(), actualCount);
        }

        [Test (Description = "If we want to select only certain columns from an entire table, rather than entire rows")]
        public void ReadAllCustomFields()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            var desiredColumns = new List<string>(){ "PersonId", "FirstName" };

            var results = db.RawReadAllCustomFields("Person", desiredColumns);

            int actualCount = int.MinValue;
            using (var conn = new SqlConnection(connString))
            {
                actualCount = conn.ExecuteSingleValue<int>("select count(*) from Person");
            }

            //Make sure we get all of the rows in the table
            Assert.AreEqual(results.Count(), actualCount);

            //Make sure we only got the columns we requested rather than entire rows
            Assert.AreEqual(results.First().Keys.ToList(), desiredColumns);
        }

        /// <summary>
        /// If someone else is using the database at the same time, this could fail
        /// </summary>
        [Test]
        public void Insert()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            var contents = db.ReadAll<Person>();

            var person = new Person();
            person.FirstName = "test";
            person.LastName = "person";

            db.Insert("Person", person._AsDictionary());

            Assert.AreEqual(contents.Count() + 1, db.ReadAll<Person>().Count());
        }

        [Test]
        public void Update()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            var person = new Person();
            person.FirstName = "test";
            person.LastName = "person";

            var keys = db.Insert("Person", person._AsDictionary());

            //Retrieve the saved person so we can verify it saved with our intended data
            var retrieved = db.RawRead("Person", keys).First();
            Assert.AreEqual(person.FirstName, retrieved["FirstName"]);
            Assert.AreEqual(person.LastName, retrieved["LastName"]);

            person.FirstName = "newFirst";
            person.LastName = "newLastName";

            //Update and retrieve the person again so we can verify that the Update worked
            db.Update("Person", person._AsDictionary(), keys);
            retrieved = db.RawRead("Person", keys).First();
            Assert.AreEqual(person.FirstName, retrieved["FirstName"]);
            Assert.AreEqual(person.LastName, retrieved["LastName"]);
        }

        [Test]
        public void RealUpdate()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            var person = new Person();
            person.FirstName = "test";
            person.LastName = "person";

            var keys = db.Insert("Person", person._AsDictionary());

            //Retrieve the saved person so we can verify it saved with our intended data
            var retrieved = db.RawRead("Person", keys).First();
            Assert.AreEqual(person.FirstName, retrieved["FirstName"]);
            Assert.AreEqual(person.LastName, retrieved["LastName"]);

            person.FirstName = "newFirst";
            person.LastName = "newLastName";

            //Update and retrieve the person again so we can verify that the Update worked
            var data = person._AsDictionary().Where(kv => !keys.Keys.Contains(kv.Key))
                .ToDictionary(i => i.Key, i => i.Value);
            db.RealUpdate("Person", data, keys);
            retrieved = db.RawRead("Person", keys).First();
            Assert.AreEqual(person.FirstName, retrieved["FirstName"]);
            Assert.AreEqual(person.LastName, retrieved["LastName"]);
        }

        [Test(Description = "Verify that a record has been deleted")]
        public void Delete()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            var person = new Person();
            person.FirstName = "bob";
            person.LastName = "morris";

            var key = db.Insert("Person", person._AsDictionary());
            db.Delete("Person", key);

            Assert.AreEqual(0, db.RawRead("Person", key).Count());
        }

        [Test(Description = "We need to make sure we can pass in null values to inserts")]
        public void AllowNullInsert()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));

            var org = new Organization();
            org.Name = "My Organization";

            var key = db.Insert("Organization", org._AsDictionary());

            //Verify that only 1 row exists with the key of this inserted organization
            Assert.AreEqual(1, db.RawRead("Organization", key).Count());
        }

        [Test(Description = "If a transaction is rolled back we should not see any results in our connection or any other connections")]
        public void TransactionRollback()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));
            var transaction = db.BeginTransaction();

            var person = new Person();
            person.FirstName = "test";
            person.LastName = "person";

            var keys = db.Insert("Person", person._AsDictionary(), transaction);

            transaction.Rollback();

            Assert.AreEqual(0, db.RawRead("Person", keys).Count());
        }

        [Test(Description = "If a transaction is abandoned we want to make sure no results were obtained from the commands inside it")]
        public void TransactionAbandoned()
        {
            var keys = new Dictionary<string, object>();

            using (var db = new RealDB(connString, new SQL2005DBAnalyzer(connString)))
            {
                var transaction = db.BeginTransaction();

                var person = new Person();
                person.FirstName = "test";
                person.LastName = "person";

                keys = db.Insert("Person", person._AsDictionary(), transaction);
            }

            var conn = new RealDB(connString, new SQL2005DBAnalyzer(connString));
            Assert.AreEqual(0, conn.RawRead("Person", keys).Count());
        }

        [Test(Description = "Results should be present on a successful, committed transaction")]
        public void TransactionSuccessful()
        {
            var db = new RealDB(connString, new SQL2005DBAnalyzer(connString));
            var transaction = db.BeginTransaction();

            var person = new Person();
            person.FirstName = "test";
            person.LastName = "person";

            var keys = db.Insert("Person", person._AsDictionary(), transaction);

            transaction.Commit();

            Assert.AreEqual(1, db.RawRead("Person", keys).Count());
        }
    }
}
