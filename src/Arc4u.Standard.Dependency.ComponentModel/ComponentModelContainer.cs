using Arc4u.Dependency.Attribute;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Arc4u.Dependency.ComponentModel
{
    public class ComponentModelContainer : IContainer
    {
        private readonly NameResolver NameResolver;

        public Object Instance => _serviceProvider;

        public bool CanCreateScope => true;

        public IServiceProvider ServiceProvider => _serviceProvider;

        private IServiceCollection _collection = null;
        private IServiceScope _serviceScope = null;
        protected bool disposed = false;

        public ComponentModelContainer() : this(new ServiceCollection())
        {
        }

        public ComponentModelContainer(IServiceCollection collection)
        {
            _collection = collection;
            NameResolver = new NameResolver();
            // register the instance as a singleton. 
            // So this is persisted and provided during the creation of a scope instance of the ComponentModelContainer.
            collection.TryAddSingleton<NameResolver>(NameResolver);
            collection.TryAddScoped<IContainerResolve, ComponentModelContainer>();
        }

        public ComponentModelContainer(IServiceProvider provider, NameResolver nameResolution)
        {
            _serviceProvider = provider;
            NameResolver = nameResolution;
        }

        /// <summary>
        /// Used when a specific call to create a scope instance is performed.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="serviceScope"></param>
        protected ComponentModelContainer(IServiceScope serviceScope, NameResolver nameResolution)
        {
            if (null == serviceScope)
                throw new ArgumentNullException(nameof(serviceScope));

            _serviceProvider = serviceScope.ServiceProvider;
            _serviceScope = serviceScope;
            NameResolver = nameResolution;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _serviceScope?.Dispose();
                    disposed = true;
                }
            }
        }

        private IServiceProvider _serviceProvider;
        public void CreateContainer()
        {
            _serviceProvider = _collection.BuildServiceProvider();
        }

        public IContainerResolve CreateScope()
        {
            // As this must be used with a Using, the Dipose will dispose the Scope!
            return new ComponentModelContainer(_serviceProvider.CreateScope(), NameResolver);
        }

        public void Initialize(Type[] types, params Assembly[] assemblies)
        {
            var attributeInspector = new AttributeInspector(this);
            if (null != types)
                foreach (var type in types)
                    attributeInspector.Register(type);

            if (null != assemblies)
                foreach (var assembly in assemblies)
                    attributeInspector.Register(assembly);
        }

        private void Add2NameResolution(Type from, Type to, string name)
        {
            var key = new Tuple<string, Type>(name, from);
            if (NameResolver.NameResolution.ContainsKey(key))
            {
                if (!NameResolver.NameResolution[key].Contains(to))
                    NameResolver.NameResolution[key].Add(to);
            }
            else
                NameResolver.NameResolution.Add(key, new List<Type> { to });
        }

        private void Add2InstanceNameResolution(Type from, object instance, string name)
        {
            var key = new Tuple<string, Type>(name, from);
            if (NameResolver.InstanceNameResolution.ContainsKey(key))
            {
                if (!NameResolver.InstanceNameResolution[key].Contains(instance))
                    NameResolver.InstanceNameResolution[key].Add(instance);
            }
            else
                NameResolver.InstanceNameResolution.Add(key, new List<object> { instance });
        }

        public void Register(Type from, Type to)
        {
            if (from != to)
                _collection.AddTransient(from, to);
            else
                _collection.AddTransient(to);
        }

        public void Register(Type from, Type to, string name)
        {
            ThrowIfNameIsNull(name);

            _collection.TryAddTransient(to);

            Add2NameResolution(from, to, name);
        }

        public void RegisterInstance<T>(T instance) where T : class
        {
            _collection.AddSingleton(instance);
        }

        public void RegisterInstance(Type type, object instance)
        {
            _collection.AddSingleton(type, instance);
        }

        public void RegisterInstance<T>(T instance, string name) where T : class
        {
            ThrowIfNameIsNull(name);

            Add2InstanceNameResolution(typeof(T), instance, name);
        }

        public void RegisterInstance(Type type, object instance, string name)
        {
            ThrowIfNameIsNull(name);

            Add2InstanceNameResolution(type, instance, name);
        }

        public void RegisterSingleton(Type from, Type to)
        {
            if (from != to)
                _collection.AddSingleton(from, to);
            else
                _collection.AddSingleton(to);
        }

        public void RegisterSingleton(Type from, Type to, string name)
        {
            ThrowIfNameIsNull(name);

            _collection.TryAddSingleton(to);

            // As we resolve by name, we don't register this to be resolvable by the engine directly!
            Add2NameResolution(from, to, name);
        }

        public void Register<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            if (typeof(TFrom) != typeof(To))
                _collection.AddTransient<TFrom, To>();
            else
                _collection.AddTransient<To>();
        }

        public void Register<TFrom, To>(string name) where TFrom : class where To : class, TFrom
        {
            ThrowIfNameIsNull(name);

            _collection.TryAddTransient<To>();

            Add2NameResolution(typeof(TFrom), typeof(To), name);
        }

        public void RegisterScoped(Type from, Type to)
        {
            if (from != to)
                _collection.AddScoped(from, to);
            else
                _collection.AddScoped(to);
        }
        public void RegisterScoped<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            if (typeof(TFrom) != typeof(To))
                _collection.AddScoped<TFrom, To>();
            else
                _collection.AddScoped<To>();
        }

        public void RegisterScoped<TFrom, To>(string name) where TFrom : class where To : class, TFrom
        {
            ThrowIfNameIsNull(name);

            _collection.TryAddScoped<To>();

            Add2NameResolution(typeof(TFrom), typeof(To), name);
        }

        public void RegisterScoped(Type from, Type to, string name)
        {
            ThrowIfNameIsNull(name);

            _collection.TryAddScoped(to);

            // As we resolve by name, we don't register this to be resolvable by the engine directly!
            Add2NameResolution(from, to, name);
        }

        /// <summary>
        /// If a signleton is registered with the same implementation, the same instance will be returned!
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="To"></typeparam>
        public void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            if (typeof(TFrom) != typeof(To)) // already added.
                _collection.AddSingleton<TFrom, To>();
            else
                _collection.TryAddSingleton<To>();
        }

        public void RegisterSingleton<TFrom, To>(string name) where TFrom : class where To : class, TFrom
        {
            ThrowIfNameIsNull(name);

            _collection.TryAddSingleton<To>();

            Add2NameResolution(typeof(TFrom), typeof(To), name);
        }

        private void ThrowIfNameIsNull(string name)
        {
            if (null == name)
                throw new ArgumentNullException(nameof(name));
        }

        private void ThrowIfNull()
        {
            if (null == Instance)
                throw new NullReferenceException("DI container is null.");
        }

        private IEnumerable<object> GetNamedInstances(Type type, string name, bool throwIfError)
        {
            IEnumerable<object> instances;
            var key = Tuple.Create(name, type);

            if (NameResolver.InstanceNameResolution.TryGetValue(key, out var oto))
            {
                if (oto.Count != 1)
                    if (throwIfError)
                        throw new IndexOutOfRangeException($"More than one instance type is registered for name {name}.");
                    else
                        return new List<object>();
                instances = oto;
            }
            else
            {
                if (!NameResolver.NameResolution.TryGetValue(key, out var to))
                    if (throwIfError)
                        throw new NullReferenceException($"No type found registered with the name: {name}.");
                    else
                        return new List<object>();

                if (to.Count != 1)
                    if (throwIfError)
                        throw new IndexOutOfRangeException($"More than one type is registered for name {name}.");
                    else
                        return new List<object>();

                instances = _serviceProvider.GetServices(to.First());
            }

            return instances;
        }


        private bool InternalTryResolve(Type type, string name, bool throwIfError, out object value)
        {
            value = null;
            if (null == Instance)
                if (throwIfError)
                    throw new NullReferenceException("DI container is null.");
                else
                    return false;
            IEnumerable<object> instances;

            if (name is null)
            {
                // On Blazor GetService<T> doesn't return null but throw a NullReferenceException...
                try
                {
                    value = _serviceProvider.GetService(type);
                }
                catch
                {
                    // value is already null.            
                }
                
            }
            else
            {
                instances = GetNamedInstances(type, name, throwIfError);
                if (instances.Count() > 1)
                    if (throwIfError)
                        throw new MultipleRegistrationException(type, instances);
                    else
                        return false;
                value = instances.FirstOrDefault();
            }

            return value != null;
        }


        private bool InternalTryResolve<T>(string name, bool throwIfError, out T value)
        {
            value = default(T);
            if (null == Instance)
                if (throwIfError)
                    throw new NullReferenceException("DI container is null.");
                else
                    return false;

            if (name == null)
            {
                // On Blazor GetService<T> doesn't return null but throw a NullReferenceException...
                try
                {
                    var instance = _serviceProvider.GetService<T>();
                    //if (instances.Count() > 1)
                    //    if (throwIfError)
                    //        throw new MultipleRegistrationException<T>(instances);
                    //    else
                    //        return false;
                    value = instance;
                }
                catch 
                {
                    // value is already null.            
                }                
            }
            else
            {
                var instances = GetNamedInstances(typeof(T), name, throwIfError);
                if (instances.Count() > 1)
                    if (throwIfError)
                        throw new MultipleRegistrationException(typeof(T), instances);
                    else
                        return false;
                value = (T)instances.FirstOrDefault();
            }

            return value != null;
        }


        public T Resolve<T>()
        {
            InternalTryResolve<T>(null, true, out var value);
            return value;
        }

        public object Resolve(Type type)
        {
            InternalTryResolve(type, null, true, out var value);
            return value;
        }

        public T Resolve<T>(string name)
        {
            InternalTryResolve<T>(name, true, out var value);
            return value;
        }

        public object Resolve(Type type, string name)
        {
            InternalTryResolve(type, name, true, out var value);
            return value;
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            ThrowIfNull();

            return _serviceProvider.GetServices<T>();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            ThrowIfNull();

            return _serviceProvider.GetServices(type);
        }

        public IEnumerable<T> ResolveAll<T>(string name)
        {
            ThrowIfNull();

            if (null == name) return ResolveAll<T>();

            if (NameResolver.InstanceNameResolution.TryGetValue(new Tuple<string, Type>(name, typeof(T)), out var oto))
            {
                return oto.Cast<T>();
            }


            if (!NameResolver.NameResolution.TryGetValue(new Tuple<string, Type>(name, typeof(T)), out var to))
                throw new NullReferenceException($"No type found registered with the name: {name}.");

            var instances = to.Select(type => _serviceProvider.GetService(type)).Cast<T>();

            return instances;
        }

        public IEnumerable<object> ResolveAll(Type type, string name)
        {
            ThrowIfNull();

            if (null == name) return ResolveAll(type);

            if (NameResolver.InstanceNameResolution.TryGetValue(new Tuple<string, Type>(name, type), out var oto))
            {
                return oto;
            }

            if (!NameResolver.NameResolution.TryGetValue(new Tuple<string, Type>(name, type), out var to))
                throw new NullReferenceException($"No type found registered with the name: {name}.");

            var instances = to.Select(_type => _serviceProvider.GetService(_type));

            return instances;
        }

        public bool TryResolve<T>(out T value)
        {
            return InternalTryResolve(null, false, out value);
        }

        public bool TryResolve(Type type, out object value)
        {
            return InternalTryResolve(type, null, false, out value);
        }

        public bool TryResolve<T>(string name, out T value)
        {
            return InternalTryResolve(name, false, out value);
        }

        public bool TryResolve(Type type, string name, out object value)
        {
            return InternalTryResolve(type, name, false, out value);
        }

        public void RegisterFactory<T>(Func<T> exportedInstanceFactory) where T : class
        {
            _collection.AddTransient<T>(x => exportedInstanceFactory());
        }

        public void RegisterFactory(Type type, Func<object> exportedInstanceFactory)
        {
            _collection.AddTransient(type, x => exportedInstanceFactory());
        }

        public void RegisterSingletonFactory<T>(Func<T> exportedInstanceFactory) where T : class
        {
            _collection.AddSingleton<T>(x => exportedInstanceFactory());
        }

        public void RegisterSingletonFactory(Type type, Func<object> exportedInstanceFactory)
        {
            _collection.AddSingleton(type, x => exportedInstanceFactory());
        }

        public void RegisterScopedFactory<T>(Func<T> exportedInstanceFactory) where T : class
        {
            _collection.AddScoped<T>(x => exportedInstanceFactory());
        }
        public void RegisterScopedFactory(Type type, Func<object> exportedInstanceFactory)
        {
            _collection.AddScoped(type, x => exportedInstanceFactory());
        }


        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

    }
}
