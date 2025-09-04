using Arc4u.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.OAuth2.TicketStore
{
    public static class CacheTicketStoreExtension
    {
        public static void AddCacheTicketStore(this IServiceCollection services, Action<CacheTicketStoreOptions> action)
        {
            var validate = new CacheTicketStoreOptions();
            new Action<CacheTicketStoreOptions>(action).Invoke(validate);

            ArgumentNullException.ThrowIfNull(validate.CacheName);
            ArgumentNullException.ThrowIfNull(validate.KeyPrefix);

            services.Configure<CacheTicketStoreOptions>(action);
            // if another implementation isalready registered, it will not be replaced.
            // so a custom implementation can be used.
            services.TryAddTransient<ITicketStore, CacheTicketStore>();
        }

        public static void AddCacheTicketStore(this IServiceCollection services, IConfiguration configuration, string sectionName = "AuthenticationCacheTicketStore")
        {
            var action = PrepareAction(configuration, sectionName);
            if (null == action)
            {
                throw new ConfigurationException("Ticket store cannot be created.");
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
            var option = new CacheTicketStoreOptions();

            if (section.Exists())
            {
                option = configuration.GetSection(sectionName).Get<CacheTicketStoreOptions>() ?? option;
            }

            void options(CacheTicketStoreOptions o)
            {
                o.CacheName = option.CacheName;
                o.KeyPrefix = option.KeyPrefix;
            }

            return options;
        }
    }
}

