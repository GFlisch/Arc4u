using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Events
{
    public class customCookieEvents : CookieAuthenticationEvents
    {
        public customCookieEvents(IServiceProvider serviceProvider, IOptions<OidcAuthenticationOptions> oidcOptions, ITokenRefreshProvider tokenRefreshProvider)
        {
            _serviceProvider = serviceProvider;
            _oidcOptions = oidcOptions.Value;
            _tokenRefreshProvider = tokenRefreshProvider;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly OidcAuthenticationOptions _oidcOptions;
        private readonly ITokenRefreshProvider _tokenRefreshProvider;

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext cookieCtx)
        {
            var now = DateTimeOffset.UtcNow;
            var expiresAt = cookieCtx.Properties.GetTokenValue("expires_at");
            var accessTokenExpiration = DateTimeOffset.Parse(expiresAt);
            var timeRemaining = accessTokenExpiration.Subtract(now);

            // => must be defined in the options
            var refreshThreshold = _oidcOptions.ForceRefreshTimeoutTimeSpan;

            // Persist the Access and Refresh tokens.
            var tokensInfo = _serviceProvider!.GetService<TokenRefreshInfo>();

            tokensInfo.AccessToken = new TokenInfo("access_token", cookieCtx.Properties.GetTokenValue("access_token"));
            tokensInfo.RefreshToken = new TokenInfo("refresh_token", cookieCtx.Properties.GetTokenValue("refresh_token"));

            if (timeRemaining < refreshThreshold)
            {
                ArgumentNullException.ThrowIfNull(_tokenRefreshProvider, nameof(_tokenRefreshProvider));
                try
                {
                    // throws an exception if the call failed.
                    await _tokenRefreshProvider.GetTokenAsync(null, null);

                    cookieCtx.Properties.UpdateTokenValue("access_token", tokensInfo.AccessToken.Token);
                    cookieCtx.Properties.UpdateTokenValue("refresh_token", tokensInfo.RefreshToken.Token);
                    cookieCtx.Properties.UpdateTokenValue("expires_at", tokensInfo.AccessToken.ExpiresOnUtc.ToString("o", CultureInfo.InvariantCulture));

                    cookieCtx.ShouldRenew = true;
                    cookieCtx.RejectPrincipal();

                }
                catch (Exception)
                {
                    cookieCtx.RejectPrincipal();
                    await cookieCtx.HttpContext.SignOutAsync();
                }

            }
        }
    }
}

