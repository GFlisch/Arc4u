using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arc4u.OAuth2.TicketStore
{
    public static class FileTicketStoreExtension
    {
        public static void AddFileTicketStore(this IServiceCollection services, Action<FileTicketStoreOptions> action)
        {

            var validate = new FileTicketStoreOptions();
            new Action<FileTicketStoreOptions>(action).Invoke(validate);

            ArgumentNullException.ThrowIfNull(validate.StorePath);

            if (!validate.StorePath.Exists)
            {
                // will throw an exception if this is not possible!
                validate.StorePath.Create();
            }

            services.Configure<FileTicketStoreOptions>(action);
            services.TryAddTransient<ITicketStore, FileTicketStore>();
        }

        public static void AddFileTicketStore(this IServiceCollection services, IConfiguration configuration, string sectionName = "AuthenticationFileTicketStore")
        {
            var section = configuration.GetSection(sectionName) as IConfigurationSection;

            if (section.Exists())
            {
                var option = configuration.GetSection(sectionName).Get<FileTicketStoreOptions>();

                if (option is null)
                {
                    throw new NullReferenceException(nameof(option));
                }

                void options(FileTicketStoreOptions o)
                {
                    o.StorePath = option.StorePath;
                }

                AddFileTicketStore(services, options);
            }
        }
    }
}

