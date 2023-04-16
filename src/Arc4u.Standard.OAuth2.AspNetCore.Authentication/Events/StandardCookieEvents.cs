using System;
using System.Globalization;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Events;

public class StandardCookieEvents : CookieAuthenticationEvents
{
    public StandardCookieEvents(IServiceProvider serviceProvider,
                                ILogger<StandardCookieEvents> logger,
                                IOptions<OidcAuthenticationOptions> oidcOptions,
                                ITokenRefreshProvider tokenRefreshProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _oidcOptions = oidcOptions.Value;
        _tokenRefreshProvider = tokenRefreshProvider;
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly OidcAuthenticationOptions _oidcOptions;
    private readonly ITokenRefreshProvider _tokenRefreshProvider;
    private readonly ILogger<StandardCookieEvents> _logger;

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext cookieCtx)
    {
        ArgumentNullException.ThrowIfNull(_serviceProvider);
        ArgumentNullException.ThrowIfNull(_oidcOptions);

        var now = DateTimeOffset.UtcNow;
        var expiresAt = cookieCtx.Properties.GetTokenValue("expires_at");
        var accessTokenExpiration = DateTimeOffset.Parse(expiresAt);
        var timeRemaining = accessTokenExpiration.Subtract(now);

        // => must be defined in the options
        var refreshThreshold = _oidcOptions.ForceRefreshTimeoutTimeSpan;

        _logger?.LogDebug("Extract token from the cookie cache.");

        // Persist the Access and Refresh tokens.
        // TokenRefreshInfo is registered as Scoped and we create at this moment (by request) an instance to
        // set the information stored in the TicketStore repo based on the cookie information.

        var tokensInfo = _serviceProvider.GetService<TokenRefreshInfo>();

        tokensInfo.AccessToken = new TokenInfo("access_token", cookieCtx.Properties.GetTokenValue("access_token"), accessTokenExpiration.UtcDateTime);
        // As not all the autorities are using a jwt token for the refresh token, the expiration date is not  extracted from the token
        tokensInfo.RefreshToken = new TokenInfo("refresh_token", cookieCtx.Properties.GetTokenValue("refresh_token"), cookieCtx.Properties.ExpiresUtc.Value.UtcDateTime);

        if (timeRemaining < refreshThreshold)
        {
            ArgumentNullException.ThrowIfNull(_tokenRefreshProvider);
            try
            {
                if (timeRemaining < TimeSpan.Zero)
                {
                    _logger?.Technical().LogInformation("Refresh the access token. Expired since {TimeExpired}", timeRemaining.Multiply(-1));
                }
                else
                {
                    _logger?.Technical().LogInformation("Refresh the access token. Will expire in {TimeExpired}", timeRemaining);
                }

                // throws an exception if the call failed.
                await _tokenRefreshProvider.GetTokenAsync(null, null).ConfigureAwait(false);

                cookieCtx.Properties.UpdateTokenValue("access_token", tokensInfo.AccessToken.Token);
                cookieCtx.Properties.UpdateTokenValue("refresh_token", tokensInfo.RefreshToken.Token);
                cookieCtx.Properties.UpdateTokenValue("expires_at", tokensInfo.AccessToken.ExpiresOnUtc.ToString("o", CultureInfo.InvariantCulture));

                cookieCtx.ShouldRenew = true;
                cookieCtx.RejectPrincipal();

            }
            catch (Exception ex)
            {
                _logger?.Technical().LogError("Cannot refresh the token. See exception.");
                _logger?.Technical().LogException(ex);

                cookieCtx.RejectPrincipal();
                await cookieCtx.HttpContext.SignOutAsync().ConfigureAwait(false);
            }

        }
    }
}

