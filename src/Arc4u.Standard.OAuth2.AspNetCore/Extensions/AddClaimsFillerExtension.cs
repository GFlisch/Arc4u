using System;
using System.Linq;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class AddClaimsFillerExtension
{
    public static void AddClaimsFiller(this IServiceCollection services, Action<ClaimsFillerOptions> options)
    {
        var validate = new ClaimsFillerOptions();
        options(validate);

        if (validate.LoadClaimsFromClaimsFillerProvider && (null == validate.SettingsKeys || !validate.SettingsKeys.Any()))
        {
            throw new ConfigurationException("Settings key must be provided.");
        }

        services.Configure<ClaimsFillerOptions>(options);
    }

    public static void AddClaimsFiller(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:ClaimsMiddleWare:ClaimsFiller")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Get the default values.
        var options = new ClaimsFillerOptions();

        if (!string.IsNullOrWhiteSpace(sectionName))
        {
            var section = configuration.GetSection(sectionName);

            if (section is not null && section.Exists())
            {
                options = section.Get<ClaimsFillerOptions>();
            }
        }

       AddClaimsFiller(services, o =>
        {
            o.LoadClaimsFromClaimsFillerProvider = options.LoadClaimsFromClaimsFillerProvider;
            o.SettingsKeys = options.SettingsKeys;
        });
    }
}
