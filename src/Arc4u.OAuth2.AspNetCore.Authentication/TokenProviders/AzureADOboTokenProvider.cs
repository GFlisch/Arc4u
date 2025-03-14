using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;

namespace Arc4u.OAuth2.TokenProviders;

[Export(ProviderName, typeof(ITokenProvider))]
public class AzureADOboTokenProvider(TokenRefreshInfo tokenRefreshInfo,
                               ICacheHelper cacheHelper,
                               IActivitySourceFactory activitySourceFactory,
                               IApplicationContext applicationContext,
                               IOptionsMonitor<AuthorityOptions> authorities,
                               ILogger<AzureADOboTokenProvider> logger) : ITokenProvider
{
    public const string ProviderName = "Obo";
    private readonly ActivitySource? _activitySource = activitySourceFactory?.GetArc4u();
    private readonly AuthorityOptions _defaultAuthority = authorities.Get("Default");

    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? _)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (applicationContext.Principal is null)
        {
            return new Error("No principal exists.");
        }
        using var activity = _activitySource?.StartActivity("Get on behal of token", ActivityKind.Producer);

        var identity = applicationContext.Principal.Identity as ClaimsIdentity;

        var currentToken = (identity is not null && identity.BootstrapContext is not null) ? identity.BootstrapContext?.ToString() : tokenRefreshInfo?.AccessToken?.Token;

        if (currentToken is null)
        {
            // this should never happend!
            if (identity is null)
            {
                return new Error("The identity given is null.");
            }

            return new Error($"Token cannot be retrieved for the AuthenticationType: {identity.AuthenticationType}");
        }

        var cache = cacheHelper.GetCache();

        // the key is defined by user!
        var cacheKey = $"_Obo_{settings.Values[TokenKeys.ClientIdKey]}_{currentToken.GetHashCode()}_{settings.Values[TokenKeys.Scope]}";

        var tokenFromCache = await cache.GetAsync<TokenInfo>(cacheKey).ConfigureAwait(false);

        // if the token is expired => we need to refresh it! 
        // Not all caches have a TTl defined for a specific key.
        if (tokenFromCache is not null)
        {
            JwtSecurityToken token = new(tokenFromCache.Token);

            // arbitrary 1 minute to have time to perform a request => must be a variable.
            if (token.ValidTo.Subtract(DateTime.UtcNow).TotalSeconds > 60)
            {
                return tokenFromCache;
            }
        }

        // We consider that the access token is still valid.
        // In a Obo from OpenIdConnect this is always the case.
        var pairs = new Dictionary<string, string>()
                                    {
                                            { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                                            { "client_id", settings.Values[TokenKeys.ClientIdKey] },
                                            { "client_secret", settings.Values[TokenKeys.ClientSecret] },
                                            { "assertion", currentToken },
                                            { "scope", settings.Values[TokenKeys.Scope] },
                                            { "requested_token_use", "on_behalf_of" }
                                    };
        using var handler = new HttpClientHandler { UseDefaultCredentials = true };
        using var client = new HttpClient(handler);
        var content = new FormUrlEncodedContent(pairs);
        var endPoint = await _defaultAuthority.GetEndpointAsync(CancellationToken.None).ConfigureAwait(false);
        using var tokenResponse = await client.PostAsync(endPoint, content, CancellationToken.None).ConfigureAwait(false);
        {
            if (!tokenResponse.IsSuccessStatusCode)
            {
                if (IdentityModelEventSource.ShowPII)
                {
                    logger.Technical().LogOboFailedReasonWithPII(tokenResponse.ReasonPhrase ?? "No reason found!");
                }
                else
                {
                    logger.Technical().LogOboFailedWithNoReasonNoPII();
                }
            }

            // throws an exception is not 200OK.
            tokenResponse.EnsureSuccessStatusCode();

            TokenInfo? oboToken = null;

            if (tokenResponse.IsSuccessStatusCode)
            {
                using var payload = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync().ConfigureAwait(false));

                // Persist the new acess token
                if (payload.RootElement.TryGetProperty("expires_in", out var property) && property.TryGetInt32(out var seconds))
                {
                    var expirationAt = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);
                    oboToken = new TokenInfo("access_token", payload!.RootElement!.GetString("access_token") ?? string.Empty, expirationAt.ToUniversalTime());
                }
                else
                {
                    oboToken = new TokenInfo("access_token", payload!.RootElement!.GetString("access_token") ?? string.Empty);
                }

                await cache.PutAsync(cacheKey, oboToken.ExpiresOnUtc - DateTime.UtcNow, oboToken).ConfigureAwait(false);
            }

            if (oboToken is null)
            {
                logger.Technical().LogError("No token was in the paylod of the message during the Obo request.");
                return new Error("No token was in the paylod of the message during the Obo request.");
            }

            return oboToken.ToResult();
        }
    }

    public ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
