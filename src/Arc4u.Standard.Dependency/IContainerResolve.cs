using Microsoft.Extensions.DependencyInjection;
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

#if false
    /// <summary>
    /// Obsolete <see cref="IContainerResolve"/> methods are kept as extension methods for source compatibility
    /// </summary>
    public static class ContainerResolveMethods
    {
        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService<T> instead")]
        public static T Resolve<T>(this INamedServiceProvider containerResolve) where T: notnull
        {
            return containerResolve.GetService<T>();
        }

        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService(type) instead")]
        public static object Resolve(this INamedServiceProvider containerResolve, Type  type)
        {
            return containerResolve.GetService(type);
        }

        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService<T>()/TryGetService<T> instead")]
        public static bool TryResolve<T>(this INamedServiceProvider containerResolve,out T value)
        {
            value = containerResolve.GetService<T>();
            return value is not null;
        }

        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetService(type)/TryGetService(type) instead")]
        public static bool TryResolve(this INamedServiceProvider containerResolve, Type type, out object value)
        {
            value = containerResolve.GetService(type);
            return value is not null;
        }


        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetServices<T>() instead")]
        public static IEnumerable<T> ResolveAll<T>(this INamedServiceProvider containerResolve)
        {
            return containerResolve.GetServices<T>();
        }

        [Obsolete("IContainerResolve now derives from IServiceProvider: use GetServices(type) instead")]
        public static IEnumerable<object> ResolveAll(this INamedServiceProvider containerResolve, Type type)
        {
            return containerResolve.GetServices(type);
        }


        [Obsolete("Use GetServices<T>(name) instead")]
        public static T Resolve<T>(this INamedServiceProvider provider, string name)
        {
            return provider.GetService<T>(name);
        }

        [Obsolete("Use GetServices(type, name) instead")]
        public static object Resolve(this INamedServiceProvider provider, Type type, string name)
        {
            return provider.GetService(type, name);
        }

        [Obsolete("Use TryGetService<T>(name, out value) instead")]
        public static bool TryResolve<T>(this INamedServiceProvider provider, string name, out T value)
        {
            return provider.TryGetService(name, out value);
        }

        [Obsolete("Use TryGetService(type, name, out value) instead")]
        public static bool TryResolve(this INamedServiceProvider provider, Type type, string name, out object value)
        {
            return provider.TryGetService(type, name, out value);
        }

        [Obsolete("Use GetServices<T>(name) instead")]
        public static IEnumerable<T> ResolveAll<T>(this INamedServiceProvider provider, string name)
        {
            return provider.GetServices<T>(name);
        }

        [Obsolete("Use GetServices(name, type) instead")]
        public static IEnumerable<object> ResolveAll(this INamedServiceProvider provider, Type type, string name)
        {
            return provider.GetServices(type, name);
        }
    }
#endif
}
