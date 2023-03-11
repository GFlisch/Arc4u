using Arc4u.Dependency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.Caching;

public static class CacheContextServicesExtension
{
    public static void AddCacheContext(this ServiceCollection services)
    {
        services.TryAddSingleton<ICacheContext, CacheContext>();
    }

    public static ICacheContext GetCacheContext(this IContainerResolve container)
    {
        return container.Resolve<ICacheContext>();
    }
}
