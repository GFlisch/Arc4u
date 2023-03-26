using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        var assemblies = new Dictionary<String, Assembly>();

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
                    assemblies.Add(assemblyName, a);
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
            disp.Dispose();

        return configuration;
    }

    public static void AddApplicationConfig(this IServiceCollection services, Action<ApplicationConfig> option)
    {
        if (option is null)
        {
            throw new ArgumentNullException(nameof(option));
        }

        var validate = new ApplicationConfig();
        option(validate);

        if (string.IsNullOrEmpty(validate.ApplicationName))
        {
            throw new MissingFieldException("Application name is not defined in the intialization of the application config settings");
        }

        if (string.IsNullOrEmpty(validate.Environment.Name))
        {
            throw new MissingFieldException("Application environment name is not defined in the intialization of the application config settings");
        }

        if (string.IsNullOrEmpty(validate.Environment.LoggingName))
        {
            throw new MissingFieldException("Application environment logging name is not defined in the intialization of the application config settings");
        }

        if (string.IsNullOrEmpty(validate.Environment.TimeZone))
        {
            throw new MissingFieldException("Application environment time zone is not defined in the intialization of the application config settings");
        }

        services.Configure<ApplicationConfig>(option);
    }

    public static void AddApplicationConfig(this IServiceCollection services, IConfiguration configuration, string sectionName = "Application.Configuration")
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof (sectionName));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var section = configuration.GetSection(sectionName);

        if (section is null)
        {
            throw new NullReferenceException($"No section exists with name {sectionName}");
        }

        var config = section.Get<ApplicationConfig>();

        void OptionFiller(ApplicationConfig option)
        {
            option.ApplicationName = config.ApplicationName;
            option.Environment.Name = config.Environment.Name;
            option.Environment.LoggingName = config.Environment.LoggingName;
            option.Environment.TimeZone = config.Environment.TimeZone;
        }

        AddApplicationConfig(services, OptionFiller);
    }

}
