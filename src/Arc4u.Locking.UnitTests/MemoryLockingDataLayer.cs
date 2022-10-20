using System;
using System.Threading;
using System.Threading.Tasks;
using Arc4u.Locking.Abstraction;

namespace Arc4u.Locking.UnitTests;

internal class MemoryLockingDataLayer : ILockingDataLayer
{
    private int _counter = 0;

    private readonly ReaderWriterLockSlim _lock = new();

    public Task<Lock?> TryCreateLockAsync(string label, TimeSpan maxAge)
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

        var @lock = new Lock(() =>
        {
            _counter = 0;
            return Task.CompletedTask;
        }, () =>
        {
            timer.Dispose();
            _lock.EnterWriteLock();
            _counter = 0;
            _lock.ExitWriteLock();
        });
        return Task.FromResult((Lock?) @lock);
    }
}