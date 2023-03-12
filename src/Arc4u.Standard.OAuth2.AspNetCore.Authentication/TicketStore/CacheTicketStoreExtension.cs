using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Arc4u.OAuth2.TicketStore;

public static class CacheTicketStoreExtension
{
    public static void AddCacheTicketStore(this IServiceCollection services, Action<CacheTicketStoreOption> action)
    {
        var validate = new CacheTicketStoreOption();
        new Action<CacheTicketStoreOption>(action).Invoke(validate);

        ArgumentNullException.ThrowIfNull(validate.CacheName);
        ArgumentNullException.ThrowIfNull(validate.KeyPrefix);
        ArgumentNullException.ThrowIfNull(validate.TicketStore);

        var type = Type.GetType(validate.TicketStore, true);

        services.Configure<CacheTicketStoreOption>(action);
        services.AddTransient(typeof(ITicketStore), type!);
    }

    public static void AddCacheTicketStore(this IServiceCollection services, IConfiguration configuration, string sectionName = "AuthenticationCacheTicketStore")
    {
        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<CacheTicketStoreOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            void options(CacheTicketStoreOption o)
            {
                o.CacheName = option.CacheName;
                o.KeyPrefix = option.KeyPrefix;
                o.TicketStore = option.TicketStore;
            }

            AddCacheTicketStore(services, options);

        }
    }
}

