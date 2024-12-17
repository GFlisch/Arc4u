using Arc4u.Configuration.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Arc4u.Configuration.Redis;
using Arc4u.Configuration.Sql;
using Arc4u.Configuration.Dapr;

namespace Arc4u.Caching;

public static class CacheContextServicesExtension
{
    public static void AddCacheContext(this IServiceCollection services, IConfiguration configuration, string sectionName = "Caching")
    {
        ArgumentNullException.ThrowIfNull(configuration);

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
                case "redis":
                    services.AddRedisCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
                case "sql":
                    services.AddSqlCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
                case "dapr":
                    services.AddDaprCache(cache.Name, configuration, BuildCacheSettingsSectionPath(idx, sectionName));
                    break;
            }
        }

    }

    private static string BuildCacheSettingsSectionPath(int idx, string rootSectionName)
    {
        return $"{rootSectionName}:Caches:{idx}:Settings";
    }

    public static ICacheContext? GetCacheContext(this IServiceProvider container)
    {
        return container.GetService<ICacheContext>();
    }
}
