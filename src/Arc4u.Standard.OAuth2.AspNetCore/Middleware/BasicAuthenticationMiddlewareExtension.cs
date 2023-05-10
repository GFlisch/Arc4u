using Arc4u.Configuration;
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
using System.Text;
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

        var configErrorsStringBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(settings.BasicSettingsPath))
        {
            configErrorsStringBuilder.AppendLine("BasicSettingsPath must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.CertificateHeaderPath))
        {
            configErrorsStringBuilder.AppendLine("CertificateHeaderPath must be filled!");
        }

        if (configErrorsStringBuilder.Length > 0)
        {
            throw new ConfigurationException(configErrorsStringBuilder.ToString());
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
    }

    public static SimpleKeyValueSettings BuildBasics(this Action<BasicSettingsOptions> options)
    {
        var validate = new BasicSettingsOptions();
        options(validate);

        var configErrorsStringBuilder = new StringBuilder();

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            configErrorsStringBuilder.AppendLine("ProviderId field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.Audience))
        {
            configErrorsStringBuilder.AppendLine("Audiences field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.Authority))
        {
            configErrorsStringBuilder.AppendLine("Authorithy field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.ClientId))
        {
            configErrorsStringBuilder.AppendLine("ClientId field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.AuthenticationType))
        {
            configErrorsStringBuilder.AppendLine("AuthenticationType field is not defined.");
        }

        if (string.IsNullOrWhiteSpace(validate.Scope))
        {
            configErrorsStringBuilder.AppendLine("Scope field is not defined.");
        }

        if (configErrorsStringBuilder.Length > 0)
        {
            throw new ConfigurationException(configErrorsStringBuilder.ToString());
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on this.

        var settings = new SimpleKeyValueSettings();

        settings.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
        settings.Add(TokenKeys.AuthenticationTypeKey, validate.AuthenticationType);
        settings.Add(TokenKeys.AuthorityKey, validate.Authority);
        settings.Add(TokenKeys.ClientIdKey, validate.ClientId);
        settings.Add(TokenKeys.Audience, validate.Audience);
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
        var configErrorsStringBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(settings.ClientId))
        {
            configErrorsStringBuilder.AppendLine("ClientId in Basic settings must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.Authority))
        {
            configErrorsStringBuilder.AppendLine("Authority in Basic settings must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            configErrorsStringBuilder.AppendLine("Audience in Basic settings must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.AuthenticationType))
        {
            configErrorsStringBuilder.AppendLine("AuthenticationType in Basic settings must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.ProviderId))
        {
            configErrorsStringBuilder.AppendLine("ProviderId in Basic settings must be filled!");
        }

        if (string.IsNullOrWhiteSpace(settings.Scope))
        {
            configErrorsStringBuilder.AppendLine("Scope in Basic settings must be filled!");
        }

        if (configErrorsStringBuilder.Length > 0)
        {
            throw new ConfigurationException(configErrorsStringBuilder.ToString());
        }

        void OptionFiller(BasicSettingsOptions option)
        {
            option.Authority = settings.Authority;
            option.ClientId = settings.ClientId;
            option.Audience = settings.Audience;
            option.AuthenticationType = settings.AuthenticationType;
            option.ProviderId = settings.ProviderId;
            option.Scope = settings.Scope;
        }

        return OptionFiller;
    }

}
