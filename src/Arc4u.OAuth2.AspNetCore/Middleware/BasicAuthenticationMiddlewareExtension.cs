using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Arc4u.Configuration;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Arc4u.Security;
using Arc4u.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using X509CertificateLoader = Arc4u.Security.Cryptography.X509CertificateLoader;

namespace Arc4u.OAuth2.Middleware;

public static class BasicAuthenticationMiddlewareExtension
{
    public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<BasicAuthenticationMiddleware>(app.ApplicationServices);
    }

    public static void AddBasicAuthenticationSettings(this IServiceCollection services, IConfiguration configuration, string sectionName = "Authentication:Basic", IX509CertificateLoader? certificateLoader = null, bool throwExceptionIfSectionDoesntExist = true)
    {
        var section = configuration.GetSection(sectionName);

        if (section is null || !section.Exists())
        {
            if (throwExceptionIfSectionDoesntExist)
            {
                throw new ConfigurationException($"No section exists with name {sectionName} in the configuration providers for Basic authentication.");
            }

            return;
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
            if (!Regex.IsMatch(basicOptions.DefaultUpn, @"^@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
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

        var hasSpecificAuthority = RegisterBasicAuthority(services, basicOptions.BasicOptions);

        var settings = basicOptions.BasicOptions.BuildBasics();

        // The Authority will be used to retrieve the AuthorityOption.
        if (hasSpecificAuthority)
        {
            settings.Add(TokenKeys.AuthorityKey, "Basic");
        }

        services.Configure<BasicAuthenticationSettingsOptions>(basicSettings =>
        {
            basicSettings.BasicSettings = settings;
            basicSettings.DefaultUpn = basicOptions.DefaultUpn;
            basicSettings.CertificateHeaderOptions = certs;
        });

    }

    public static bool RegisterBasicAuthority(IServiceCollection services, Action<BasicSettingsOptions> options)
    {
        var validate = new BasicSettingsOptions();
        options(validate);

        if (validate.Authority is null)
        {
            return false;
        }

        services.AddAuthority(authOptions =>
        {
            authOptions.SetData(validate.Authority.Url, validate.Authority.TokenEndpoint, validate.Authority.MetaDataAddress);
        }, "Basic");

        return true;
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

        if (!validate.Scopes.Any())
        {
            configErrors += "Scope field is not defined." + System.Environment.NewLine;
        }

        if (!string.IsNullOrWhiteSpace(configErrors))
        {
            throw new ConfigurationException(configErrors);
        }

        // We map this to a IKeyValuesSettings dictionary.
        // The TokenProviders are based on 

        var settings = new SimpleKeyValueSettings();

        settings.Add(TokenKeys.ProviderIdKey, validate!.ProviderId);
        settings.Add(TokenKeys.AuthenticationTypeKey, validate.AuthenticationType);
        settings.Add(TokenKeys.ClientIdKey, validate.ClientId);
        settings.Add(TokenKeys.Scope, string.Join(' ', validate.Scopes));
        if (!string.IsNullOrWhiteSpace(validate.ClientSecret))
        {
            settings.Add(TokenKeys.ClientSecret, validate.ClientSecret!);
        }

        return settings;
    }
    private static Action<Dictionary<string, X509Certificate2>> PrepareCertificatesAction(IConfiguration configuration, [DisallowNull] string sectionName, [DisallowNull] IX509CertificateLoader certificateLoader)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(certificateLoader);

        var section = configuration.GetSection(sectionName);

        if (section is null)
        {
            throw new InvalidOperationException($"No section exists with name {sectionName}");
        }

        var settings = section.Get<Dictionary<string, CertificateStoreOrFileInfo>>() ?? new Dictionary<string, CertificateStoreOrFileInfo>();

        void CertificateFiller(Dictionary<string, X509Certificate2> option)
        {
            foreach (var setting in settings)
            {
                var cert = certificateLoader.FindCertificate(setting.Value);
                // must add a functionality to load it from a File or Store
                if (cert is not null)
                {
                    option.Add(setting.Key, cert);
                }
            }
        }

        return CertificateFiller;
    }
    private static Action<BasicSettingsOptions> PrepareBasicAction(IConfiguration configuration, [DisallowNull] string sectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentNullException.ThrowIfNull(configuration);

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

        if (!settings.Scopes.Any())
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
            option.Scopes = settings.Scopes;
            option.ClientSecret = settings.ClientSecret;
        }

        return OptionFiller;
    }

}
