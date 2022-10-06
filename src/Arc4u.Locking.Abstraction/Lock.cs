namespace Arc4u.Locking.Abstraction;

public class Lock : IDisposable
{
    private readonly Action _releaseFunction;

    public Lock(Action releaseFunction)
    {
        _releaseFunction = releaseFunction;
    }

    public void Dispose()
    {
        _releaseFunction();
    }
}