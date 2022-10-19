namespace Arc4u.Locking.Abstraction;

public class Lock : IDisposable
{
    private readonly string _label;
    private readonly TimeSpan _ttl;
    private readonly Func<Task> _keepAliveFunction;
    private readonly Action _releaseFunction;

    public Lock(Func<Task> keepAliveFunction, Action releaseFunction)
    {
        _keepAliveFunction = keepAliveFunction;
        _releaseFunction = releaseFunction;
    }

    public Task KeepAlive()
    {
        return _keepAliveFunction();
    }
    
    public void Dispose()
    {
        _releaseFunction();
    }
}