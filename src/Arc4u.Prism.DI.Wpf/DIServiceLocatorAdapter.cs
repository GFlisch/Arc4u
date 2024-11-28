using Arc4u.Dependency;
using CommonServiceLocator;

namespace Prism.DI;

/// <summary>
/// Defines a <see cref="DIModelContainer"/> adapter for the <see cref="IServiceLocator"/> interface to be used by the Prism Library.
/// </summary>
public class DIServiceLocatorAdapter : ServiceLocatorImplBase
{
    private readonly IContainerResolve Container;
    /// <summary>Exposes underlying Container for direct operation.</summary>
    /// <summary>Creates new locator as adapter for provided container.</summary>
    /// <param name="container">Container to use/adapt.</param>
    public DIServiceLocatorAdapter(IContainerResolve container)
    {
        Container = container;
    }

    /// <summary>Resolves service from container. Throws if unable to resolve.</summary>
    /// <param name="serviceType">Service type to resolve.</param>
    /// <param name="key">(optional) Service key to resolve.</param>
    /// <returns>Resolved service object.</returns>
    protected override object DoGetInstance(Type serviceType, string key)
    {
        if (serviceType == null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        var result = string.IsNullOrWhiteSpace(key) ? Container.Resolve(serviceType) : Container.Resolve(serviceType, key);
        return result;
    }

    /// <summary>Returns enumerable which when enumerated! resolves all default and named 
    /// implementations/registrations of requested service type. 
    /// If no services resolved when enumerable accessed, no exception is thrown - enumerable is empty.</summary>
    /// <param name="serviceType">Service type to resolve.</param>
    /// <returns>Returns enumerable which will return resolved service objects.</returns>
    protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
    {
        if (serviceType == null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        return Container.ResolveAll(serviceType);
    }
}