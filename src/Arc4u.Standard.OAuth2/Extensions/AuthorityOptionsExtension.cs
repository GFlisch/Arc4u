using System;
using Arc4u.Configuration;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class AuthorityOptionsExtension
{
    public static void AddDefaultAuthority(this IServiceCollection services, Action<AuthorityOptions> options)
    {

        services.AddAuthority(options, "Default");
    }

    public static void AddDefaultAuthority(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:DefaultAuthority")
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(services);
#endif
#if NETSTANDARD2_0
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
#endif

        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(sectionName);
        }

        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            throw new ConfigurationException($"Section {sectionName} doesn't exist");
        }

        var option = section.Get<AuthorityOptions>();

        if (option is null)
        {
            throw new ConfigurationException($"Section {sectionName} doesn't correspond to the expected format.");
        }

        services.AddDefaultAuthority(options =>
        {
            options.SetData(option.Url, option.TokenEndpoint, option.MetaDataAddress);
        });
    }

    public static void AddAuthority(this IServiceCollection services, Action<AuthorityOptions> options, string optionKey)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

#endif
#if NETSTANDARD2_0
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
#endif
        if (string.IsNullOrWhiteSpace(optionKey))
        {
            throw new ArgumentNullException(optionKey);
        }

        // validation.
        var extract = new AuthorityOptions();
        options(extract);

        if (extract.Url is null)
        {
            throw new ConfigurationException("Url authority field is mandatory.");
        }

        // v1.0 is not mandatory and should disappear.

        services.Configure(optionKey, options);
    }

}
