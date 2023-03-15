using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Arc4u.Caching.Memory;
public static class MemoryCacheExtension
{
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, [DisallowNull] string name, Action<MemoryCacheOption> options)
    {
        var rawCacheOption = new MemoryCacheOption();
        new Action<MemoryCacheOption>(options).Invoke(rawCacheOption);
        var action = new Action<MemoryCacheOption>(o =>
        {
            o.CompactionPercentage = rawCacheOption.CompactionPercentage;
            o.SizeLimit = rawCacheOption.SizeLimit * 1024 * 1024;
            o.SerializerName = rawCacheOption.SerializerName;
        });


#if NET6_0
        ArgumentNullException.ThrowIfNull(name, nameof(name));
#endif
#if NET7_0_OR_GREATER
        ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
#endif
        services.Configure<MemoryCacheOption>(name, action);

        return services;
    }

    public static IServiceCollection AddMemoryCache(this IServiceCollection services, [DisallowNull] string name, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName)
    {
        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<MemoryCacheOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            void options(MemoryCacheOption o)
            {
                o.SerializerName = option.SerializerName;
                o.SizeLimit = option.SizeLimit;
                o.CompactionPercentage = option.CompactionPercentage;
            }

            services.AddMemoryCache(name, options);
        }

        return services;
    }

}
