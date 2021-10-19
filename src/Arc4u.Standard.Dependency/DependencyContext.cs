using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    public class DependencyContext
    {
        private IContainer Resolver { get; set; }

        protected DependencyContext()
        {
            throw new UnauthorizedAccessException();
        }
        private DependencyContext(IContainer container)
        {
            Resolver = container ?? throw new ArgumentNullException(nameof(container));
        }

        private static object _lock = new object();

        /// <summary>
        /// Register the <see cref="IContainer"/> if no one is already register.
        /// Do not throw any exceptions so multiple registration will not failed.
        /// </summary>
        /// <param name="container"></param>
        public static void CreateContext(IContainer container)
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

        public static Scope<DependencyContext> CreateContextScoped(IContainer container)
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
        public Object Container { get { return Resolver.Instance; } }

        public T Resolve<T>()
        {
            return Resolver.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return Resolver?.Resolve(type);
        }

        public T Resolve<T>(string name)
        {
            return Resolver.Resolve<T>(name);
        }
        public object Resolve(Type type, string name)
        {
            return Resolver?.Resolve(type, name);
        }

        public bool TryResolve<T>(out T value)
        {
            return Resolver.TryResolve<T>(out value);
        }

        public bool TryResolve(Type type, out object value)
        {
            return Resolver.TryResolve(type, out value);
        }

        public bool TryResolve<T>(string name, out T value)
        {
            return Resolver.TryResolve<T>(name, out value);
        }

        public bool TryResolve(Type type, string name, out object value)
        {
            return Resolver.TryResolve(type, name, out value);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return Resolver?.ResolveAll<T>();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return Resolver?.ResolveAll(type);
        }

        public IEnumerable<T> ResolveAll<T>(string name)
        {
            return Resolver?.ResolveAll<T>(name);
        }
        public IEnumerable<object> ResolveAll(Type type, string name)
        {
            return Resolver?.ResolveAll(type, name);
        }
    }
}
