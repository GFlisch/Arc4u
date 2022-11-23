using System;
using System.Threading.Tasks;

namespace Arc4u.Network.Pooling
{
    public interface IClientFactory<out T>
    where T : PoolableItem
    {
        T CreateClient(Func<T, Task> releaseFunc);
    }
}
