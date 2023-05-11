using Arc4u.Configuration;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Token;
using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Arc4u.Standard.OAuth2.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Arc4u.Standard.OAuth2.Middleware;

public static class BasicAuthenticationMiddlewareExtension
{
    public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<BasicAuthenticationMiddleware>(app.ApplicationServices);
    }

    public static void AddBasicAuthenticationSettings(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:Basic", IX509CertificateLoader? certificateLoader = null)
    {
        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            throw new ConfigurationException($"No section exists with name {sectionName} in the configuration providers for Basic authentication.");
        }

        var settings = section.Get<BasicAuthenticationConfigurationSectionOptions>() ?? throw new NullReferenceException($"No section exists with name {sectionName} in the configuration providers for Basic authentication.");

        string? configErrors = null;
        if (string.IsNullOrWhiteSpace(settings.BasicSettingsPath))
        {
            configErrors += "BasicSettingsPath must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(settings.CertificateHeaderPath))
        {
            configErrors += "CertificateHeaderPath must be filled!" + System.Environment.NewLine;
        }

        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        certificateLoader ??= new X509CertificateLoader(null);

        AddBasicAuthenticationSettings(services, options =>
        {
            options.DefaultUpn = settings!.DefaultUpn;
            options.BasicOptions = PrepareBasicAction(configuration, settings.BasicSettingsPath);
            options.CertificateHeaderOptions = PrepareCertificatesAction(configuration, settings.CertificateHeaderPath, certificateLoader);
        });

    }

    public static void AddBasicAuthenticationSettings(this IServiceCollection services, Action<BasicAuthenticationConfigurationOptions> options)
    {
        var basicOptions = new BasicAuthenticationConfigurationOptions();
        options(basicOptions);

        if (!string.IsNullOrEmpty(basicOptions.DefaultUpn))
        {
            if (!Regex.IsMatch(basicOptions.DefaultUpn, @"^@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))"))
            {
                throw new ConfigurationException("Bad upn format, we expect a @ and one point.");
            }
        }

        var certs = new Dictionary<string, X509Certificate2>();
        basicOptions.CertificateHeaderOptions(certs);

        foreach (var cert in certs)
        {
            if (string.IsNullOrWhiteSpace(cert.Key))
            {
                throw new ConfigurationException("Header key cannot be empty!");
            }

            if (cert.Value is null)
            {
                throw new ConfigurationException($"Certificate for key {cert.Key} is null.");
            }
        }

        services.Configure<BasicAuthenticationSettingsOptions>(basicSettings =>
        {
            basicSettings.BasicSettings = basicOptions.BasicOptions.BuildBasics();
            basicSettings.DefaultUpn = basicOptions.DefaultUpn;
            basicSettings.CertificateHeaderOptions = certs;
        });

        RegisterBasicAuthority(services, basicOptions.BasicOptions);
    }

    public static void RegisterBasicAuthority(IServiceCollection services, Action<BasicSettingsOptions> options)
    {
        var validate = new BasicSettingsOptions();
        options(validate);

        if (validate.Authority is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(validate.Authority.Url))
        {
            return;
        }

        services.AddAuthority(authOptions =>
        {
            authOptions.Url = validate.Authority.Url;
            authOptions.TokenEndpoint = validate.Authority.TokenEndpoint;
        }, "Basic");

    }

    public static SimpleKeyValueSettings BuildBasics(this Action<BasicSettingsOptions> options)
    {
        var validate = new BasicSettingsOptions();
        options(validate);
        var configErrors = string.Empty;

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            configErrors += "ProviderId field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.ClientId))
        {
            configErrors += "ClientId field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.AuthenticationType))
        {
            configErrors += "AuthenticationType field is not defined." + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(validate.Scope))
        {
            configErrors += "Scope field is not defined." + System.Environment.NewLine;
        }

        if (!string.IsNullOrWhiteSpace(configErrors))
        {
            throw new ConfigurationException(configErrors);
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on this.

        var settings = new SimpleKeyValueSettings();

        settings.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
        settings.Add(TokenKeys.AuthenticationTypeKey, validate.AuthenticationType);
        settings.Add(TokenKeys.ClientIdKey, validate.ClientId);
        settings.Add(TokenKeys.Scope, validate.Scope);

        return settings;
    }
    private static Action<Dictionary<string, X509Certificate2>> PrepareCertificatesAction(IConfiguration configuration, [DisallowNull] string sectionName, [DisallowNull] IX509CertificateLoader certificateLoader)
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
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

        var settings = section.Get<Dictionary<string, CertificateStoreOrFileInfo>>() ?? new Dictionary<string, CertificateStoreOrFileInfo>();

        void CertificateFiller(Dictionary<string, X509Certificate2> option)
        {
            foreach (var setting in settings)
            {
                // must add a functionality to load it from a File or Store
                option.Add(setting.Key, certificateLoader.FindCertificate(setting.Value));
            }
        }

        return CertificateFiller;
    }
    private static Action<BasicSettingsOptions> PrepareBasicAction(IConfiguration configuration, [DisallowNull] string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
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

        var settings = section.Get<BasicSettingsOptions>() ?? throw new NullReferenceException($"No section exists with name {sectionName}");

        // Validate mandatory fields!
        string? configErrors = null;
        if (string.IsNullOrWhiteSpace(settings.ClientId))
        {
            configErrors += "ClientId in Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(settings.AuthenticationType))
        {
            configErrors += "AuthenticationType in Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(settings.ProviderId))
        {
            configErrors += "ProviderId in Basic settings must be filled!" + System.Environment.NewLine;
        }

        if (string.IsNullOrWhiteSpace(settings.Scope))
        {
            configErrors += "Scope in Basic settings must be filled!" + System.Environment.NewLine;
        }


        if (configErrors is not null)
        {
            throw new ConfigurationException(configErrors);
        }

        void OptionFiller(BasicSettingsOptions option)
        {
            option.Authority = settings.Authority;
            option.ClientId = settings.ClientId;
            option.AuthenticationType = settings.AuthenticationType;
            option.ProviderId = settings.ProviderId;
            option.Scope = settings.Scope;
        }

        return OptionFiller;
    }

}
