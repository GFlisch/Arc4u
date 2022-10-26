using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;

namespace Arc4u.Dependency
{
    [Obsolete("Use IServiceCollection instead. Call AddNamedServicesSupport() on it if you want named services")]
    public interface IContainerRegistry: IServiceCollection
    {
        void CreateContainer();
    }

    /// <summary>
    /// Obsolete <see cref="IContainerRegistry"/> methods are kept as extension methods for compatibility
    /// </summary>
    public static class ContainerRegistryObsoleteMethods
    {
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddExportableTypes")]
        public static void Initialize(this IContainerRegistry services, Type[] types, params Assembly[] assemblies)
        {
            services.AddExportableTypes(types);
            services.AddExportableTypes(assemblies);
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient<From, To>")]
        public static void Register<TFrom, To>(this IContainerRegistry services) where TFrom : class where To : class, TFrom
        {
            services.AddTransient<TFrom, To>();
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient(from, to)")]
        public static void Register(this IContainerRegistry services, Type from, Type to)
        {
            services.AddTransient(from, to);
        }

        [Obsolete("Use AddTransient instead of Register for named registrations")]
        public static void Register<TFrom, To>(this IContainerRegistry services, string name) where TFrom : class where To : class, TFrom
        {
            services.AddTransient<TFrom, To>(name);
        }

        [Obsolete("Use AddTransient instead of Register for named registrations")]
        public static void Register(this IContainerRegistry services, Type from, Type to, string name)
        {
            services.AddTransient(from, to, name);
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient<T>(provider => exportedInstanceFactory()")]
        public static void RegisterFactory<T>(this IContainerRegistry services, Func<T> exportedInstanceFactory) where T : class
        {
            services.AddTransient<T>(provider => exportedInstanceFactory());
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient(type, provider => exportedInstanceFactory())")]
        public static void RegisterFactory(this IContainerRegistry services, Type type, Func<object> exportedInstanceFactory)
        {
            services.AddTransient(type, provider => exportedInstanceFactory());
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(exportedInstanceFactory)")]
        public static void RegisterSingletonFactory<T>(this IContainerRegistry services, Func<T> exportedInstanceFactory) where T : class
        {
            services.AddSingleton(provider => exportedInstanceFactory());
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(type, provider => exportedInstanceFactory())")]
        public static void RegisterSingletonFactory(this IContainerRegistry services, Type type, Func<object> exportedInstanceFactory)
        {
            services.AddSingleton(type, provider => exportedInstanceFactory());
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped(provider => exportedInstanceFactory())")]
        public static void RegisterScopedFactory<T>(this IContainerRegistry services, Func<T> exportedInstanceFactory) where T : class
        {
            services.AddScoped(provider => exportedInstanceFactory());
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped(type, provider => exportedInstanceFactory())")]
        public static void RegisterScopedFactory(this IContainerRegistry services, Type type, Func<object> exportedInstanceFactory)
        {
            services.AddScoped(type, provider => exportedInstanceFactory());
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(instance)")]
        public static void RegisterInstance<T>(this IContainerRegistry services, T instance) where T : class
        {
            services.AddSingleton(instance);
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(type, instance)")]
        public static void RegisterInstance(this IContainerRegistry services, Type type, object instance)
        {
            services.AddSingleton(type, instance);
        }

        [Obsolete("Use AddSingleton instead of Register for named registrations")]
        public static void RegisterInstance<T>(this IContainerRegistry services, T instance, string name) where T : class
        {
            services.AddSingleton(instance, name);
        }

        [Obsolete("Use AddSingleton instead of Register for named registrations")]
        public static void RegisterInstance(this IContainerRegistry services, Type type, object instance, string name)
        {
            services.AddSingleton(type, instance, name);
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(from, to)")]
        public static void RegisterSingleton(this IContainerRegistry services, Type from, Type to)
        {
            services.AddSingleton(from, to);
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton<TFrom, To>()")]
        public static void RegisterSingleton<TFrom, To>(this IContainerRegistry services)
        {
            services.AddSingleton(typeof(TFrom), typeof(To));
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped<TFrom, To>()")]
        public static void RegisterScoped<TFrom, To>(this IContainerRegistry services) where TFrom : class where To : class, TFrom
        {
            services.AddScoped<TFrom, To>();
        }

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped(from, to)")]
        public static void RegisterScoped(this IContainerRegistry services, Type from, Type to)
        {
            services.AddScoped(from, to);
        }

        [Obsolete("Use AddScoped instead of Register for named registrations")]
        public static void RegisterScoped<TFrom, To>(this IContainerRegistry services, string name) where TFrom : class where To : class, TFrom
        {
            services.AddScoped<TFrom, To>(name);
        }

        [Obsolete("Use AddScoped instead of Register for named registrations")]
        public static void RegisterScoped(this IContainerRegistry services, Type from, Type to, string name)
        {
            services.AddScoped(from, to, name);
        }

        [Obsolete("Use AddScoped instead of Register for named registrations")]
        public static void RegisterSingleton<TFrom, To>(this IContainerRegistry services, string name) where TFrom : class where To : class, TFrom
        {
            services.AddSingleton<TFrom, To>(name);
        }

        [Obsolete("Use AddSingleton instead of Register for named registrations")]
        public static void RegisterSingleton(this IContainerRegistry services, Type from, Type to, string name)
        {
            services.AddSingleton(from, to, name);
        }
    }
}
