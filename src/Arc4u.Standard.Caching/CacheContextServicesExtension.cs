using Arc4u.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.Dependency
{
    public static class CacheContextServicesExtension
    {
        public static void AddCacheContext(this ServiceCollection services)
        {
            services.TryAddSingleton<CacheContext>();
        }

        public static CacheContext GetCacheContext(this IContainerResolve container)
        {
            return container.Resolve<CacheContext>();
        }
    }
}
