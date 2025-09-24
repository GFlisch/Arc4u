using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.Blazor.Options;

/// <summary>
/// Provides extension methods to register and configure <see cref="AuthenticationCookieSettingsOption"/> for handling authentication cookies.
/// Options can be configured directly from application configuration or via custom code using an action delegate.
/// </summary>
public static partial class CookieAuthenticationExtensions
{
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

        if (string.IsNullOrWhiteSpace(validate.ProviderId))
        {
            throw new MissingFieldException("RequestUrl field is missing.");
        }

        return validate;
    }

    public static AuthenticationCookieSettingsOption ReadAuthenticationCookieSettingsOption(this IServiceCollection services,
                                                                                                 IConfiguration configuration,
                                                                                                 string sectionName = "Authentication:OAuth2.Settings")
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
            option.RequestUrl = cookieOption.RequestUrl;
        });
    }
}

