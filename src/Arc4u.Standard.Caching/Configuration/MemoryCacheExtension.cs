using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Arc4u.Configuration.Memory;
public static class MemoryCacheExtension
{
    public static IServiceCollection AddMemoryCache(this IServiceCollection services, [DisallowNull] string name, Action<MemoryCacheOption> options)
    {
        var rawCacheOption = new MemoryCacheOption();
        new Action<MemoryCacheOption>(options).Invoke(rawCacheOption);
        var action = new Action<MemoryCacheOption>(o =>
        {
            o.CompactionPercentage = rawCacheOption.CompactionPercentage;
            o.SizeLimitInMB = rawCacheOption.SizeLimitInMB * 1024 * 1024;
            o.SerializerName = rawCacheOption.SerializerName;
        });

        ArgumentException.ThrowIfNullOrEmpty(name);

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
                o.SizeLimitInMB = option.SizeLimitInMB;
                o.CompactionPercentage = option.CompactionPercentage;
            }

            services.AddMemoryCache(name, options);
        }

        return services;
    }

}
