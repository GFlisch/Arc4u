using System.Transactions;

namespace Arc4u.Transaction
{
    public class ReadUncommittedTransactionScope : BaseTransactionScope
    {
        public ReadUncommittedTransactionScope() : base(IsolationLevel.ReadUncommitted)
        {
        }

        public ReadUncommittedTransactionScope(TransactionScopeOption transactionScopeOption) : base(transactionScopeOption, IsolationLevel.ReadUncommitted)
        {
        }
    }
}
