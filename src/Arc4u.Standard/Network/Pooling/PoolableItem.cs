#nullable enable
using System;
using System.Threading.Tasks;

namespace Arc4u.Network.Pooling;

public abstract class PoolableItem : IDisposable
{
    protected PoolableItem(Func<PoolableItem, Task>? releaseClient)
    {
        ReleaseClient = releaseClient;
    }

    public abstract bool IsActive { get; }

    public Func<PoolableItem, Task>? ReleaseClient { get; }

    public void Dispose()
    {
        Dispose(true);

        ReleaseClient?.Invoke(this);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}
#nullable restore