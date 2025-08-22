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
    }
}
