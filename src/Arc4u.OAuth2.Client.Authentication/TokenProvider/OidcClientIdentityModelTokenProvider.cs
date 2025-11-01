using Arc4u.Caching;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using FluentResults;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Client.Authentication.TokenProvider;

[Export(TokenProviderName, typeof(ITokenProvider))]
public class OidcClientIdentityModelTokenProvider(
                                                    OidcClientOptions options,
                                                    TokenRefreshInfo tokensInfo,
                                                    ISecureCache secureCache,
                                                    IOptionsMonitor<ApiExtraContextAuthenticationOption> apiExtraContextOption) : ITokenProvider
{
    public const string TokenProviderName = "OidcClientIdentityModel";
    private const string TokenKey = "TokensInfo";
    private const string TokenType = "Bearer";
    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings? settings, object? platformParameters)
    {
        // Check if the token exists?
        if (null == tokensInfo.AccessToken)
        {
            var tokens = await secureCache.GetAsync<TokenRefreshInfo>(TokenKey).ConfigureAwait(false) ??  new TokenRefreshInfo();
            tokensInfo.AccessToken = tokens.AccessToken;
            tokensInfo.RefreshToken = tokens.RefreshToken;
        }

        if (tokensInfo.AccessToken is not null && tokensInfo.AccessToken.ExpiresOnUtc > DateTime.UtcNow)
        {
            return tokensInfo.AccessToken;
        }

        var client = new OidcClient(options);

        if (tokensInfo.RefreshToken is not null && tokensInfo.RefreshToken.ExpiresOnUtc > DateTime.UtcNow)
        {
            var parameters = new Parameters();
            foreach (var parameter in apiExtraContextOption.CurrentValue.AuthorizationParameters.Values)
            {
                parameters.Add(parameter.Key, parameter.Value);
            }
            foreach (var parameter in apiExtraContextOption.CurrentValue.TokenParameters.Values)
            {
                parameters.Add(parameter.Key, parameter.Value);
            }

            var result = await client.RefreshTokenAsync(tokensInfo.RefreshToken.Token, parameters).ConfigureAwait(false);

            if (result.IsError)
            {
                throw new Exception(result.Error);
            }

            tokensInfo.RefreshToken = new TokenInfo(TokenType, result.RefreshToken, DateTime.UtcNow.AddDays(90));
            tokensInfo.AccessToken = new TokenInfo(TokenType, result.AccessToken, result.AccessTokenExpiration.UtcDateTime);
            await secureCache.PutAsync(TokenKey, tokensInfo).ConfigureAwait(false);

            return tokensInfo.AccessToken;
        }

        // Perform the login.
        var loginRequest = new LoginRequest();
        foreach (var parameter in apiExtraContextOption.CurrentValue.AuthorizationParameters.Values)
        {
            loginRequest.BackChannelExtraParameters.Add(parameter.Key, parameter.Value);
        }
        foreach (var parameter in apiExtraContextOption.CurrentValue.TokenParameters.Values)
        {
            loginRequest.FrontChannelExtraParameters.Add(parameter.Key, parameter.Value);
        }
        var loginResult = await client.LoginAsync(loginRequest, CancellationToken.None).ConfigureAwait(false);

        if (loginResult.IsError)
        {
            throw new AccessViolationException(loginResult.Error);
        }

        tokensInfo.RefreshToken = new TokenInfo(TokenType, loginResult.RefreshToken, DateTime.UtcNow.AddDays(90));
        tokensInfo.AccessToken = new TokenInfo(TokenType, loginResult.AccessToken, loginResult.AccessTokenExpiration.UtcDateTime);
        await secureCache.PutAsync("TokensInfo", tokensInfo).ConfigureAwait(false);

        return tokensInfo.AccessToken;
    }

    public async ValueTask SignOutAsync(IKeyValueSettings settings, CancellationToken cancellationToken)
    {
        await secureCache.RemoveAsync(TokenKey, cancellationToken).ConfigureAwait(false);

        var client = new OidcClient(options);

        await client.LogoutAsync(null, cancellationToken).ConfigureAwait(false);
    }
}
