using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProviders;

[Export(AzureADOboTokenProvider.ProviderName, typeof(ITokenProvider))]
public class AzureADOboTokenProvider : ITokenProvider
{

    public AzureADOboTokenProvider(TokenRefreshInfo tokenRefreshInfo,
                                   CacheContext cacheContext, 
                                   IOptionsMonitor<OpenIdConnectOptions> openIdConnectOptions,
                                   IActivitySourceFactory activitySourceFactory,
                                   ILogger<AzureADOboTokenProvider> logger)
    {
        _logger = logger;
        _openIdConnectOptions= openIdConnectOptions.Get(OpenIdConnectDefaults.AuthenticationScheme);
        _cacheContext = cacheContext;
        _tokenRefreshInfo = tokenRefreshInfo;
        _activitySource = activitySourceFactory?.GetArc4u();
    }

    const string ProviderName = "Obo";


    private readonly ILogger<AzureADOboTokenProvider> _logger;
    private readonly OpenIdConnectOptions _openIdConnectOptions;
    private OpenIdConnectConfiguration? _metadata;
    private readonly CacheContext _cacheContext;
    private readonly TokenRefreshInfo _tokenRefreshInfo;
    private readonly ActivitySource _activitySource;
    

    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
    {
        ArgumentNullException.ThrowIfNull(_openIdConnectOptions, nameof(_openIdConnectOptions));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));

        using var activity = _activitySource?.StartActivity("Get on behal of token", ActivityKind.Producer);

        var cache = string.IsNullOrEmpty(_cacheContext.Principal?.CacheName) ? _cacheContext.Default : _cacheContext[_cacheContext.Principal?.CacheName];

        // the key is defined by user!
        var cacheKey = $"_Obo_{settings.Values[TokenKeys.ClientIdKey]}_{_tokenRefreshInfo.AccessToken.Token.GetHashCode()}";

        var tokenFromCache = await cache.GetAsync<TokenInfo>(cacheKey);

        // if the token is expired => we need to refresh it! 
        // Not all caches have a TTl defined for a specific key.
        if (tokenFromCache is not null)
        {
            JwtSecurityToken token = new(tokenFromCache.Token);

            // arbitrary 1 minute to have time to perform a request => must be a variable.
            if (token.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds > 60) 
                return tokenFromCache;
        }
            
        _metadata ??= await _openIdConnectOptions!.ConfigurationManager!.GetConfigurationAsync(CancellationToken.None);

        // We consider that the access token is still valid.
        // In a Obo from OpenIdConnect this is always the case.

        var pairs = new Dictionary<string, string>()
                                    {
                                            { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                                            { "client_id", settings.Values[TokenKeys.ClientIdKey] },
                                            { "client_secret", settings.Values[TokenKeys.ApplicationKey] },
                                            { "assertion", _tokenRefreshInfo.AccessToken.Token },
                                            { "scope", settings.Values[TokenKeys.Scopes] },
                                            { "requested_token_use", "on_behalf_of" }
                                    };
        var content = new FormUrlEncodedContent(pairs);
        var tokenResponse = await _openIdConnectOptions.Backchannel.PostAsync(_metadata.TokenEndpoint, content, CancellationToken.None);
        
        if (!tokenResponse.IsSuccessStatusCode)
        {
            if (IdentityModelEventSource.ShowPII)
                _logger.Technical().LogError($"Getting the Access token with Obo failed. {tokenResponse.ReasonPhrase}");
            else
                _logger.Technical().LogError("Getting the Access token with Obo failed. Enable PII to have more info.");
        }

        // throws an exception is not 200OK.
        tokenResponse.EnsureSuccessStatusCode();

        TokenInfo? oboToken = null;

        if (tokenResponse.IsSuccessStatusCode)
        {
            using (var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync()))
            {
                // Persist the new acess token
                if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                {
                    var expirationAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                    oboToken = new Token.TokenInfo("access_token", payload!.RootElement!.GetString("access_token"), expirationAt.DateTime.ToUniversalTime());
                }
                else
                {
                    oboToken = new Token.TokenInfo("access_token", payload!.RootElement!.GetString("access_token"));
                }

                await cache.PutAsync(cacheKey, oboToken.ExpiresOnUtc - DateTime.UtcNow, oboToken);
            }
        }

        if (oboToken is null)
        {
            _logger.Technical().LogError("No token was in the paylod of the message during the Obo request.");
            throw new NullReferenceException(nameof(oboToken));
        }

        return oboToken;
    }

    public void SignOut(IKeyValueSettings settings)
    {
        throw new NotImplementedException();
    }
}
