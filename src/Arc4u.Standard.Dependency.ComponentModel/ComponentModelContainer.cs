using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

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

        public void CreateContainer()
        {
            _serviceProvider = _collection.BuildNamedServiceProvider();
        }


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

        #region IContainerResolve implementation

        public bool CanCreateScope => true;

        #endregion

        #region IContainer implementation

        public object Instance => _serviceProvider;

        #endregion

        #region IContainerResolve legacy methods
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
