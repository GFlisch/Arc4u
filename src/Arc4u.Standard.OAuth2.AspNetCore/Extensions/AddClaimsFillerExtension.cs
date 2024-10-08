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

        if (validate.LoadClaimsFromClaimsFillerProvider)
        {
            string? configErrors = null;
            if (null == validate.SettingsKeys || !validate.SettingsKeys.Any())
            {
                configErrors += "Settings key must be provided." + System.Environment.NewLine;
            }
            if (validate.LoadClaimsFromClaimsFillerProvider && validate.MaxTime == TimeSpan.Zero)
            {
                configErrors += $"If {nameof(validate.LoadClaimsFromClaimsFillerProvider)} is true, then {nameof(validate.MaxTime)} must be provided." + System.Environment.NewLine;
            }
            if (configErrors is not null)
            {
                throw new ConfigurationException(configErrors);
            }
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
