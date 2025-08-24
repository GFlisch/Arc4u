using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Options;

/// <summary>
/// This class is registered by default in the static class "AuthenticationExtensions" in the method AddOidcAuthentication".
/// </summary>
public class ConfigureStandardCookieAuthenticationOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly IOptionsMonitor<OidcAuthenticationOptions> _options;

    public ConfigureStandardCookieAuthenticationOptions(IOptionsMonitor<OidcAuthenticationOptions> optionsMonitor)
    {
        _options = optionsMonitor;
    }

    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        options.Cookie.Name = _options.CurrentValue.CookieName;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = _options.CurrentValue.AuthenticationTicketTTL;
        options.EventsType = typeof(CookieAuthenticationEvents);
        // we need this to persist the cookie and keep the user logged in.
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        options.Cookie.MaxAge = _options.CurrentValue.RefreshTokenLifetime;
    }
}
