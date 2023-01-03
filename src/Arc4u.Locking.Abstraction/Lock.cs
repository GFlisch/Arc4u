namespace Arc4u.Locking.Abstraction;

/// <summary>
///     Lock holding all informations needed for a distributed lock and handling the release of the lock
/// </summary>
/// <remarks>Distributed lock will be release, when this object in being disposed</remarks>
public class Lock : IDisposable
{
    private readonly CancellationTokenSource _cancelTokenSource;
    private readonly Func<Task> _keepAliveFunction;
    private readonly Action _releaseFunction;

    /// <summary>
    ///     Creates a Lock
    /// </summary>
    /// <param name="keepAliveFunction">Method that when called will keep the lock alive on datalayer level</param>
    /// <param name="releaseFunction">Method that when called will delete the lock in datalayer level</param>
    /// <param name="cancellationToken">Token that will cancel the entire locking</param>
    public Lock(Func<Task> keepAliveFunction, Action releaseFunction, CancellationToken cancellationToken)
    {
        _keepAliveFunction = keepAliveFunction;
        _releaseFunction = releaseFunction;

        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(() =>
        {
            if (cancellationToken.WaitHandle.WaitOne(TimeSpan.FromDays(1))) _releaseFunction();
        }, _cancelTokenSource.Token);
    }

    /// <summary>
    ///     Disposes the Lock and calls the releaseFunction provided in the ctor
    /// </summary>
    public void Dispose()
    {
        _cancelTokenSource.Cancel();
        _releaseFunction();
    }

    /// <summary>
    ///     Keeping the Lock alive
    /// </summary>
    /// <returns>Task that can be awaited until the lock was really refreshed</returns>
    /// <remarks>Calls the keepAlivefunctionthat was provided in the ctor </remarks>
    public Task KeepAlive()
    {
        return _keepAliveFunction();
    }
}
