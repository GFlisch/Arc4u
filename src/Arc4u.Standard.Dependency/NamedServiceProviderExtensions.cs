using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    /// <summary>
    /// Extension methods for named service lookup, with the signatures similar to <see cref="ServiceCollectionServiceExtensions"/>.
    /// Note that this is formulated in terms of <see cref="IServiceProvider"/> insted of <see cref="INamedServiceProvider"/> to allow the
    /// use of named service lookup as long as we pass a <see cref="IServiceProvider"/> instance along. 
    /// </summary>
    public static class NamedServiceProviderExtensions
    {
        private static void ThrowInvalidProvider()
        {
            throw new ArgumentException($"The provider is expected to implement {nameof(INamedServiceProvider)}");
        }

        private static INamedServiceProvider AsNamedServiceProvider(this IServiceProvider provider)
        {
            var namedServiceProvider = provider.GetService<INamedServiceProvider>();
            if (provider is not null)
                return namedServiceProvider;
            else
            {
                // throw exception in a separate method to allow for inline expansion under current limitations.
                ThrowInvalidProvider();
                return null;
            }
        }

        public static object GetService(this IServiceProvider provider, Type type, string name)
        {
            provider.AsNamedServiceProvider().TryGetService(type, name, throwIfError: true, out object value);
            return value;
        }

        public static IEnumerable<TService> GetServices<TService>(this IServiceProvider provider, string name)
        {
            return (IEnumerable<TService>)provider.AsNamedServiceProvider().GetServices(typeof(TService), name);
        }


        public static bool TryGetService<T>(this IServiceProvider provider, string name, bool throwIfError, out T value)
        {
            if (provider.AsNamedServiceProvider().TryGetService(typeof(T), name, throwIfError, out var result))
            {
                value = (T)result;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static bool TryGetService<T>(this IServiceProvider provider, string name, out T value)
        {
            return provider.TryGetService(name, throwIfError: false, out value);
        }

        public static bool TryGetService(this IServiceProvider provider, Type serviceType, string name, out object value)
        {
            return provider.AsNamedServiceProvider().TryGetService(serviceType, name, throwIfError: false, out value);
        }


        public static TService GetService<TService>(this IServiceProvider provider, string name)
        {
            return (TService)provider.GetService(typeof(TService), name);
        }

        #region Methods from the INamedServiceProvider interface

        public static IEnumerable<object> GetServices(this IServiceProvider provider, Type type, string name)
        {
            return provider.AsNamedServiceProvider().GetServices(type, name);
        }
        public static bool TryGetService(this IServiceProvider provider, Type type, string name, bool throwIfError, out object value)
        {
            return provider.AsNamedServiceProvider().TryGetService(type, name, throwIfError, out value);
        }

        #endregion
    }
}
