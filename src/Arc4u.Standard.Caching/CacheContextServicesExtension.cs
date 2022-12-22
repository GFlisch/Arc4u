using Arc4u.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Arc4u.Dependency
{
    public static class CacheContextServicesExtension
    {
        public static void AddCacheContext(this ServiceCollection services)
        {
            services.TryAddSingleton<CacheContext>();
        }

        public static CacheContext GetCacheContext(this IServiceProvider container)
        {
            return container.GetService<CacheContext>();
        }
    }
}
