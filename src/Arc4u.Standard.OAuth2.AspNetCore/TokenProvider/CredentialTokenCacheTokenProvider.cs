using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.TokenProvider;

[Export(CredentialTokenCacheTokenProvider.ProviderName, typeof(ICredentialTokenProvider))]
public class CredentialTokenCacheTokenProvider : ICredentialTokenProvider
{
    public const string ProviderName = "Credential";

    private readonly ITokenCache TokenCache;
    private readonly IContainerResolve Container;
    private readonly ILogger<CredentialTokenCacheTokenProvider> _logger;
    private readonly IOptionsMonitor<AuthorityOptions> _authorities;

    public CredentialTokenCacheTokenProvider(ITokenCache tokenCache, ILogger<CredentialTokenCacheTokenProvider> logger, IContainerResolve container, IOptionsMonitor<AuthorityOptions> authorities)
    {
        TokenCache = tokenCache;
        _logger = logger;
        Container = container;
        _authorities = authorities;
    }

    public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var messages = GetContext(settings, out AuthorityOptions? authority, out string scope);

        if (string.IsNullOrWhiteSpace(credential.Upn))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Error, "No Username is provided."));
        }

        if (string.IsNullOrWhiteSpace(credential.Password))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical, ServiceModel.MessageType.Error, "No password is provided."));
        }

        messages.LogAndThrowIfNecessary(_logger);
        messages.Clear();

        if (null != TokenCache)
        {
            // Get a HashCode from the password so a second call with the same upn but with a wrong password will not be impersonated due to
            // the lack of password check.
            // authority is not null here => messages log and throw will throw an exception if null.
            var cacheKey = BuildKey(credential, authority!, scope);

            _logger.Technical().System($"Check if the cache contains a token for {cacheKey}.").Log();
            var tokenInfo = TokenCache.Get<TokenInfo>(cacheKey);
            var hasChanged = false;

            if (null != tokenInfo)
            {
                _logger.Technical().System($"Token loaded from the cache for {cacheKey}.").Log();

                if (tokenInfo.ExpiresOnUtc < DateTime.UtcNow.AddMinutes(1))
                {
                    _logger.Technical().System($"Token is expired for {cacheKey}.").Log();

                    // We need to refresh the token.
                    tokenInfo = await CreateBasicTokenInfoAsync(settings, credential).ConfigureAwait(false);
                    hasChanged = true;
                }
            }
            else
            {
                _logger.Technical().System($"Contact the STS to create an access token for {cacheKey}.").Log();
                tokenInfo = await CreateBasicTokenInfoAsync(settings, credential).ConfigureAwait(false);
                hasChanged = true;
            }

            if (hasChanged)
            {
                try
                {
                    _logger.Technical().System($"Save the token in the cache for {cacheKey}, will expire at {tokenInfo.ExpiresOnUtc} Utc.").Log();
                    TokenCache.Put(cacheKey, tokenInfo);
                }
                catch (Exception ex)
                {
                    _logger.Technical().LogException(ex);
                }

            }

            return tokenInfo;
        }

        // no cache, do a direct call on every calls.
        _logger.Technical().System($"No cache is defined. STS is called for every call.").Log();
        return await CreateBasicTokenInfoAsync(settings, credential).ConfigureAwait(false);

    }

    protected async Task<TokenInfo> CreateBasicTokenInfoAsync(IKeyValueSettings settings, CredentialsResult credential)
    {
        var basicTokenProvider = Container.Resolve<ICredentialTokenProvider>(CredentialTokenProvider.ProviderName);

        return await basicTokenProvider.GetTokenAsync(settings, credential).ConfigureAwait(false);
    }

    private static string BuildKey(CredentialsResult credential, AuthorityOptions authority, string audience)
    {
        return authority.Url + "_" + audience + "_Password_" + credential.Upn + "_" + credential.Password.GetHashCode().ToString();
    }

    private Messages GetContext(IKeyValueSettings settings, out AuthorityOptions? authority, out string scope)
    {
        // Check the information.
        var messages = new Messages();

        if (null == settings)
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                                     ServiceModel.MessageType.Error,
                                     "Settings parameter cannot be null."));
            authority = null;
            scope = string.Empty;

            return messages;
        }

        // Valdate arguments.
        if (!settings.Values.ContainsKey(TokenKeys.Scope))
        {
            messages.Add(new Message(ServiceModel.MessageCategory.Technical,
                     ServiceModel.MessageType.Error,
                     "Scope is missing. Cannot process the request."));
        }

        _logger.Technical().System($"Creating an authentication context for the request.").Log();

        if (!settings.Values.ContainsKey(TokenKeys.AuthorityKey))
        {
            authority = _authorities.Get("Default");
        }
        else
        {
            authority = _authorities.Get(settings.Values[TokenKeys.AuthorityKey]);
        }
        scope = settings.Values[TokenKeys.Scope];

        return messages;

    }
}
