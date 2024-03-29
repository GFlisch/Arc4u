using System;
using System.Collections.Generic;
using Arc4u.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class DomainMappingExtensions
{
    public static void AddDomainMapping(this IServiceCollection services, Action<SimpleKeyValueSettings> options, string sectionKey = "DomainMapping")
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (string.IsNullOrWhiteSpace(sectionKey))
        {
            throw new ArgumentNullException(nameof(sectionKey));
        }

        services.Configure<SimpleKeyValueSettings>(sectionKey, options);
    }

    public static void AddDomainMapping(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:DomainsMapping", string sectionKey = "DomainMapping")
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }
        if (string.IsNullOrWhiteSpace(sectionKey))
        {
            throw new ArgumentNullException(nameof(sectionKey));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var section = configuration.GetSection(sectionName);

        var settings = (section is null || !section.Exists()) ? new Dictionary<string, string>() : section.Get<Dictionary<string, string>>();

        settings ??= new Dictionary<string, string>();

        AddDomainMapping(services, options => {
            foreach (var key in settings.Keys )
            {
                options.Add(key, settings[key]);
            }
        }, sectionKey);
    }
}
