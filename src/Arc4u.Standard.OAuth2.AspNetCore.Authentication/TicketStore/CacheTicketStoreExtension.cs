using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.TicketStore;

public static class CacheTicketStoreExtension
{
    public static void AddCacheTicketStore(this IServiceCollection services, Action<CacheTicketStoreOptions> action)
    {
        var validate = new CacheTicketStoreOptions();
        new Action<CacheTicketStoreOptions>(action).Invoke(validate);

        ArgumentNullException.ThrowIfNull(validate.CacheName);
        ArgumentNullException.ThrowIfNull(validate.KeyPrefix);
        ArgumentNullException.ThrowIfNull(validate.TicketStore);

        var type = Type.GetType(validate.TicketStore, true);

        services.Configure<CacheTicketStoreOptions>(action);
        services.AddTransient(typeof(ITicketStore), type!);
    }

    public static void AddCacheTicketStore(this IServiceCollection services, IConfiguration configuration, string sectionName = "AuthenticationCacheTicketStore")
    {
        var action = PrepareAction(configuration, sectionName);
        if (null == action)
        {
            throw new InvalidOperationException("Ticket store cannot be created.");
        }

        AddCacheTicketStore(services, action);
    }

    internal static Action<CacheTicketStoreOptions>? PrepareAction(IConfiguration configuration, string? sectionName)
    {
        if (sectionName is null)
        {
            return null;
        }

        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<CacheTicketStoreOptions>();

            if (option is null)
            {
                throw new InvalidOperationException($"Section nameof(option) cannot be deserialize to CacheTicketStoreOptions");
            }

            void options(CacheTicketStoreOptions o)
            {
                o.CacheName = option.CacheName;
                o.KeyPrefix = option.KeyPrefix;
                o.TicketStore = option.TicketStore;
            }

            return options;
        }

        return null;
    }

}

