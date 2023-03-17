using Arc4u.Configuration.Memory;
using Arc4u.Configuration.Redis;
using Arc4u.Configuration.Sql;
using Arc4u.Dependency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.Caching;

public static class CacheContextServicesExtension
{
    public static void AddCacheContext(this IServiceCollection services, IConfiguration configuration ,string sectionName = "Caching")
    {
        services.TryAddSingleton<ICacheContext, CacheContext>();

        var config = new Configuration.Caching();
        configuration.Bind("Caching", config);

        for (var idx = 0; idx < config.Caches.Count; idx++)
        {
            var cache = config.Caches[idx];

            switch (cache.Kind.ToLowerInvariant())
            {
                case "memory":
                    services.AddMemoryCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
#if NET6_0_OR_GREATER
                case "redis":
                    services.AddRedisCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
                case "sql":
                    services.AddSqlCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
#endif
            }
        }

    }

    private static string BuildCacheSettingsSectionPath(int idx, string rootSectionName)
    {
        return $"{rootSectionName}:Caches:{idx}:Settings";
    }

    public static ICacheContext GetCacheContext(this IContainerResolve container)
    {
        return container.Resolve<ICacheContext>();
    }
}
