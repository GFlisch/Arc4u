using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration;
public static class ConfigurationSettingsExtension
{
    public static void ConfigureSettings(this IServiceCollection services, string name, IConfiguration configuration, string sectionName)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var section = configuration.GetSection(sectionName);

        if (!section.Exists())
        {
            throw new ConfigurationException($"No section with name {sectionName} exists.");
        }

        var dic = section.Get<Dictionary<string, string>>() ?? throw new ConfigurationException($"Section {sectionName} is not a Dictionary<string,string>.");

        void options(Dictionary<string, string> o)
        {
            foreach (var kv in dic!)
            {
                o.Add(kv.Key, kv.Value);
            }
        }

        services.Configure<Dictionary<string, string>>("OAuth2", options);

    }
}
