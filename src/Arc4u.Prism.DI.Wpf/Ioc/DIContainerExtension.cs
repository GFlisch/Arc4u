using Microsoft.Extensions.DependencyInjection;
using Arc4u.Dependency;
using Prism.Ioc;
using System;
using IContainerRegistry = Prism.Ioc.IContainerRegistry;

namespace Prism.DI.Ioc
{
    public abstract class DIContainerExtension : IContainerExtension<IContainer>
    {
        protected IContainer Container { get; }

        public bool SupportsModules => true;

        IContainer IContainerExtension<IContainer>.Instance => Container;

        public DIContainerExtension(IContainer container)
        {
            Container = container;
        }

        public abstract void FinalizeExtension();

        public object Resolve(Type type)
        {
            return Container?.GetService(type);
        }

        public object Resolve(Type type, string name)
        {
            return Container?.GetService(type, name);
        }

        public object ResolveViewModelForView(object view, Type viewModelType)
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
            Container?.AddSingleton(type, instance);
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            Container?.AddSingleton(type, instance, name);
            return this;
        }

        IContainerRegistry IContainerRegistry.RegisterSingleton(Type from, Type to)
        {
            Container?.AddSingleton(from, to);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            Container?.AddSingleton(from, to, name);
            return this;
        }

        IContainerRegistry IContainerRegistry.Register(Type from, Type to)
        {
            Container?.AddTransient(from, to);
            return this;
        }

        IContainerRegistry IContainerRegistry.Register(Type from, Type to, string name)
        {
            Container?.AddTransient(from, to, name);
            return this;
        }

        public bool IsRegistered(Type type)
        {
            return Container.TryGetService(type, out var value);
        }

        public bool IsRegistered(Type type, string name)
        {
            return Container.TryGetService(type, name, out var value);
        }
    }
}
