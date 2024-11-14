#if NET8_0_OR_GREATER

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration.Redis;
public static class RedisCacheExtension
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, [DisallowNull] string name, Action<RedisCacheOption> options)
    {
        var validate = new RedisCacheOption();
        new Action<RedisCacheOption>(options).Invoke(validate);

        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentException.ThrowIfNullOrEmpty(validate.ConnectionString, nameof(validate.ConnectionString));
        ArgumentException.ThrowIfNullOrEmpty(validate.InstanceName, nameof(validate.InstanceName));

        services.Configure<RedisCacheOption>(name, options);

        return services;
    }

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
