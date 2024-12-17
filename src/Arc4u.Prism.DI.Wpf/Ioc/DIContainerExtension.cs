using Arc4u.Dependency;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;
using IContainerRegistry = Prism.Ioc.IContainerRegistry;

namespace Prism.DI.Ioc;

public abstract class DIContainerExtension : IContainerExtension<IServiceProvider>
{
    protected IServiceProvider Container { get; }

    public bool SupportsModules => true;

    IServiceProvider IContainerExtension<IServiceProvider>.Instance => Container;

    public DIContainerExtension(IServiceProvider container)
    {
        Container = container;
    }

    public abstract void FinalizeExtension();

    public object? Resolve(Type type)
    {
        return Container?.GetService(type);
    }

    public object? Resolve(Type type, string name)
    {
        return Container?.GetRequiredKeyedService(type, name);
    }

    public object? ResolveViewModelForView(object view, Type viewModelType)
    {
        return Container?.GetService(viewModelType);
    }

    public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
    {
        throw new NotImplementedException();
    }

    public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
    {
        throw new NotImplementedException();
    }

    IContainerRegistry IContainerRegistry.RegisterInstance(Type type, object instance)
    {
        Container?.RegisterInstance(type, instance);
        return this;
    }

    public IContainerRegistry RegisterInstance(Type type, object instance, string name)
    {
        Container?.RegisterInstance(type, instance, name);
        return this;
    }

    IContainerRegistry IContainerRegistry.RegisterSingleton(Type from, Type to)
    {
        Container?.RegisterSingleton(from, to);
        return this;
    }

    public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
    {
        Container?.RegisterSingleton(from, to, name);
        return this;
    }

    IContainerRegistry IContainerRegistry.Register(Type from, Type to)
    {
        Container?.Register(from, to);
        return this;
    }

    IContainerRegistry IContainerRegistry.Register(Type from, Type to, string name)
    {
        Container?.Register(from, to, name);
        return this;
    }

    public bool IsRegistered(Type type)
    {
        return Container.TryResolve(type, out var value);
    }

    public bool IsRegistered(Type type, string name)
    {
        return Container.TryResolve(type, name, out var value);
    }
}
