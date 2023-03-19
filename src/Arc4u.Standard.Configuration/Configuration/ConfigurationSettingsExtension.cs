using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration;

// Extension class to register a key/value with IOption<T>...
public static class ConfigurationSettingsExtension
{
    /// <summary>
    /// Register a key/value collection with IOption model.
    /// <param name="services">The service collection <see cref="IServiceCollection"/></param>
    /// <param name="name">The name to get the back the DIctionary.</param>
    /// <param name="configuration"><see cref="IConfiguration"/> to read the settings.</param>
    /// <param name="sectionName">The section name containing the keys and values.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ConfigurationException"></exception>
    public static IKeyValueSettings ConfigureSettings(this IServiceCollection services, string name, IConfiguration configuration, string sectionName)
    {
        // Check if the `name` parameter is null or empty.
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        // Check if the `sectionName` parameter is null or empty.
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }

        // Check if the `configuration` parameter is null.
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        // Get the specified `sectionName` from the `configuration` parameter.
        var section = configuration.GetSection(sectionName);

        // Check if the `section` exists in the `configuration`.
        if (!section.Exists())
        {
            // Throw a `ConfigurationException` if the `section` doesn't exist.
            throw new ConfigurationException($"No section with name {sectionName} exists.");
        }

        // Get a `Dictionary<string, string>` from the `section`.
        var dic = section.Get<Dictionary<string, string>>() ?? throw new ConfigurationException($"Section {sectionName} is not a Dictionary<string,string>.");

        // Define a local method `options` that takes a `Dictionary<string, string>` parameter `o` and adds each key-value pair
        // from the retrieved dictionary `dic` to it.
        void options(SimpleKeyValueSettings o)
        {
            foreach (var kv in dic!)
            {
                o.Add(kv.Key, kv.Value);
            }
        }

        // Configure the specified `services` parameter with the `OAuth2` section and the `options` method.
        services.Configure<SimpleKeyValueSettings>(name, options);

        var result = new SimpleKeyValueSettings();
        options(result);

        return result;
    }
}

