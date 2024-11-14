using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace Arc4u.Configuration.Memory;
public static class MemoryCacheExtension
{
#if NET8_0_OR_GREATER
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, [DisallowNull] string name, Action<MemoryCacheOption> options)
#else
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, string name, Action<MemoryCacheOption> options)
#endif
    {
        var rawCacheOption = new MemoryCacheOption();
        new Action<MemoryCacheOption>(options).Invoke(rawCacheOption);
        var action = new Action<MemoryCacheOption>(o =>
        {
            o.CompactionPercentage = rawCacheOption.CompactionPercentage;
            o.SizeLimitInMB = rawCacheOption.SizeLimitInMB * 1024 * 1024;
            o.SerializerName = rawCacheOption.SerializerName;
        });

#if NET7_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(name);
#else
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("The value cannot be an empty string.", nameof(name));
        }
#endif

        services.Configure<MemoryCacheOption>(name, action);

        return services;
    }

#if NET8_0_OR_GREATER
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, [DisallowNull] string name, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName)
#else
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, string name, IConfiguration configuration, string sectionName)
#endif

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
                o.SizeLimitInMB = option.SizeLimitInMB;
                o.CompactionPercentage = option.CompactionPercentage;
            }

            services.AddMemoryCache(name, options);
        }

        return services;
    }

}
