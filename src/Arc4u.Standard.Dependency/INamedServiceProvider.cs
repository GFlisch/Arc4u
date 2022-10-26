using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    /// <summary>
    /// A service provider which also provides named service resolution.
    /// It cannot be as "frugal" as <see cref="IServiceProvider"/>, since we have unique scenarios to implement.
    /// </summary>
    public interface INamedServiceProvider : IServiceProvider
    {
        /// <summary>
        /// This method is a bit of a misnomer: 
        /// If <paramref name="name"/> is not null,  it behaves like "GetRequiredServices" since an exception will be thrown if <paramref name="type"/> is not a known service type.
        /// If <paramref name="name"/> is null, it behaves like GetServices.
        /// This is how the old implementation behaved, and we need to replicate that.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerable<object> GetServices(Type type, string name);

        bool TryGetService(Type type, string name, bool throwIfError, out object value);

        INamedServiceScope CreateScope();
    }

    /// <summary>
    /// Extension methods for named service lookup, with the signatures similar to <see cref="ServiceCollectionServiceExtensions"/>.
    /// Note that this is formulated in terms of <see cref="IServiceProvider"/> insted of <see cref="INamedServiceProvider"/> to allow the
    /// use of named service lookup as long as we pass a <see cref="IServiceProvider"/> instance along. 
    /// </summary>
    public static class NamedServiceProviderExtensions
    {
        private static void ThrowInvalidProvider(IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            else
                throw new ArgumentException($"The provider is expected to implement {nameof(INamedServiceProvider)} but it's {provider.GetType().Name}", nameof(provider));
        }

        private static INamedServiceProvider AsNamedServiceProvider(this IServiceProvider provider)
        {
            if (provider is INamedServiceProvider namedServiceProvider)
                return namedServiceProvider;
            else
            {
                // don't throw exceptions, we want this to have a chance to be expanded inline.
                ThrowInvalidProvider(provider);
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
            return provider.AsNamedServiceProvider().TryGetService(name, throwIfError: false, out value);
        }

        public static bool TryGetService(this IServiceProvider provider, Type serviceType, string name, out object value)
        {
            return provider.AsNamedServiceProvider().TryGetService(serviceType, name, throwIfError: false, out value);
        }


        public static TService GetService<TService>(this IServiceProvider provider, string name)
        {
            return (TService)provider.AsNamedServiceProvider().GetService(typeof(TService), name);
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
