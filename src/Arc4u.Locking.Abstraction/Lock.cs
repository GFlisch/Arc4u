namespace Arc4u.Locking.Abstraction;

public class Lock : IDisposable
{
    private readonly Func<Task> _keepAliveFunction;
    private readonly Action _releaseFunction;
    private readonly CancellationTokenSource _cancelTokenSource;

    public Lock(Func<Task> keepAliveFunction, Action releaseFunction, CancellationToken cancellationToken)
    {
    
    new CancellationTokenSource().Cancel();
        _keepAliveFunction = keepAliveFunction;
        _releaseFunction = releaseFunction;

        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(() =>
        {
            if (cancellationToken.WaitHandle.WaitOne(TimeSpan.FromDays(1)))
            {
                _releaseFunction();
            }
        }, _cancelTokenSource.Token);
    }


    public Task KeepAlive()
    {
        return _keepAliveFunction();
    }
    
    public void Dispose()
    {
        _cancelTokenSource.Cancel();
        // Todo: should we call the release function, if we had a cancellation in between ?
        _releaseFunction();
    }
}