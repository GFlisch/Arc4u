using System;
using System.Threading;
using System.Threading.Tasks;
using Arc4u.Locking.Abstraction;

namespace Arc4u.Locking.UnitTests;

internal class MemoryLockingDataLayer : ILockingDataLayer
{
    private int _counter = 0;

    private readonly ReaderWriterLockSlim _lock = new();

    /// <inheritdoc />
    public Task<Lock?> TryCreateLockAsync(string label, TimeSpan maxAge, CancellationToken cancellationToken)
    {
        return InternalTryCreateLockAsync(label, maxAge, null, cancellationToken);
    }
    private Task<Lock?> InternalTryCreateLockAsync(string label, TimeSpan maxAge, Func<Task>? cleanUpCallBack , CancellationToken cancellationToken)
    {
        _lock.EnterReadLock();
        try
        {
            if (_counter > 0)
                return Task.FromResult((Lock?) null);
        }
        finally
        {
            _lock.ExitReadLock();
        }

        var timer = new Timer(state =>
        {
            _lock.EnterWriteLock();
            _counter++;
            _lock.ExitWriteLock();
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        async void ReleaseFunction()
        {
            _lock.EnterWriteLock();
            await timer.DisposeAsync();
            if (cleanUpCallBack is not null)
            {
                await cleanUpCallBack();
            }

            _counter = 0;
            _lock.ExitWriteLock();
        }

        var @lock = new Lock(() =>
        {
            _counter = 0;
            return Task.CompletedTask;
        }, ReleaseFunction, cancellationToken);
        return Task.FromResult((Lock?) @lock);
    }

    public Task<Lock?> TryCreateLockAsync(string label, TimeSpan maxAge, Func<Task> cleanUpCallBack, CancellationToken cancellationToken)
    {
        return InternalTryCreateLockAsync(label, maxAge, cleanUpCallBack, cancellationToken);
    }
}