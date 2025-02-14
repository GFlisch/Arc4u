using Arc4u.Configuration.Memory;
using Arc4u.Dependency;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#if NET8_0_OR_GREATER
using Arc4u.Configuration.Redis;
using Arc4u.Configuration.Sql;
using Arc4u.Configuration.Dapr;
#endif

namespace Arc4u.Caching;

public static class CacheContextServicesExtension
{
    public static void AddCacheContext(this IServiceCollection services, IConfiguration configuration, string sectionName = "Caching")
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configuration);
#endif
#if NETSTANDARD2_0_OR_GREATER
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
#endif
        var section = configuration.GetSection(sectionName);

        if (!section.Exists())
        {
            throw new InvalidOperationException($"Section {sectionName} in the configuration providers doesn't exists!");
        }

        var config = section.Get<Configuration.Caching>();
        if (config == null)
        {
            throw new InvalidOperationException("Configuration for caching is missing.");
        }

        services.TryAddSingleton<ICacheContext, CacheContext>();

        for (var idx = 0; idx < config.Caches.Count; idx++)
        {
            var cache = config.Caches[idx];

            switch (cache.Kind.ToLowerInvariant())
            {
                case "memory":
                    services.AddMemoryCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
#if NET8_0_OR_GREATER
                case "redis":
                    services.AddRedisCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
                case "sql":
                    services.AddSqlCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
                case "dapr":
                    services.AddDaprCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
#endif
            }
        }

    }

    private static string BuildCacheSettingsSectionPath(int idx, string rootSectionName)
    {
        return $"{rootSectionName}:Caches:{idx}:Settings";
    }

    public static ICacheContext? GetCacheContext(this IContainerResolve container)
    {
        return container.Resolve<ICacheContext>();
    }
}
