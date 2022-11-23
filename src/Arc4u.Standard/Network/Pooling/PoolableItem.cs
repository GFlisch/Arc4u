using System;
using System.Threading.Tasks;
#nullable enable
namespace Arc4u.Network.Pooling
{
    public abstract class PoolableItem : IDisposable
    {
        public abstract bool IsActive { get; }


        public Func<PoolableItem, Task>? ReleaseClient { get; set; }

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
}
#nullable restore