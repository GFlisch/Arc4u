using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Arc4u.Dependency.ComponentModel
{

    public class ComponentModelContainer : IContainer
    {
        private readonly IServiceCollection _collection;
        private INamedServiceProvider _serviceProvider;
        private INamedServiceScope _scope;

        public ComponentModelContainer() : this(new ServiceCollection())
        {
        }

        public ComponentModelContainer(IServiceCollection collection)
        {
            collection.AddNamedServicesSupport();
            collection.TryAddScoped<IContainerResolve, ComponentModelContainer>();
            _collection = collection;
        }

        public ComponentModelContainer(IServiceProvider serviceProvider, INameResolver nameResolver)
        {
            if (serviceProvider is INamedServiceProvider namedServiceProvider)
                _serviceProvider = namedServiceProvider;
            else
                _serviceProvider = new NamedServiceProvider(serviceProvider, nameResolver);
        }

        private ComponentModelContainer(INamedServiceScope scope)
        {
            _scope = scope;
            _serviceProvider = scope.ServiceProvider;
        }


        #region IContainerRegistry implementation

        public void CreateContainer()
        {
            _serviceProvider = _collection.BuildNamedServiceProvider();
        }

        public void Initialize(Type[] types, params Assembly[] assemblies)
        {
            _collection.AddExportableTypes(types);
            _collection.AddExportableTypes(assemblies);
        }

        public void Register<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            _collection.AddTransient<TFrom, To>();
        }

        public void Register( Type from, Type to)
        {
            _collection.AddTransient(from, to);
        }

        public void Register<TFrom, To>( string name) where TFrom : class where To : class, TFrom
        {
            _collection.AddTransient<TFrom, To>(name);
        }

        public void Register( Type from, Type to, string name)
        {
            _collection.AddTransient(from, to, name);
        }

        public void RegisterFactory<T>( Func<T> exportedInstanceFactory) where T : class
        {
            _collection.AddTransient<T>(provider => exportedInstanceFactory());
        }

        public void RegisterFactory( Type type, Func<object> exportedInstanceFactory)
        {
            _collection.AddTransient(type, provider => exportedInstanceFactory());
        }

        public void RegisterSingletonFactory<T>( Func<T> exportedInstanceFactory) where T : class
        {
            _collection.AddSingleton(provider => exportedInstanceFactory());
        }

        public void RegisterSingletonFactory( Type type, Func<object> exportedInstanceFactory)
        {
            _collection.AddSingleton(type, provider => exportedInstanceFactory());
        }

        public void RegisterScopedFactory<T>( Func<T> exportedInstanceFactory) where T : class
        {
            _collection.AddScoped(provider => exportedInstanceFactory());
        }

        public void RegisterScopedFactory( Type type, Func<object> exportedInstanceFactory)
        {
            _collection.AddScoped(type, provider => exportedInstanceFactory());
        }

        public void RegisterInstance<T>( T instance) where T : class
        {
            _collection.AddSingleton(instance);
        }

        public void RegisterInstance( Type type, object instance)
        {
            _collection.AddSingleton(type, instance);
        }

        public void RegisterInstance<T>( T instance, string name) where T : class
        {
            _collection.AddSingleton(instance, name);
        }

        public void RegisterInstance( Type type, object instance, string name)
        {
            _collection.AddSingleton(type, instance, name);
        }

        public void RegisterSingleton( Type from, Type to)
        {
            _collection.AddSingleton(from, to);
        }

        public void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            _collection.AddSingleton(typeof(TFrom), typeof(To));
        }

        public void RegisterScoped<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            _collection.AddScoped<TFrom, To>();
        }

        public void RegisterScoped( Type from, Type to)
        {
            _collection.AddScoped(from, to);
        }

        public void RegisterScoped<TFrom, To>( string name) where TFrom : class where To : class, TFrom
        {
            _collection.AddScoped<TFrom, To>(name);
        }

        public void RegisterScoped( Type from, Type to, string name)
        {
            _collection.AddScoped(from, to, name);
        }

        public void RegisterSingleton<TFrom, To>( string name) where TFrom : class where To : class, TFrom
        {
            _collection.AddSingleton<TFrom, To>(name);
        }

        public void RegisterSingleton( Type from, Type to, string name)
        {
            _collection.AddSingleton(from, to, name);
        }

        #endregion


        #region IServiceCollection implementation (forwarded to _collection)

        public ServiceDescriptor this[int index] { get => _collection[index]; set => _collection[index]= value; }

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public void Add(ServiceDescriptor item)
        {
            _collection.Add(item);
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _collection.Insert(index, item);
        }

        public bool Remove(ServiceDescriptor item)
        {
            return _collection.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        #endregion

        #region IServiceProvider implementation (forwarded to ServiceProvider)

        public object GetService(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        #endregion

        #region INamedServiceProvider implementation (forwarded to ServiceProvider)

        INamedServiceScope INamedServiceProvider.CreateScope()
        {
            return _serviceProvider.CreateScope();
        }

        public IEnumerable<object> GetServices(Type type, string name)
        {
            return _serviceProvider.GetServices(type, name);
        }

        public bool TryGetService(Type type, string name, bool throwIfError, out object value)
        {
            return _serviceProvider.TryGetService(type, name, throwIfError, out value);
        }

        public object GetService(Type type, string name)
        {
            return _serviceProvider.GetService(type, name);
        }

        #endregion

        #region IContainer implementation

        public object Instance => _serviceProvider;

        #endregion

        #region IContainerResolve implementation

        public bool CanCreateScope => true;

        public T Resolve<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        public object Resolve(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        public T Resolve<T>(string name)
        {
            return _serviceProvider.GetService<T>(name);
        }

        public object Resolve(Type type, string name)
        {
            return _serviceProvider.GetService(type, name);
        }

        public bool TryResolve<T>(out T value)
        {
            value = _serviceProvider.GetService<T>();
            return value is not null;
        }

        public bool TryResolve(Type type, out object value)
        {
            value = _serviceProvider.GetService(type);
            return value is not null;
        }

        public bool TryResolve<T>(string name, out T value)
        {
            return _serviceProvider.TryGetService(name, out value);
        }

        public bool TryResolve(Type type, string name, out object value)
        {
            return _serviceProvider.TryGetService(type, name, out value);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _serviceProvider.GetServices<T>();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return _serviceProvider.GetServices(type);
        }

        public IEnumerable<T> ResolveAll<T>(string name)
        {
            return _serviceProvider.GetServices<T>(name);
        }

        public IEnumerable<object> ResolveAll(Type type, string name)
        {
            return _serviceProvider.GetServices(type, name);
        }

        public IContainerResolve CreateScope()
        {
            return new ComponentModelContainer(((INamedServiceProvider)this).CreateScope());
        }

        public void Dispose()
        {
            if (_scope != null)
            {
                _scope.Dispose();
                _scope = null;
            }
        }

        #endregion
    }
}
