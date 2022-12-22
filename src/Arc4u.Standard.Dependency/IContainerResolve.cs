using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    [Obsolete("Use INamedServiceProvider instead")]
    public interface IContainerResolve : INamedServiceProvider, IDisposable
    {
        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService<T> instead")]
        T Resolve<T>();

        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService(type) instead")]
        object Resolve(Type type);

        [Obsolete("IContainerResolve now derives from INamedServiceProvider: use GetService(name) instead")]
        T Resolve<T>(string name);
        [Obsolete("IContainerResolve now derives from INamedServiceProvider: use GetService(type, name) instead")]
        object Resolve(Type type, string name);

        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService<T>()/TryGetService<T> instead")]
        bool TryResolve<T>(out T value);
        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService(type)/TryGetService(type) instead")]
        bool TryResolve(Type type, out object value);

        [Obsolete("IContainerResolve now derives from INamedServiceProvider: use TryGetService<T>(name, out value) instead")]
        bool TryResolve<T>(string name, out T value);
        [Obsolete("IContainerResolve now derives from INamedServiceProvider: use TryGetService(type, name, out value) instead")]
        bool TryResolve(Type type, string name, out object value);

        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetServices<T>() instead")]
        IEnumerable<T> ResolveAll<T>();
        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetServices(type) instead")]
        IEnumerable<object> ResolveAll(Type type);

        [Obsolete("Use GetServices<T>(name) instead")]
        IEnumerable<T> ResolveAll<T>(string name);
        [Obsolete("Use GetServices(type, name) instead")]
        IEnumerable<object> ResolveAll(Type type, string name);

        new IContainerResolve CreateScope();


        /// <summary>
        /// This returns always true since the only implementation of <see cref="IContainerResolve"/> is ComponentModelContainer.
        /// Except in tests, it is not used anywhere else and can be deleted.
        /// </summary>
        bool CanCreateScope { get; }
    }
}
