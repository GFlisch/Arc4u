using System.Globalization;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.AspNetCore;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TokenProvider;

[Export(CredentialTokenCacheTokenProvider.ProviderName, typeof(ICredentialTokenProvider))]
public class CredentialTokenCacheTokenProvider(ITokenCache tokenCache, ILogger<CredentialTokenCacheTokenProvider> logger, IServiceProvider container, IOptionsMonitor<AuthorityOptions> authorities) : ICredentialTokenProvider
{
    public const string ProviderName = "Credential";

    public async Task<Result<TokenInfo>> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var result = GetContext(settings, out var authority, out var scope);

        if (string.IsNullOrWhiteSpace(credential.Upn))
        {
            result.WithError("No Username is provided.");
        }

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            result.WithError("No password is provided.");
        }

        if (result.IsFailed)
        {
            return result;
        }

        if (null != tokenCache)
        {
            Result<TokenInfo> tokenInfoResult;

            // Get a HashCode from the password so a second call with the same upn but with a wrong password will not be impersonated due to
            // the lack of password check.
            // authority is not null here => messages log and throw will throw an exception if null.
            var cacheKey = BuildKey(credential, authority!, scope);

            logger.Technical().LogCheckContainsKeyInCache(cacheKey);
            var tokenInfo = tokenCache.Get<TokenInfo>(cacheKey);
            var hasChanged = false;

            if (null != tokenInfo)
            {
                tokenInfoResult = Result.Ok(tokenInfo);
                logger.Technical().LogTokenLoadedFromCache(cacheKey);

                if (tokenInfo.ExpiresOnUtc < DateTime.UtcNow.AddMinutes(1))
                {
                    logger.Technical().LogTokenExpiredLoadedFromCache(cacheKey);

                    // We need to refresh the token.
                    tokenInfoResult = await CreateBasicTokenInfoAsync(settings, credential).ConfigureAwait(false);
                    hasChanged = true;
                }
            }
            else
            {
                logger.Technical().LogCallSTS(cacheKey);
                tokenInfoResult = await CreateBasicTokenInfoAsync(settings, credential).ConfigureAwait(false);
                hasChanged = true;
            }

            if (hasChanged)
            {
                try
                {
                    logger.Technical().LogSaveTokenInCache(cacheKey, tokenInfoResult.Value.ExpiresOnUtc);
                    tokenCache.Put(cacheKey, tokenInfoResult.Value);
                }
                catch (Exception ex)
                {
                    tokenInfoResult.WithError(new ExceptionalError(ex));
                }
            }

            return tokenInfoResult;
        }

        // no cache, do a direct call on every calls.
        logger.Technical().LogNoCachePerformance();
        return await CreateBasicTokenInfoAsync(settings, credential).ConfigureAwait(false);

    }

    protected async Task<Result<TokenInfo>> CreateBasicTokenInfoAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var basicTokenProvider = container.GetKeyedService<ICredentialTokenProvider>(CredentialTokenProvider.ProviderName);

        if (basicTokenProvider == null)
        {
            logger.Technical().LogNoTokenProvider(CredentialTokenProvider.ProviderName);
            return Result.Fail($"No token provider found for {CredentialTokenProvider.ProviderName}.");
        }

        return await basicTokenProvider.GetTokenAsync(settings, credential).ConfigureAwait(false);
    }

    private static string BuildKey(CredentialsResult credential, AuthorityOptions authority, string audience)
    {
        if (null == credential?.Password)
        {
            throw new InvalidOperationException("The password cannot be null.");
        }
        return authority.Url + "_" + audience + "_Password_" + credential.Upn + "_" + credential.Password.GetHashCode().ToString(CultureInfo.InvariantCulture);
    }

    private Result GetContext(IKeyValueSettings settings, out AuthorityOptions? authority, out string scope)
    {
        // Check the information.
        var result = new Result();

        if (null == settings)
        {
            authority = null;
            scope = string.Empty;

            return Result.Fail("Settings parameter cannot be null.");
        }

        // Valdate arguments.
        if (!settings.Values.ContainsKey(TokenKeys.Scope))
        {
            result.WithError("Scope is missing. Cannot process the request.");
        }

        logger.Technical().LogCreateAuthenticationContext();

        if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
        {
            authority = authorities.Get("Default");
        }
        else
        {
            authority = authorities.Get(settings.Values[TokenKeys.AuthorityKey]);
        }
        scope = settings.Values[TokenKeys.Scope];

        return result;
    }
}
