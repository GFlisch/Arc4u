using Arc4u.OAuth2.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2
{
    public class TokenRefresh
    {
        public TokenRefresh(TokenRefreshInfo refreshInfo, IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions, IOptions<OidcAuthenticationOptions> oidcOptions)
        {
            _tokenRefreshInfo = refreshInfo;
            _openIdConnectOptions = openIdConnectOptions.CurrentValue;
            _oidcOptions = oidcOptions.Value;
        }

        private readonly TokenRefreshInfo _tokenRefreshInfo;
        private readonly OpenIdConnectOptions _openIdConnectOptions;
        private readonly OidcAuthenticationOptions _oidcOptions;

        public async Task Refresh()
        {

            ArgumentNullException.ThrowIfNull(_tokenRefreshInfo, nameof(_tokenRefreshInfo));
            ArgumentNullException.ThrowIfNull(_openIdConnectOptions, nameof(_openIdConnectOptions));
            ArgumentNullException.ThrowIfNull(_oidcOptions, nameof(_oidcOptions));

            var now = DateTimeOffset.UtcNow;

            // throw a ArgumentNullException if null.
            var jwtToken = new JwtSecurityToken(_tokenRefreshInfo.AccessToken);

            var timeRemaining = jwtToken.ValidTo.Subtract(DateTime.UtcNow);

            // => must be defined in the options
            var refreshThreshold = _oidcOptions.ForceRefreshTimeoutTimeSpan;

            if (timeRemaining < refreshThreshold)
            {
                var metadata = await _openIdConnectOptions.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None);

                var pairs = new Dictionary<string, string>()
                                        {
                                                { "client_id", _openIdConnectOptions.ClientId },
                                                { "client_secret", _openIdConnectOptions.ClientSecret },
                                                { "grant_type", "refresh_token" },
                                                { "refresh_token", _tokenRefreshInfo.RefreshToken }
                                        };
                var content = new FormUrlEncodedContent(pairs);
                var tokenResponse = await _openIdConnectOptions.Backchannel.PostAsync(metadata.TokenEndpoint, content, CancellationToken.None);
                tokenResponse.EnsureSuccessStatusCode();

                if (tokenResponse.IsSuccessStatusCode)
                {
                    using (var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync()))
                    {

                        // Persist the new acess token
                        _tokenRefreshInfo.AccessToken = payload!.RootElement!.GetString("access_token");
                        _tokenRefreshInfo.RefreshToken = payload!.RootElement!.GetString("refresh_token");
                        //if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                        //{
                        //    var expirationAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                        //    cookieCtx.Properties.UpdateTokenValue("expires_at", expirationAt.ToString("o", CultureInfo.InvariantCulture));
                        //}

                        ////await context.SignInAsync(user, props);

                        //// Indicate to the cookie middleware that the cookie should be remade (since we have updated it)
                        //cookieCtx.ShouldRenew = true;
                    }
                }
            }
        }
    }
}