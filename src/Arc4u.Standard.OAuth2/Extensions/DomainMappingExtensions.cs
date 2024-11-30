using Arc4u.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Extensions;
public static class DomainMappingExtensions
{
    public static void AddDomainMapping(this IServiceCollection services, Action<SimpleKeyValueSettings> options, string sectionKey = "DomainMapping")
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionKey);
#else
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrWhiteSpace(sectionKey))
        {
            throw new ArgumentNullException(nameof(sectionKey));
        }
#endif
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
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configuration);
#else
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
#endif
        var section = configuration.GetSection(sectionName);

        var settings = (section is null || !section.Exists()) ? new Dictionary<string, string>() : section.Get<Dictionary<string, string>>();

        settings ??= new Dictionary<string, string>();

        AddDomainMapping(services, options =>
        {
            foreach (var key in settings.Keys)
            {
                options.Add(key, settings[key]);
            }
        }, sectionKey);
    }
}
