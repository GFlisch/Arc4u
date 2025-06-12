using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class AddClaimsFillerExtension
{
    public static readonly List<string> DefaultClaimsToExclude = [ "aud", "iss", "iat", "nbf", "acr", "aio", "appidacr", "ipaddr", "scp", "sub", "tid", "uti", "unique_name", "apptype", "appid", "ver", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant", "http://schemas.microsoft.com/identity/claims/scope" ];
    public static void AddClaimsFiller(this IServiceCollection services, Action<ClaimsFillerOptions> options)
    {
        var validate = new ClaimsFillerOptions();
        options(validate);

        if (validate.LoadClaimsFromClaimsFillerProvider && (null == validate.SettingsKeys || !validate.SettingsKeys.Any()))
        {
            throw new ConfigurationException("Settings key must be provided.");
        }

        if (string.IsNullOrWhiteSpace(validate.ExpireClaim))
        {
            throw new ConfigurationException("Expire claim must be provided, usually 'exp'.");
        }

        if (validate.ClaimsToExclude.Any(k => k.Equals(validate.ExpireClaim, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConfigurationException($"The claim use to define when the validity period is expired: {validate.ExpireClaim}, cannot be excluded.");
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

            if (section.Exists())
            {
                section.Bind(options);
                if (!section.GetSection("ClaimsToExclude").Exists())
                {
                    options!.ClaimsToExclude = DefaultClaimsToExclude;
                }
            }
        }

        AddClaimsFiller(services, o =>
        {
            o.LoadClaimsFromClaimsFillerProvider = options.LoadClaimsFromClaimsFillerProvider;
            o.SettingsKeys = options.SettingsKeys.Any() ? options.SettingsKeys :  [ Constants.OpenIdOptionsName ];
            o.ClaimsToExclude = options.ClaimsToExclude;
            o.ExpireClaim = options.ExpireClaim;
        });
    }
}
