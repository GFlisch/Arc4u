#if NET8_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Arc4u.Dependency.Attribute;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.Dependency.ComponentModel;

public class ComponentModelContainer : IContainer
{
    public object Instance => _serviceProvider ?? throw new NullReferenceException("DI container is null.");

    public bool CanCreateScope => true;

    public IServiceProvider ServiceProvider => _serviceProvider ?? throw new NullReferenceException("DI container is null.");

    readonly IServiceCollection? _collection;
    private readonly IServiceScope? _serviceScope;
    private IServiceProvider? _serviceProvider;

    protected bool disposed;

    public ComponentModelContainer() : this(new ServiceCollection())
    {
    }

    public ComponentModelContainer([DisallowNull] IServiceCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _collection = collection;
        collection.TryAddScoped<IContainerResolve, ComponentModelContainer>();
    }

    [ActivatorUtilitiesConstructor]
    public ComponentModelContainer(IServiceProvider provider)
    {
        _serviceProvider = provider;
    }

    /// <summary>
    /// Used when a specific call to create a scope instance is performed.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="serviceScope"></param>
    protected ComponentModelContainer([DisallowNull] IServiceScope serviceScope)
    {
        ArgumentNullException.ThrowIfNull(serviceScope);

        _serviceProvider = serviceScope.ServiceProvider;
        _serviceScope = serviceScope;
        _collection ??= new ServiceCollection();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                _serviceScope?.Dispose();
                disposed = true;
            }
        }
    }

    public void CreateContainer()
    {
        if (_serviceProvider is not null)
        {
            throw new InvalidOperationException("Service Provider already created.");
        }

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _serviceProvider = _collection.BuildServiceProvider();
    }

    public IContainerResolve CreateScope()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("No ServiceProvider exist.");
        }

        // As this must be used with a Using, the Dipose will dispose the Scope!
        return new ComponentModelContainer(_serviceProvider.CreateScope());
    }

    public void Initialize(Type[] types, params Assembly[] assemblies)
    {
        var attributeInspector = new AttributeInspector(this);
        if (null != types)
        {
            foreach (var type in types)
            {
                attributeInspector.Register(type);
            }
        }

        if (null != assemblies)
        {
            foreach (var assembly in assemblies)
            {
                attributeInspector.Register(assembly);
            }
        }
    }

    public void Register(Type from, Type to)
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        if (from != to)
        {
            _collection.AddTransient(from, to);
        }
        else
        {
            _collection.AddTransient(to);
        }
    }

    public void Register(Type from, Type to, [DisallowNull] string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.TryAddKeyedTransient(from, name, to);
    }

    public void RegisterInstance<T>(T instance) where T : class
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddSingleton(instance);
    }

    public void RegisterInstance(Type type, object instance)
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddSingleton(type, instance);
    }

    public void RegisterInstance<T>(T instance, [DisallowNull] string name) where T : class
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddKeyedSingleton<T>(name, instance);
    }

    public void RegisterInstance(Type type, object instance, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddKeyedSingleton(type, name, instance);
    }

    public void RegisterSingleton(Type from, Type to)
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        if (from != to)
        {
            _collection.AddSingleton(from, to);
        }
        else
        {
            _collection.AddSingleton(to);
        }
    }

    public void RegisterSingleton(Type from, Type to, string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddKeyedSingleton(from, name, to);
    }

    public void Register<TFrom, To>() where TFrom : class where To : class, TFrom
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }
        if (typeof(TFrom) != typeof(To))
        {
            _collection.AddTransient<TFrom, To>();
        }
        else
        {
            _collection.AddTransient<To>();
        }
    }

    public void Register<TFrom, To>([DisallowNull] string name) where TFrom : class where To : class, TFrom
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddKeyedTransient<TFrom, To>(name);
    }

    public void RegisterScoped(Type from, Type to)
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        if (from != to)
        {
            _collection.AddScoped(from, to);
        }
        else
        {
            _collection.AddScoped(to);
        }
    }
    public void RegisterScoped<TFrom, To>() where TFrom : class where To : class, TFrom
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        if (typeof(TFrom) != typeof(To))
        {
            _collection.AddScoped<TFrom, To>();
        }
        else
        {
            _collection.AddScoped<To>();
        }
    }

    public void RegisterScoped<TFrom, To>([DisallowNull] string name) where TFrom : class where To : class, TFrom
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddKeyedScoped<TFrom, To>(name);
    }

    public void RegisterScoped(Type from, Type to, [DisallowNull] string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }
        _collection.AddKeyedScoped(from, name, to);
    }

    /// <summary>
    /// If a signleton is registered with the same implementation, the same instance will be returned!
    /// </summary>
    /// <typeparam name="TFrom"></typeparam>
    /// <typeparam name="To"></typeparam>
    public void RegisterSingleton<TFrom, To>() where TFrom : class where To : class, TFrom
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        if (typeof(TFrom) != typeof(To)) // already added.
        {
            _collection.AddSingleton<TFrom, To>();
        }
        else
        {
            _collection.TryAddSingleton<To>();
        }
    }

    public void RegisterSingleton<TFrom, To>([DisallowNull] string name) where TFrom : class where To : class, TFrom
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddKeyedSingleton<TFrom, To>(name);
    }

    public T? Resolve<T>()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("No ServiceProvider exist.");
        }

        return ServiceProvider.GetService<T>();
    }

    public object? Resolve(Type type)
    {
        return ServiceProvider.GetService(type); // ?? throw new NullReferenceException($"No registration exitst for {type}");
    }

    public T? Resolve<T>(string name)
    {

        return ServiceProvider.GetKeyedService<T>(name); // ?? throw new NullReferenceException($"No registration exitst for {typeof(T)}");
    }

    public object? Resolve(Type type, string name)
    {
        if (ServiceProvider is IKeyedServiceProvider keyedServiceProvider)
        {
            return keyedServiceProvider.GetKeyedService(type, name) ?? throw new NullReferenceException($"No registration exitst for {type}");
        }

        throw new InvalidOperationException("Keyed service is not supported.");
    }

    public IEnumerable<T> ResolveAll<T>()
    {

        return _serviceProvider!.GetServices<T>();
    }

    public IEnumerable<object> ResolveAll(Type type)
    {

        return ServiceProvider!.GetServices(type).Where(o => o is not null).Cast<object>();
    }

    public IEnumerable<T> ResolveAll<T>(string name)
    {

        return ServiceProvider!.GetKeyedServices<T>(name);
    }

    public IEnumerable<object> ResolveAll(Type type, string name)
    {

        return ServiceProvider!.GetKeyedServices(type, name).Where(o => o is not null).Cast<object>();
    }

    public bool TryResolve<T>(out T? value)
    {
        try
        {
            value = ServiceProvider!.GetService<T>();
            return value is not null;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public bool TryResolve(Type type, out object? value)
    {
        try
        {
            value = ServiceProvider!.GetService(type);
            return value is not null;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public bool TryResolve<T>(string name, out T? value)
    {
        try
        {
            value = ServiceProvider!.GetKeyedService<T>(name);
            return value is not null;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public bool TryResolve(Type type, string name, out object? value)
    {
        try
        {
            value = ServiceProvider!.GetKeyedServices(type, name);
            return value is not null;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }

    public void RegisterFactory<T>(Func<T> exportedInstanceFactory) where T : class
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddTransient<T>(x => exportedInstanceFactory());
    }

    public void RegisterFactory(Type type, Func<object> exportedInstanceFactory)
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddTransient(type, x => exportedInstanceFactory());
    }

    public void RegisterSingletonFactory<T>(Func<T> exportedInstanceFactory) where T : class
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddSingleton<T>(x => exportedInstanceFactory());
    }

    public void RegisterSingletonFactory(Type type, Func<object> exportedInstanceFactory)
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddSingleton(type, x => exportedInstanceFactory());
    }

    public void RegisterScopedFactory<T>(Func<T> exportedInstanceFactory) where T : class
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddScoped<T>(x => exportedInstanceFactory());
    }
    public void RegisterScopedFactory(Type type, Func<object> exportedInstanceFactory)
    {
        if (_collection is null)
        {
            throw new InvalidOperationException("No ServiceCollection exists.");
        }

        _collection.AddScoped(type, x => exportedInstanceFactory());
    }

    public object? GetService(Type serviceType)
    {
        return Resolve(serviceType);
    }
}
#endif
