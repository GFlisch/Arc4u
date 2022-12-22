using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Arc4u.Dependency
{
    /// <summary>
    /// Expose the methods for named services in a way that mimics those on <see cref="IServiceCollection"/>.
    /// This has been written as a set of extension methods on <see cref="IServiceCollection"/> instead of having our own collection type,
    /// because the current way hosts are built in .NET does not allow customizing or chaning the service collection: it's always a <see cref="ServiceCollection"/>.
    /// </summary>
    public static class NamedServiceCollectionExtensions
    {
        public static INameResolver NameResolver(this IServiceCollection services)
        {
            foreach (var service in services)
                if (service.ServiceType == typeof(INameResolver))
                    return (INameResolver)service.ImplementationInstance;
            return null;
        }

        #region Methods for registering named Services 

        public static IServiceCollection Add(this IServiceCollection services, ServiceDescriptor serviceDescriptor, string name)
        {
            var resolver = services.NameResolver();
            if (resolver == null)
                throw new ArgumentException("You forgot to call AddNamedServicesSupport() on the service collection");
            var descriptor = resolver.Add(serviceDescriptor, name);
            if (descriptor != null)
                services.TryAdd(descriptor);
            return services;
        }

        public static IServiceCollection Add(this IServiceCollection collection, Type serviceType, Type implementationType, string name, ServiceLifetime lifetime)
        {
            collection.Add(new ServiceDescriptor(serviceType, implementationType, lifetime), name);
            return collection;
        }

        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, object implementationInstance, string name)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));
            services.Add(new ServiceDescriptor(serviceType, implementationInstance), name);
            return services;
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationInstance, string name)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return services.Add(serviceType, implementationInstance, name, ServiceLifetime.Transient);
        }

        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services, string name)
            where TService : class
            where TImplementation : class, TService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddTransient(typeof(TService), typeof(TImplementation), name);
        }

        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Type implementationInstance, string name)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return services.Add(serviceType, implementationInstance, name, ServiceLifetime.Singleton);
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services, string name)
            where TService : class
            where TImplementation : class, TService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(typeof(TService), typeof(TImplementation), name);
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance, string name)
            where TService : class
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(typeof(TService), implementationInstance, name);
        }

        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Type implementationInstance, string name)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return services.Add(serviceType, implementationInstance, name, ServiceLifetime.Scoped);
        }

        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, string name)
            where TService : class
            where TImplementation : class, TService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped(typeof(TService), typeof(TImplementation), name);
        }

        #endregion
    }
}
