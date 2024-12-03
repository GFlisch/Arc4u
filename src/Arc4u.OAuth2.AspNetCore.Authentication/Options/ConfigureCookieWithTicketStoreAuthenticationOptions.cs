using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Options;

/// <summary>
/// This class is registered by default in the <see cref="AuthenticationExtensions"/>.
/// This is possible to registe anothe one via the <see cref="OidcAuthenticationBuilderOptions"/>.
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
        options.ExpireTimeSpan = _options.CurrentValue.AuthenticationTicketTTL;
        options.EventsType = _options.CurrentValue.CookieAuthenticationEventsType;
    }
}
