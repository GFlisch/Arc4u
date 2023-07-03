using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    public interface IContainerResolve : IDisposable
    {
        T Resolve<T>();
        object Resolve(Type type);

        T Resolve<T>(string name);
        object Resolve(Type type, string name);

        bool TryResolve<T>(out T value);
        bool TryResolve(Type type, out object value);

        bool TryResolve<T>(string name, out T value);
        bool TryResolve(Type type, string name, out object value);

        IEnumerable<T> ResolveAll<T>();
        IEnumerable<object> ResolveAll(Type type);

        IEnumerable<T> ResolveAll<T>(string name);
        IEnumerable<object> ResolveAll(Type type, string name);

        IContainerResolve CreateScope();

        IServiceProvider ServiceProvider { get; }

        bool CanCreateScope { get; }
    }
}
