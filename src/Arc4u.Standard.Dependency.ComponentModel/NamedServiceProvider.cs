using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arc4u.Dependency.ComponentModel
{
    public class NamedServiceProvider : INamedServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INameResolver _nameResolver;


        public NamedServiceProvider(IServiceCollection collection, ServiceProviderOptions options)
        {
            _nameResolver = collection.NameResolver();
            if(_nameResolver == null)
                throw new ArgumentException("To construct a named service provider, you need to add a ", nameof(collection));
            // block changes to the name resolver.
            _nameResolver.Freeze();
            collection.AddScoped<INamedServiceProvider, NamedServiceProvider>();
            _serviceProvider = collection.BuildServiceProvider(options);
        }

        public NamedServiceProvider(IServiceProvider serviceProvider, INameResolver nameResolver)
        {
            _serviceProvider = serviceProvider;
            _nameResolver = nameResolver;
        }

        private sealed class ServiceScope : INamedServiceScope
        {
            private IServiceScope _serviceScope;
            private NamedServiceProvider _serviceProvider;

            public ServiceScope(NamedServiceProvider root)
            {
                _serviceScope = root._serviceProvider.CreateScope();
                _serviceProvider = new NamedServiceProvider(_serviceScope.ServiceProvider, root._nameResolver);
            }

            public IServiceProvider ServiceProvider => _serviceProvider;

            INamedServiceProvider INamedServiceScope.ServiceProvider => _serviceProvider;

            public void Dispose()
            {
                if (_serviceScope != null)
                {
                    _serviceScope.Dispose();
                    _serviceProvider = null;
                }
            }
        }

        public INamedServiceScope CreateScope()
        {
            return new ServiceScope(this);
        }


        #region IServiceProvider implementation

        public object GetService(Type type)
        {
            return _serviceProvider.GetService(type);
        }

        #endregion

        #region INamedServiceProvider implementation


        public IEnumerable<object> GetServices(Type type, string name)
        {
            if (name == null)
                return _serviceProvider.GetServices(type);

            if (_nameResolver.TryGetInstances(name, type, out var singletons))
                return singletons;

            if (!_nameResolver.TryGetTypes(name, type, out var to))
                throw new NullReferenceException($"No type found registered with the name: {name}.");

            var instances = Array.CreateInstance(type, to.Count);
            for (int index = 0; index < to.Count; ++index)
                instances.SetValue(_serviceProvider.GetService(to[index]), index);

            // covariance at work here. This is really a IEnumerable<type>
            return (IEnumerable<object>)instances;
        }

        public bool TryGetService(Type type, string name, bool throwIfError, out object value)
        {
            value = null;

            if (name == null)
                value = _serviceProvider.GetService(type);
            else
            {
                IEnumerable<object> instances;

                if (_nameResolver.TryGetInstances(name, type, out var oto))
                {
                    if (oto.Count != 1)
                        if (throwIfError)
                            throw new IndexOutOfRangeException($"More than one instance type is registered for name {name}.");
                        else
                            return false;
                    instances = oto;
                }
                else
                {
                    if (!_nameResolver.TryGetTypes(name, type, out var to))
                        if (throwIfError)
                            throw new NullReferenceException($"No type found registered with the name: {name}.");
                        else
                            return false;

                    if (to.Count != 1)
                        if (throwIfError)
                            throw new IndexOutOfRangeException($"More than one type is registered for name {name}.");
                        else
                            return false;

                    instances = _serviceProvider.GetServices(to[0]);
                }

                // the happy flow is the single-instance case. Don't waste time counting
                try
                {
                    value = instances.SingleOrDefault();
                }
                catch (Exception e)
                {
                    if (throwIfError)
                        throw new MultipleRegistrationException(type, instances, e);
                    else
                        return false;
                }
            }
            return value != null;
        }

        #endregion
    }
}
