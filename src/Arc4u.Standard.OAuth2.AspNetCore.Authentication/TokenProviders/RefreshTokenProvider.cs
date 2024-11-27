using System.Diagnostics;
using System.Text.Json;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;

namespace Arc4u.OAuth2.TokenProviders;

/// <summary>
/// The purpose of this token provider is to be used by Oidc one.
/// It will refresh a token based on a refresh token and update the scoped TokenRefreshInfo.
/// The Oidc token provider is responsible to get back an access token.
/// </summary>
[Export(typeof(ITokenRefreshProvider))]
public class RefreshTokenProvider : ITokenRefreshProvider
{
    public const string ProviderName = "Refresh";

    public RefreshTokenProvider(TokenRefreshInfo refreshInfo,
                                IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions,
                                IOptions<OidcAuthenticationOptions> oidcOptions,
                                IActivitySourceFactory activitySourceFactory,
                                ILogger<RefreshTokenProvider> logger)
    {
        _tokenRefreshInfo = refreshInfo;
        _openIdConnectOptions = openIdConnectOptions;
        _oidcOptions = oidcOptions.Value;
        _logger = logger;
        _activitySource = activitySourceFactory?.GetArc4u();
    }

    private readonly TokenRefreshInfo _tokenRefreshInfo;
    private readonly IOptionsMonitor<OpenIdConnectOptions> _openIdConnectOptions;
    private readonly OidcAuthenticationOptions _oidcOptions;
    private readonly ILogger<RefreshTokenProvider> _logger;
    private readonly ActivitySource? _activitySource;

    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        ArgumentNullException.ThrowIfNull(_tokenRefreshInfo);
        ArgumentNullException.ThrowIfNull(_openIdConnectOptions);
        ArgumentNullException.ThrowIfNull(_oidcOptions);

        using var activity = _activitySource?.StartActivity("Get on behal of token", ActivityKind.Producer);

        // Check if the token refresh is not expired. 
        // if yes => we have to log this and return a Unauthorized!
        if (DateTime.UtcNow > _tokenRefreshInfo.RefreshToken.ExpiresOnUtc)
        {
            _logger.Technical().LogError($"Refresh token is expired: {_tokenRefreshInfo.RefreshToken.ExpiresOnUtc}.");
            throw new InvalidOperationException("Refreshing the token is impossible, validity date is expired.");
        }

        var options = _openIdConnectOptions.Get(OpenIdConnectDefaults.AuthenticationScheme) ?? throw new InvalidCastException("The OpenIdConnectOptions is not found!");
        var metadata = await options!.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);

        var pairs = new Dictionary<string, string>()
                                    {
                                            { "client_id", options.ClientId ?? string.Empty},
                                            { "client_secret", options.ClientSecret ?? string.Empty },
                                            { "grant_type", "refresh_token" },
                                            { "refresh_token", _tokenRefreshInfo.RefreshToken.Token }
                                    };
        var content = new FormUrlEncodedContent(pairs);

        var tokenResponse = await options.Backchannel.PostAsync(metadata.TokenEndpoint, content, CancellationToken.None).ConfigureAwait(false);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            if (IdentityModelEventSource.ShowPII)
            {
                _logger.Technical().LogError($"Refreshing the token is failing. {tokenResponse.ReasonPhrase}");
            }
            else
            {
                _logger.Technical().LogError("Refreshing the token is failing. Enable PII to have more info.");
            }
        }
        // throws an exception is not 200OK.
        tokenResponse.EnsureSuccessStatusCode();

        if (tokenResponse.IsSuccessStatusCode)
        {
            using var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
            // Persist the new acess token
            var refresh_token = payload!.RootElement!.GetString("refresh_token");
            var access_token = payload!.RootElement!.GetString("access_token")!;
            if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
            {
                var expirationAt = DateTimeOffset.UtcNow.AddSeconds(seconds).DateTime.ToUniversalTime();
                _tokenRefreshInfo.AccessToken = new Token.TokenInfo("access_token", access_token, expirationAt);
                if (!string.IsNullOrEmpty(refresh_token))
                {
                    _tokenRefreshInfo.RefreshToken = new Token.TokenInfo("refresh_token", refresh_token, expirationAt);
                }
            }
            else
            {
                _tokenRefreshInfo.AccessToken = new Token.TokenInfo("access_token", access_token);
                if (!string.IsNullOrEmpty(refresh_token))
                {
                    _tokenRefreshInfo.RefreshToken = new Token.TokenInfo("refresh_token", refresh_token, _tokenRefreshInfo.RefreshToken.ExpiresOnUtc);
                }
            }
        }

        return _tokenRefreshInfo.AccessToken;
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        // there is no Signout on a provider for the token refresh...
        throw new NotImplementedException();
    }
}
