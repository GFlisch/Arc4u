using System;

namespace Arc4u.Dependency
{
    public interface IContainer : IContainerRegistry, IContainerResolve, IServiceProvider
    {
        Object Instance { get; }
    }
}
