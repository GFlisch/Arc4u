using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProviders;

/// <summary>
/// The purpose of this token provider is to be used by Oidc one.
/// It will refresh a token based on a refresh token and update the scoped TokenRefreshInfo.
/// The Oidc token provider is responsible to get back an access token.
/// </summary>
[Export(RefreshTokenProvider.ProviderName, typeof(ITokenProvider))]
public class RefreshTokenProvider: ITokenProvider
{
    const string ProviderName = "Refresh";

    public RefreshTokenProvider(TokenRefreshInfo refreshInfo, IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions, IOptions<OidcAuthenticationOptions> oidcOptions)
    {
        _tokenRefreshInfo = refreshInfo;
        _openIdConnectOptions = openIdConnectOptions.CurrentValue;
        _oidcOptions = oidcOptions.Value;
    }

    private readonly TokenRefreshInfo _tokenRefreshInfo;
    private readonly OpenIdConnectOptions _openIdConnectOptions;
    private readonly OidcAuthenticationOptions _oidcOptions;

    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
    {
        ArgumentNullException.ThrowIfNull(_tokenRefreshInfo, nameof(_tokenRefreshInfo));
        ArgumentNullException.ThrowIfNull(_openIdConnectOptions, nameof(_openIdConnectOptions));
        ArgumentNullException.ThrowIfNull(_oidcOptions, nameof(_oidcOptions));

        // throw a ArgumentNullException if null.
        var jwtToken = new JwtSecurityToken(_tokenRefreshInfo.AccessToken.Token);

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
                                            { "refresh_token", _tokenRefreshInfo.RefreshToken.Token }
                                    };
            var content = new FormUrlEncodedContent(pairs);
            var tokenResponse = await _openIdConnectOptions.Backchannel.PostAsync(metadata.TokenEndpoint, content, CancellationToken.None);
            tokenResponse.EnsureSuccessStatusCode();

            if (tokenResponse.IsSuccessStatusCode)
            {
                using (var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync()))
                {
                    // Persist the new acess token
                    _tokenRefreshInfo.RefreshToken = new Token.TokenInfo("refresh_token", payload!.RootElement!.GetString("refresh_token"));
                    if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                    {
                        var expirationAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                        _tokenRefreshInfo.AccessToken = new Token.TokenInfo("access_token", payload!.RootElement!.GetString("access_token"), expirationAt.DateTime.ToUniversalTime());
                    }
                    else
                    {
                        _tokenRefreshInfo.AccessToken = new Token.TokenInfo("access_token", payload!.RootElement!.GetString("access_token"));
                    }
                }
            }
        }

        return _tokenRefreshInfo.AccessToken;
    }

    public void SignOut(IKeyValueSettings settings)
    {
        throw new NotImplementedException();
    }
}
