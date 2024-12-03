using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Configuration;

public static class ConfigurationHelper
{
    /// <summary>
    /// Load json files for the configuration and returns a IConfiguration so we can extract the
    /// settings.
    /// More than one file is possible.
    /// </summary>
    /// <param name="configFiles"></param>
    /// <returns></returns>
    public static IConfiguration GetConfigurationFromFile(params string[] configFiles)
    {
        var configBuilder = new ConfigurationBuilder();
        foreach (var file in configFiles)
        {
            configBuilder.AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file));
        }
        return configBuilder.Build();
    }

    public static IConfiguration GetConfigurationFromResourceStream(params string[] fileTypes)
    {
        var configBuilder = new ConfigurationBuilder();

        var disposables = new List<IDisposable>();
        var assemblies = new Dictionary<string, Assembly>();

        foreach (var file in fileTypes)
        {
            var fileInfo = file.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (fileInfo.Length != 2)
            {
                throw new ApplicationException($"File info receives is badly formatted: '{file}'. We expect: 'Assembly, full namespace file'.");
            }
            var assemblyName = fileInfo[0].Trim();

            if (!assemblies.ContainsKey(assemblyName))
            {
                var a = Assembly.Load(assemblyName);
                if (null != a)
                {
                    assemblies.Add(assemblyName, a);
                }
            }

            var stream = assemblies[assemblyName].GetManifestResourceStream(fileInfo[1].Trim());
            if (null != stream)
            {
                disposables.Add(stream);
                configBuilder.AddJsonStream(stream);
            }
        }

        var configuration = configBuilder.Build();

        foreach (var disp in disposables)
        {
            disp.Dispose();
        }

        return configuration;
    }

    public static void AddApplicationConfig(this IServiceCollection services, Action<ApplicationConfig> option)
    {
        var validate = new ApplicationConfig();
        option(validate);

        // try to detect as many configuration errors as possible instead of stopping at the first misconfigured property.
        // since the happy flow is the norm, it's OK to use just a string to concatenate messages instead of a full-blown list or StringBuilder.

        string? configErrors = null;
        if (string.IsNullOrEmpty(validate.ApplicationName))
        {
            configErrors += "Application name is not defined in the intialization of the application config settings" + System.Environment.NewLine;
        }

        if (string.IsNullOrEmpty(validate.Environment.Name))
        {
            configErrors += "Application environment name is not defined in the intialization of the application config settings" + System.Environment.NewLine;
        }

        if (string.IsNullOrEmpty(validate.Environment.LoggingName))
        {
            configErrors += "Application environment logging name is not defined in the intialization of the application config settings" + System.Environment.NewLine;
        }

        if (string.IsNullOrEmpty(validate.Environment.TimeZone))
        {
            configErrors += "Application environment time zone is not defined in the intialization of the application config settings" + System.Environment.NewLine;
        }

        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        services.Configure<ApplicationConfig>(option);
    }

    public static void AddApplicationConfig(this IServiceCollection services, IConfiguration configuration, string sectionName = "Application.Configuration")
    {

        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName);

        var section = configuration.GetSection(sectionName) ?? throw new NullReferenceException($"No section exists with name {sectionName}");
        var config = section.Get<ApplicationConfig>() ?? throw new NullReferenceException($"section exists with name {sectionName} but it is not an ApplicationConfig object.");

        void OptionFiller(ApplicationConfig option)
        {
            ArgumentNullException.ThrowIfNull(option);

            option.ApplicationName = config.ApplicationName;
            option.Environment.Name = config.Environment.Name;
            option.Environment.LoggingName = config.Environment.LoggingName;
            option.Environment.TimeZone = config.Environment.TimeZone;
        }

        AddApplicationConfig(services, OptionFiller);
    }
}
