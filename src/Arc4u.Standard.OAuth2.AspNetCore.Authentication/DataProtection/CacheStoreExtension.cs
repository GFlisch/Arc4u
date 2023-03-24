using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arc4u.OAuth2.DataProtection;
public static class CacheStoreExtension
{
    public static IDataProtectionBuilder PersistKeysToCache(this IDataProtectionBuilder builder, [DisallowNull] string cacheKey, string? cacheName)
    {
        ArgumentNullException.ThrowIfNull(cacheKey, nameof(cacheKey));

        builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
        {
            var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new CacheStore(services, loggerFactory, cacheKey, cacheName);
            });
        });

        return builder;
    }
}
