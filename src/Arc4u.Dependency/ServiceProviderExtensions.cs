using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Dependency;
public static class ServiceProviderExtensions
{
    public static bool TryGetService<T>(this IServiceProvider provider, out T? service)
    {
        try
        {
            service = provider.GetService<T>();
        }
        catch (Exception)
        {
            service = default;
        }
        return null != service;
    }

    public static bool TryGetService(this IServiceProvider provider, Type type, string name, out object? value)
    {
        try
        {
            value = provider.GetRequiredKeyedService(type, name);
            return value is not null;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }

    public static bool TryGetService<T>(this IServiceProvider provider, string name, out T? service)
    {
        try
        {
            service = provider.GetKeyedService<T>(name);

        }
        catch (Exception)
        {
            service = default;
        }
        return null != service;
    }

}
