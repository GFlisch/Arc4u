using Arc4u.Caching;
using Arc4u.Standard.OAuth2.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Events
{
    public class customCookieEvents : CookieAuthenticationEvents
    {
        public customCookieEvents(IServiceProvider serviceProvider, IOptions<AuthenticationOidcOptions> oidcOptions)
        {
            _serviceProvider = serviceProvider;
            _oidcOptions = oidcOptions.Value;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly AuthenticationOidcOptions _oidcOptions;

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

            tokensInfo.AccessToken = cookieCtx.Properties.GetTokenValue("access_token");
            tokensInfo.RefreshToken = cookieCtx.Properties.GetTokenValue("refresh_token");

            if (timeRemaining < refreshThreshold)
            {

                IOptionsMonitor<OpenIdConnectOptions> optionsMonitor = _serviceProvider!.GetService<IOptionsMonitor<OpenIdConnectOptions>>();

                var options = optionsMonitor!.Get(OpenIdConnectDefaults.AuthenticationScheme);
                var metadata = await options!.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None);

                var pairs = new Dictionary<string, string>()
                                        {
                                                { "client_id", options.ClientId },
                                                { "client_secret", options.ClientSecret },
                                                { "grant_type", "refresh_token" },
                                                { "refresh_token", tokensInfo.RefreshToken }
                                        };
                var content = new FormUrlEncodedContent(pairs);
                var tokenResponse = await options.Backchannel.PostAsync(metadata.TokenEndpoint, content, CancellationToken.None);
                tokenResponse.EnsureSuccessStatusCode();

                if (tokenResponse.IsSuccessStatusCode)
                {
                    using (var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync()))
                    {
                        // Persist the new acess token
                        cookieCtx.Properties.UpdateTokenValue("access_token", payload!.RootElement!.GetString("access_token"));
                        cookieCtx.Properties.UpdateTokenValue("refresh_token", payload!.RootElement!.GetString("refresh_token"));
                        if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                        {
                            var expirationAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                            cookieCtx.Properties.UpdateTokenValue("expires_at", expirationAt.ToString("o", CultureInfo.InvariantCulture));
                        }
                        
                        //await context.SignInAsync(user, props);

                        // Indicate to the cookie middleware that the cookie should be remade (since we have updated it)
                        cookieCtx.ShouldRenew = true;
                    }
                }
                else
                {
                    cookieCtx.RejectPrincipal();
                    await cookieCtx.HttpContext.SignOutAsync();
                }
            }
        }
    }
}
