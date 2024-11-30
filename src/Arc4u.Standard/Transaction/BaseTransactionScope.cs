using System.Transactions;

namespace Arc4u.Transaction;

public abstract class BaseTransactionScope : IDisposable
{
    public BaseTransactionScope(TransactionScopeOption transactionScopeOption, IsolationLevel isolationLevel)
    {
        var transactionOption = new TransactionOptions
        {
            IsolationLevel = isolationLevel
        };

        transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOption, TransactionScopeAsyncFlowOption.Enabled);
    }

    public BaseTransactionScope(IsolationLevel isolationLevel) : this(TransactionScopeOption.Required, isolationLevel)
    {
    }

    protected bool disposed;
    protected TransactionScope transactionScope;

    public virtual void Complete()
    {
        transactionScope?.Complete();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(Boolean disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                transactionScope?.Dispose();
                disposed = true;
            }
        }
    }
}
