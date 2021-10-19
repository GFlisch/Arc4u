using Arc4u.Dependency;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Reflection;

namespace Arc4u.ComponentModel.Composition
{
    public class CompositionContainer : IContainer
    {
        public CompositionContainer()
        {
            Configuration = new ContainerConfiguration();
        }

        public CompositionContainer(ContainerConfiguration configuration)
        {
            Configuration = configuration;
        }

        public CompositionContainer(CompositionHost container)
        {
            container = Container;
        }

        private ContainerConfiguration Configuration { get; set; }
        private CompositionHost Container { get; set; }
        private bool IsCreated { get; set; } = false;
        protected bool disposed = false;

        public Object Instance => Container;

        public bool CanCreateScope => false;

        public void Initialize(Type[] types, params Assembly[] assemblies)
        {
            if (null == Configuration || IsCreated)
                return;

            if (null != types)
                Configuration.WithParts(types);

            Configuration.WithAssemblies(assemblies);
        }

        public void Register<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            Register(typeof(TFrom), typeof(To));
        }

        public void Register(Type from, Type to)
        {
            var mefConvention = new ConventionBuilder();
            mefConvention.ForType(to)
                         .Export((actionBuilder) =>
                         {
                             actionBuilder.AsContractType(from);
                         });

            Configuration?.WithParts(new[] { to }, mefConvention);
        }

        public void Register<TFrom, To>(string name) where TFrom : class where To : class, TFrom
        {
            Register(typeof(TFrom), typeof(To), name);
        }

        public void Register(Type from, Type to, string name)
        {
            var mefConvention = new ConventionBuilder();
            mefConvention.ForType(to)
                         .Export((actionBuilder) =>
                         {
                             actionBuilder.AsContractType(from);
                             actionBuilder.AsContractName(name);
                         });

            Configuration?.WithParts(new[] { to }, mefConvention);
        }

        public void RegisterInstance<T>(T instance) where T : class
        {
            RegisterInstance(typeof(T), instance);
        }

        public void RegisterInstance(Type type, object instance)
        {
            Configuration?.WithProvider(new InstanceExportDescriptorProvider(
                instance, type, null, null));
        }

        public void RegisterInstance<T>(T instance, string name) where T : class
        {
            RegisterInstance(typeof(T), instance, name);
        }
        public void RegisterInstance(Type type, object instance, string name)
        {
            Configuration?.WithProvider(new InstanceExportDescriptorProvider(
                instance, type, name, null));
        }

        public void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom
        {
            RegisterSingleton(typeof(TFrom), typeof(To));
        }

        public void RegisterSingleton(Type from, Type to)
        {
            var mefConvention = new ConventionBuilder();
            mefConvention.ForType(to)
                         .Shared()
                         .Export((actionBuilder) =>
                            {
                                actionBuilder.AsContractType(from);
                            });

            Configuration?.WithParts(new[] { to }, mefConvention);
        }

        public void RegisterSingleton<TFrom, To>(string name) where TFrom : class where To : class, TFrom
        {
            RegisterSingleton(typeof(TFrom), typeof(To), name);
        }

        public void RegisterSingleton(Type from, Type to, string name)
        {
            var mefConvention = new ConventionBuilder();
            mefConvention.ForType(to)
                         .Shared()
                         .Export((actionBuilder) =>
                         {
                             actionBuilder.AsContractType(from);
                             actionBuilder.AsContractName(name);
                         });

            Configuration?.WithParts(new[] { to }, mefConvention);
        }

        public void RegisterScoped(Type from, Type to)
        {
            throw new NotImplementedException();
        }

        void IContainerRegistry.RegisterScoped<TFrom, To>()
        {
            throw new NotImplementedException();
        }

        void IContainerRegistry.RegisterScoped<TFrom, To>(string name)
        {
            throw new NotImplementedException();
        }

        public void RegisterScoped(Type from, Type to, string name)
        {
            throw new NotImplementedException();
        }

        public void CreateContainer()
        {
            if (IsCreated) return;

            Container = Configuration.CreateContainer();

            IsCreated = true;
        }

        public T Resolve<T>()
        {
            ThrowIfNull();
            return Container.GetExport<T>();
        }

        public object Resolve(Type type)
        {
            ThrowIfNull();
            return Container.GetExport(type);
        }
        public T Resolve<T>(string name)
        {
            ThrowIfNull();
            return Container.GetExport<T>(name);
        }
        public object Resolve(Type type, string name)
        {
            ThrowIfNull();
            return Container.GetExport(type, name);
        }

        public object GetService(Type type)
        {
            ThrowIfNull();
            return Container.GetExport(type);
        }

        public bool TryResolve<T>(out T value)
        {
            value = default(T);
            return (null == Container) ? false : Container.TryGetExport<T>(out value);
        }
        public bool TryResolve(Type type, out object value)
        {
            value = null;
            return (null == Container) ? false : Container.TryGetExport(type, out value);
        }
        public bool TryResolve<T>(string name, out T value)
        {
            value = default(T);
            return (null == Container) ? false : Container.TryGetExport<T>(name, out value);
        }

        public bool TryResolve(Type type, string name, out object value)
        {
            value = null;
            return (null == Container) ? false : Container.TryGetExport(type, name, out value);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            ThrowIfNull();
            return Container.GetExports<T>();
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            ThrowIfNull();
            return Container.GetExports(type);
        }

        public IEnumerable<T> ResolveAll<T>(string name)
        {
            ThrowIfNull();
            return Container.GetExports<T>(name);
        }

        public IEnumerable<object> ResolveAll(Type type, string name)
        {
            ThrowIfNull();
            return Container.GetExports(type, name);
        }

        private void ThrowIfNull()
        {
            if (null == Container)
                throw new NullReferenceException("CompositionHost container is null.");
        }

        public void RegisterFactory<T>(Func<T> exportedInstanceFactory) where T : class
        {
            RegisterFactory(exportedInstanceFactory, typeof(T), null);
        }

        public void RegisterFactory(Type type, Func<object> exportedInstanceFactory)
        {
            RegisterFactory(exportedInstanceFactory, type, null);
        }

        private void RegisterFactory(Func<object> exportedInstanceFactory, Type type, string name)
        {
            Configuration?.WithProvider(new DelegateExportDescriptorProvider(
                exportedInstanceFactory, type, name, null, false));
        }

        public void RegisterSingletonFactory<T>(Func<T> exportedInstanceFactory) where T : class
        {
            RegisterSingletonFactory(exportedInstanceFactory, typeof(T), null);
        }

        public void RegisterSingletonFactory(Type type, Func<object> exportedInstanceFactory)
        {
            RegisterSingletonFactory(exportedInstanceFactory, type, null);
        }

        private void RegisterSingletonFactory(Func<object> exportedInstanceFactory, Type type, string name)
        {
            Configuration?.WithProvider(new DelegateExportDescriptorProvider(
                exportedInstanceFactory, type, name, null, true));
        }

        public void RegisterScopedFactory<T>(Func<T> exportedInstanceFactory) where T : class
        {
            throw new NotImplementedException();
        }

        public void RegisterScopedFactory(Type type, Func<object> exportedInstanceFactory)
        {
            throw new NotImplementedException();
        }

        public IContainerResolve CreateScope()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
                if (disposing)
                {
                    Container?.Dispose();
                    disposed = true;
                }
        }
    }
}
