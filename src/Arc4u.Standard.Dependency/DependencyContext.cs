using Arc4u.Threading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    public class DependencyContext: INamedServiceProvider
    {
        private INamedServiceProvider Resolver { get; set; }

        protected DependencyContext()
        {
            throw new UnauthorizedAccessException();
        }
        private DependencyContext(INamedServiceProvider container)
        {
            Resolver = container ?? throw new ArgumentNullException(nameof(container));
        }

        private static object _lock = new object();

        /// <summary>
        /// Register the <see cref="IContainer"/> if no one is already register.
        /// Do not throw any exceptions so multiple registration will not failed.
        /// </summary>
        /// <param name="container"></param>
        public static void CreateContext(INamedServiceProvider container)
        {
            if (null == container)
                throw new ArgumentNullException(nameof(container));

            // Do not replace once created.
            lock (_lock)
            {
                if (null == Instance)
                {
                    var context = new DependencyContext(container);
                    Instance = context;
                }
            }
        }

        public static Scope<DependencyContext> CreateContextScoped(INamedServiceProvider container)
        {
            return new Scope<DependencyContext>(new DependencyContext(container));
        }

        private static DependencyContext Instance { get; set; }

        /// <summary>
        /// Return the Scoped context or the normal, basic instance of the context.
        /// </summary>
        public static DependencyContext Current => Scope<DependencyContext>.Current ?? Instance;

        /// <summary>
        /// Return the Dependency instance of the context => defined by the Current property.
        /// </summary>
        public Object Container { get { return Resolver; } }

        #region INamedServiceProvider implementation

        public IEnumerable<object> GetServices(Type type, string name)
        {
            return Resolver?.GetServices(type, name);
        }

        public bool TryGetService(Type type, string name, bool throwIfError, out object value)
        {
            if (Resolver != null)
                return Resolver.TryGetService(type, name, throwIfError, out value);
            else
            {
                value = null;
                return false;
            }
        }

        public object GetService(Type type, string name)
        {
            return Resolver?.GetService(type, name);
        }

        public INamedServiceScope CreateScope()
        {
            // Are we sure we want this? There are 2 notions of "Scope" here: (1) the service scope, and (2) the dependecy context scope.
            // It is confusing to know which is which.
            return Resolver?.CreateScope();
        }

        public object GetService(Type serviceType)
        {
            return Resolver?.GetService(serviceType);
        }
        #endregion

        #region Legacy methods

        [Obsolete("DependencyContext now implements IServiceProvider: Use GetService<T>() instead")]
        public T Resolve<T>()
        {
            return Resolver.GetService<T>();
        }

        [Obsolete("DependencyContext now implements IServiceProvider: Use GetService(type) instead")]
        public object Resolve(Type type)
        {
            return Resolver.GetService(type);
        }

        [Obsolete("DependencyContext now implements INamedServiceProvider: Use GetService<T>(name) instead")]
        public T Resolve<T>(string name)
        {
            return Resolver.GetService<T>(name);
        }

        [Obsolete("DependencyContext now implements INamedServiceProvider: Use GetService(type, name) instead")]
        public object Resolve(Type type, string name)
        {
            return Resolver.GetService(type, name);
        }

        [Obsolete("DependencyContext now implements IServiceProvider: Use GetService<T>/TryGetService<T>() instead")]
        public bool TryResolve<T>(out T value)
        {
            value = Resolver.GetService<T>();
            return value is not null;
        }

        [Obsolete("DependencyContext now implements IServiceProvider: Use GetService(type)/TryGetService(type, value) instead")]
        public bool TryResolve(Type type, out object value)
        {
            value = Resolver.GetService(type);
            return value is not null;
        }

        [Obsolete("DependencyContext now implements INamedServiceProvider: Use TryGetService(name, out value) instead")]
        public bool TryResolve<T>(string name, out T value)
        {
            return Resolver.TryGetService(name, out value);
        }

        [Obsolete("DependencyContext now implements INamedServiceProvider: Use TryGetService(type, name, out value) instead")]
        public bool TryResolve(Type type, string name, out object value)
        {
            return Resolver.TryGetService(type, name, out value);
        }

        [Obsolete("DependencyContext now implements IServiceProvider: Use GetServices<T>() instead")]
        public IEnumerable<T> ResolveAll<T>()
        {
            return Resolver.GetServices<T>();
        }

        [Obsolete("DependencyContext now implements IServiceProvider: Use GetServices(type) instead")]
        public IEnumerable<object> ResolveAll(Type type)
        {
            return Resolver.GetServices(type);
        }

        [Obsolete("DependencyContext now implements INamedServiceProvider: Use GetServices<T>(name) instead")]
        public IEnumerable<T> ResolveAll<T>(string name)
        {
            return Resolver?.GetServices<T>(name);
        }

        [Obsolete("DependencyContext now implements INamedServiceProvider: Use GetServices(type, name) instead")]
        public IEnumerable<object> ResolveAll(Type type, string name)
        {
            return Resolver?.GetServices(type, name);
        }

        #endregion
    }
}
