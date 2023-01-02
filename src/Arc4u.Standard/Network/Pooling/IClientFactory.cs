using System;
using System.Threading.Tasks;

namespace Arc4u.Network.Pooling;

public interface IClientFactory<out T>
    where T : PoolableItem
{
    /// <summary>
    /// Create a new instance of <typeparamref name="T"/>
    /// </summary>
    /// <param name="releaseFunc">Function to be called, when the created object is being disposed</param>
    /// <returns>created <typeparamref name="T"/></returns>
    T CreateClient(Func<T, Task> releaseFunc);
}