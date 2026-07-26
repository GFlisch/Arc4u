using Arc4u.Blazor.Handlers;
using Arc4u.Configuration;
using Arc4u.OAuth2.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Blazor.Options;

/// <summary>
/// Provides extension methods to register and configure <see cref="AuthenticationCookieSettingsOption"/> for handling authentication cookies.
/// Options can be configured directly from application configuration or via custom code using an action delegate.
/// </summary>
public static class CookieAuthenticationExtensions
{
    public static void AddAuthenticationCookie(this IServiceCollection services,
                                                    Action<AuthenticationCookieSettingsOption> option,
                                                    string sectionKey = "OAuth2")
    {
        ArgumentNullException.ThrowIfNull(option);
        ArgumentNullException.ThrowIfNull(sectionKey);

        var authenticationCookieSettingsOption = services.ReadAuthenticationCookieSettingsOption(option);

        ConfigureAuthenticationCookieSettings(services, sectionKey, authenticationCookieSettingsOption);
    }

    public static void AddAuthenticationCookie(this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Authentication:OAuth.Settings", string sectionKey = "OAuth2")
    {
        var authenticationCookieSettingsOption =
            services.ReadAuthenticationCookieSettingsOption(configuration, sectionName);

        ConfigureAuthenticationCookieSettings(services, sectionKey, authenticationCookieSettingsOption);
    }

    private static void ConfigureAuthenticationCookieSettings(IServiceCollection services,
                                                              string sectionKey,
                                                              AuthenticationCookieSettingsOption authenticationCookieSettingsOption)
    {
        // Register the settings for the cookie client token provider.
        void SettingsFiller(SimpleKeyValueSettings keyOptions)
        {
            keyOptions.Add(TokenKeys.ProviderIdKey, authenticationCookieSettingsOption.ProviderId);
            keyOptions.Add(TokenKeys.TokenRequestUrl, authenticationCookieSettingsOption.TokenRequestUrl);
            keyOptions.Add(TokenKeys.HttpClientName, authenticationCookieSettingsOption.HttpClientName);
        }

        services.Configure<SimpleKeyValueSettings>(sectionKey, SettingsFiller);

        services.AddHttpClient(authenticationCookieSettingsOption.HttpClientName,
                client => { client.BaseAddress = authenticationCookieSettingsOption.BaseUri; })
                .AddHttpMessageHandler<AttachCookiesHandler>();
    }

    public static AuthenticationCookieSettingsOption ReadAuthenticationCookieSettingsOption(this IServiceCollection services, Action<AuthenticationCookieSettingsOption> option)
    {
        var validate = new AuthenticationCookieSettingsOption();
        option(validate);

        if (string.IsNullOrWhiteSpace(validate.HttpClientName))
        {
            throw new MissingFieldException("HttpClientName field is missing.");
        }

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            throw new MissingFieldException("ProviderId field is missing.");
        }

        if (string.IsNullOrWhiteSpace(validate.TokenRequestUrl))
        {
            throw new MissingFieldException("TokenRequestUrl field is missing.");
        }

        if (validate.BaseUri is null || string.IsNullOrWhiteSpace(validate.BaseUri.AbsoluteUri))
        {
            throw new MissingFieldException("BaseUri field is missing.");
        }

        return validate;
    }

    public static AuthenticationCookieSettingsOption ReadAuthenticationCookieSettingsOption(this IServiceCollection services,
                                                                                                 IConfiguration configuration,
                                                                                                 string sectionName = "Authentication:OAuth.Settings")
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentNullException(nameof(sectionName));
        }

        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(sectionName);

        if (!section.Exists())
        {
            throw new ArgumentException($"Section {sectionName} not found.");
        }

        var cookieOption = configuration.GetSection(sectionName).Get<AuthenticationCookieSettingsOption>();

        if (cookieOption is null)
        {
            throw new NullReferenceException(nameof(cookieOption));
        }

        return ReadAuthenticationCookieSettingsOption(services, option =>
        {
            option.BaseUri = cookieOption.BaseUri;
            option.HttpClientName = cookieOption.HttpClientName;
            option.ProviderId = cookieOption.ProviderId;
            option.TokenRequestUrl = cookieOption.TokenRequestUrl;
        });
    }
}

