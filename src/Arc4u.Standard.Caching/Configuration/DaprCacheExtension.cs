using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Arc4u.Configuration.Dapr;

#if NET6_0_OR_GREATER
public static class DaprCacheExtension
{
    public static IServiceCollection AddDaprCache(this IServiceCollection services, [DisallowNull] string name, Action<DaprCacheOption> options)
    {
        var rawCacheOption = new DaprCacheOption();
        new Action<DaprCacheOption>(options).Invoke(rawCacheOption);
        var action = new Action<DaprCacheOption>(o =>
        {
            o.Name = rawCacheOption.Name;
        });

#if NET7_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(name);
#else
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("The value cannot be an empty string.", nameof(name));
        }
#endif

        services.Configure<DaprCacheOption>(name, action);

        return services;
    }

    public static IServiceCollection AddDaprCache(this IServiceCollection services, [DisallowNull] string name, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName)
    {
        var section = configuration.GetSection(sectionName);

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<DaprCacheOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            void options(DaprCacheOption o)
            {
                o.Name = option.Name;
            }

            services.AddDaprCache(name, options);
        }

        return services;
    }

}

#endif
