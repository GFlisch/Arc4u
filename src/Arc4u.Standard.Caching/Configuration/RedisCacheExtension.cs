#if NET6_0_OR_GREATER

using System;
using System.Diagnostics.CodeAnalysis;
#if !NET7_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration.Redis;
public static class RedisCacheExtension
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, [DisallowNull] string name, Action<RedisCacheOption> options)
    {
        var validate = new RedisCacheOption();
        new Action<RedisCacheOption>(options).Invoke(validate);

#if NET7_0_OR_GREATER
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentException.ThrowIfNullOrEmpty(validate.ConnectionString, nameof(validate.ConnectionString));
        ArgumentException.ThrowIfNullOrEmpty(validate.InstanceName, nameof(validate.InstanceName));
#else
        ThrowIfNullOrEmpty(name, nameof(name));
        ThrowIfNullOrEmpty(validate.ConnectionString, nameof(validate.ConnectionString));
        ThrowIfNullOrEmpty(validate.InstanceName, nameof(validate.InstanceName));
#endif

        services.Configure<RedisCacheOption>(name, options);

        return services;
    }

#if !NET7_0_OR_GREATER
    private static void ThrowIfNullOrEmpty(string? s, [CallerArgumentExpression("s")] string? conditionExpression = null)
    {
        if (string.IsNullOrEmpty(s))
        {
            throw new ArgumentException("The value cannot be an empty string.", conditionExpression);
        }
    }
#endif

    public static IServiceCollection AddRedisCache(this IServiceCollection services, [DisallowNull] string name, [DisallowNull] IConfiguration configuration, [DisallowNull] string sectionName)
    {
        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<RedisCacheOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            void options(RedisCacheOption o)
            {
                o.SerializerName = option.SerializerName;
                o.ConnectionString = option.ConnectionString;
                o.InstanceName = option.InstanceName;
            }

            services.AddRedisCache(name, options);
        }

        return services;
    }

}
#endif
