using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

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
            return _nameResolver.GetServices(_serviceProvider, type, name);
        }

        public bool TryGetService(Type type, string name, bool throwIfError, out object value)
        {
            return _nameResolver.TryGetService(_serviceProvider, type, name, throwIfError, out value);  
        }

        #endregion
    }
}
