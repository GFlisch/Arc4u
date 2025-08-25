using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Options;

/// <summary>
/// This class is registered by default in the AuthenticationExtensions static class.
/// This is possible to registe anothe one via the OidcAuthenticationBuilderOptions.
/// </summary>
public class ConfigureCookieWithTicketStoreAuthenticationOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly ITicketStore _ticketStore;
    private readonly IOptionsMonitor<OidcAuthenticationOptions> _options;

    public ConfigureCookieWithTicketStoreAuthenticationOptions(ITicketStore ticketStore, IOptionsMonitor<OidcAuthenticationOptions> optionsMonitor)
    {
        _ticketStore = ticketStore;
        _options = optionsMonitor;
    }

    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        options.SessionStore = _ticketStore;
        options.Cookie.Name = _options.CurrentValue.CookieName;
        options.SlidingExpiration = true;
        // Set the expiration time span to the minimum of AuthenticationTicketTTL and RefreshTokenLifetime
        // to avoid having a ticket that is expired but still valid.
        options.ExpireTimeSpan = _options.CurrentValue.AuthenticationTicketTTL < _options.CurrentValue.RefreshTokenLifetime
            ? _options.CurrentValue.AuthenticationTicketTTL
            : _options.CurrentValue.RefreshTokenLifetime;
        options.EventsType = typeof(CookieAuthenticationEvents);
        // we need this to persist the cookie and keep the user logged in.
        options.Cookie.IsEssential = true;
        // Need to set the same site to Lax to allow the cookie to be sent to the portal from the authority provider.
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        options.Cookie.MaxAge = _options.CurrentValue.RefreshTokenLifetime;
    }
}
