using System.Transactions;

namespace Arc4u.Transaction
{
    public class ReadCommittedTransactionScope : BaseTransactionScope
    {
        public ReadCommittedTransactionScope() : base(IsolationLevel.ReadCommitted)
        {
        }

        public ReadCommittedTransactionScope(TransactionScopeOption transactionScopeOption) : base(transactionScopeOption, IsolationLevel.ReadCommitted)
        {
        }
    }
}
