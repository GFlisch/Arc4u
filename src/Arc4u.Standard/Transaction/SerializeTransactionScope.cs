using System.Transactions;

namespace Arc4u.Transaction
{
    public class SerializeTransactionScope : BaseTransactionScope
    {
        public SerializeTransactionScope() : base(IsolationLevel.Serializable)
        {
        }

        public SerializeTransactionScope(TransactionScopeOption transactionScopeOption) : base(transactionScopeOption, IsolationLevel.Serializable)
        {
        }
    }
}
