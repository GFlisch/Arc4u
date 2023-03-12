using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.TicketStore;

public static class FileTicketStoreExtension
{
    public static void AddFileTicketStore(this IServiceCollection services, Action<FileTicketStoreOption> action)
    {

        var validate = new FileTicketStoreOption();
        new Action<FileTicketStoreOption>(action).Invoke(validate);

        ArgumentNullException.ThrowIfNull(validate.StorePath);
        ArgumentNullException.ThrowIfNull(validate.TicketStore);

        var type = Type.GetType(validate.TicketStore, true);

        if (!validate.StorePath.Exists)
        {
            // will throw an exception if this is not possible!
            validate.StorePath.Create();
        }

        services.Configure<FileTicketStoreOption>(action);
        services.AddTransient(typeof(ITicketStore), type);
    }

    public static void AddFileTicketStore(this IServiceCollection services, IConfiguration configuration, string sectionName = "AuthenticationFileTicketStore")
    {
        var section = configuration.GetSection(sectionName) as IConfigurationSection;

        if (section.Exists())
        {
            var option = configuration.GetSection(sectionName).Get<FileTicketStoreOption>();

            if (option is null)
            {
                throw new NullReferenceException(nameof(option));
            }

            void options(FileTicketStoreOption o)
            {
                o.StorePath = option.StorePath;
                o.TicketStore = option.TicketStore;
            }

            AddFileTicketStore(services, options);
        }
    }
}

