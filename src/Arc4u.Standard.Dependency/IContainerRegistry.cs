using System.Reflection;

namespace Arc4u.Dependency;

public interface IContainerRegistry
{
    void Register<TFrom, To>() where TFrom : class where To : class, TFrom;
    void Register(Type from, Type to);

    void Register<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
    void Register(Type from, Type to, string name);

    void RegisterFactory<T>(Func<T> exportedInstanceFactory) where T : class;
    void RegisterFactory(Type type, Func<object> exportedInstanceFactory);

    void RegisterSingletonFactory<T>(Func<T> exportedInstanceFactory) where T : class;
    void RegisterSingletonFactory(Type type, Func<object> exportedInstanceFactory);

    void RegisterScopedFactory<T>(Func<T> exportedInstanceFactory) where T : class;
    void RegisterScopedFactory(Type type, Func<object> exportedInstanceFactory);

    void RegisterInstance<T>(T instance) where T : class;
    void RegisterInstance(Type type, object instance);

    void RegisterInstance<T>(T instance, string name) where T : class;
    void RegisterInstance(Type type, object instance, string name);

    void RegisterSingleton(Type from, Type to);
    void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom;

    void RegisterScoped<TFrom, To>() where TFrom : class where To : class, TFrom;
    void RegisterScoped(Type from, Type to);

    void RegisterScoped<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
    void RegisterScoped(Type from, Type to, string name);

    void RegisterSingleton<TFrom, To>(string name) where TFrom : class where To : class, TFrom;
    void RegisterSingleton(Type from, Type to, string name);

    void Initialize(Type[] types, params Assembly[] assemblies);

    void CreateContainer();
}
