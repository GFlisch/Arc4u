using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace Arc4u.Dependency
{
    /// <summary>
    /// Expose the methods for named services in a way that mimics those on <see cref="IServiceCollection"/>.
    /// This has been written as a set of extension methods on <see cref="IServiceCollection"/> instead of having our own collection type,
    /// because the current way hosts are built in .NET does not allow customizing or chaning the service collection: it's always a <see cref="ServiceCollection"/>.
    /// </summary>
    public static class NamedServiceCollectionMethods
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

#if false
#region Methods for adding standard (unnamed) services, using the same signatures are IServiceCollection

        public static IServiceCollection Add(this IServiceCollection collection, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            collection.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
            return collection;
        }

        private static IServiceCollection Add(this IServiceCollection collection, Type serviceType, Func<IServiceProvider, object> implementationFactory, ServiceLifetime lifetime)
        {
            collection.Add(new ServiceDescriptor(serviceType, implementationFactory, lifetime));
            return collection;
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Type implementationInstance)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return services.Add(serviceType, implementationInstance, ServiceLifetime.Transient);
        }

        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddTransient(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddTransient(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return Add(services, serviceType, implementationFactory, ServiceLifetime.Transient);
        }

        public static IServiceCollection AddTransient<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return services.AddTransient(typeof(TService), implementationFactory);
        }


        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Type implementationInstance)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return services.Add(serviceType, implementationInstance, ServiceLifetime.Singleton);
        }

        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, object implementationInstance)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));
            services.Add(new ServiceDescriptor(serviceType, implementationInstance));
            return services;
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, TService implementationInstance)
            where TService : class
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(typeof(TService), implementationInstance);
        }

        public static IServiceCollection AddSingleton(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return Add(services, serviceType, implementationFactory, ServiceLifetime.Singleton);
        }

        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return services.AddSingleton(typeof(TService), implementationFactory);
        }

        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Type implementationInstance)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null)
                throw new ArgumentNullException(nameof(implementationInstance));

            return services.Add(serviceType, implementationInstance, ServiceLifetime.Scoped);
        }

        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddScoped(this IServiceCollection services, Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return Add(services, serviceType, implementationFactory, ServiceLifetime.Scoped);
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null)
                throw new ArgumentNullException(nameof(implementationFactory));

            return services.AddScoped(typeof(TService), implementationFactory);
        }


        /// <summary>
        /// This is an extremely dangerous and nonstandard way of registering a singleton. However, it needed to be provided for compatibilityand therefore not marked as opsolete.
        /// Contrary to <see cref="AddSingleton{TService, TImplementation}(IServiceCollection)"/>, if a singleton is registered whose service type (<typeparamref name="TFrom"/>) is the same type as the implemntation type (<typeparamref name="To"/>), 
        /// only one service descriptor will be regierered if it is called multiple times.
        /// In all other cases, it will behave like <see cref="AddSingleton{TService, TImplementation}(IServiceCollection)"/>.
        /// Because of this deviant behavior, the original name has been kept, instead of aligning to existing method names.
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="To"></typeparam>
        public static void RegisterSingleton<TFrom, To>(this IServiceCollection services) where TFrom : class where To : class, TFrom
        {
            if (typeof(TFrom) != typeof(To)) // already added.
                services.AddSingleton<TFrom, To>();
            else
                services.TryAdd(new ServiceDescriptor(typeof(To), typeof(To), ServiceLifetime.Singleton));
        }

#endregion
#endif
    }


}
