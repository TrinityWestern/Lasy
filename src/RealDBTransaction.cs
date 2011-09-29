using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Lasy
{
    public class RealDBTransaction : ITransaction
    {
        public SqlTransaction UnderlyingTransaction;

        public RealDBTransaction(SqlTransaction transaction)
        {
            UnderlyingTransaction = transaction;
        }

        #region ITransaction Members

        public void Commit()
        {
            UnderlyingTransaction.Commit();
        }

        public void Rollback()
        {
            UnderlyingTransaction.Rollback();
        }   

        #endregion
    }
}
