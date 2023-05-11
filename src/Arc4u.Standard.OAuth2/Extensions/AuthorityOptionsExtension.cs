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


    public static void AddAuthority(this IServiceCollection services, Action<AuthorityOptions> options, string optionKey)
    {
#if NET6_0_OR_GREATER
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

        if (string.IsNullOrWhiteSpace(extract.Url))
        {
            throw new ConfigurationException("Url authority field is mandatory.");
        }

        if (string.IsNullOrWhiteSpace(extract.TokenEndpoint))
        {
            throw new ConfigurationException("TokenEndpoint field is mandatory.");
        }

        // v1.0 is not mandatory and should disappear.

        services.Configure(optionKey, options);
    }

    public static void AddDefaultAuthority(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:DefaultAuthority")
    {
#if NET6_0_OR_GREATER
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
            options.Url = option.Url;
            options.TokenEndpoint = option.TokenEndpoint;
        });
    }
}
