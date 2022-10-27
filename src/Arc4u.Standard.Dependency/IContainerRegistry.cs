using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Arc4u.Dependency
{
    [Obsolete("Use IServiceCollection instead. Call AddNamedServicesSupport() on it if you want named services")]
    public interface IContainerRegistry: IServiceCollection
    {
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient<From, To>")]
        void Register<TFrom, To>() where TFrom : class where To : class, TFrom;
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient(from, to)")]
        void Register(Type from, Type to);

        [Obsolete("Use AddTransient instead of Register for named registrations")]
        void Register<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        [Obsolete("Use AddTransient instead of Register for named registrations")]
        void Register(Type from, Type to, string name);

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient(provider => exportedInstanceFactory())")]
        void RegisterFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddTransient(type, provider => exportedInstanceFactory())")]
        void RegisterFactory(Type type, Func<object> exportedInstanceFactory);

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(exportedInstanceFactory)")]
        void RegisterSingletonFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(type, provider => exportedInstanceFactory())")]
        void RegisterSingletonFactory(Type type, Func<object> exportedInstanceFactory);

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped(provider => exportedInstanceFactory())")]
        void RegisterScopedFactory<T>(Func<T> exportedInstanceFactory) where T : class;
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped(provider => exportedInstanceFactory())")]
        void RegisterScopedFactory(Type type, Func<object> exportedInstanceFactory);

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(instance)")]
        void RegisterInstance<T>(T instance) where T : class;
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(type, instance)")]
        void RegisterInstance(Type type, object instance);

        [Obsolete("Use AddSingleton instead of Register for named registrations")]
        void RegisterInstance<T>(T instance, string name) where T : class;
        [Obsolete("Use AddSingleton instead of Register for named registrations")]
        void RegisterInstance(Type type, object instance, string name);

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton(from, to)")]
        void RegisterSingleton(Type from, Type to);
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddSingleton<TFrom, To>()")]
        void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom;

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped<TFrom, To>()")]
        void RegisterScoped<TFrom, To>() where TFrom : class where To : class, TFrom;
        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddScoped(from, to)")]
        void RegisterScoped(Type from, Type to);

        [Obsolete("Use AddScoped instead of RegisterScoped for named registrations")]
        void RegisterScoped<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        [Obsolete("Use AddScoped instead of RegisterScoped for named registrations")]
        void RegisterScoped(Type from, Type to, string name);

        [Obsolete("Use AddSingleton instead of RegisterSingleton for named registrations")]
        void RegisterSingleton<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
        [Obsolete("Use AddSingleton instead of RegisterSingleton for named registrations")]
        void RegisterSingleton(Type from, Type to, string name);

        [Obsolete("IContainerRegistry support IServiceCollection method signatures: use AddExportableTypes")]
        void Initialize(Type[] types, params Assembly[] assemblies);

        void CreateContainer();
    }
}
