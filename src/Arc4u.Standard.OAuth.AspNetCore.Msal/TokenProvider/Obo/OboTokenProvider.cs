using Arc4u.Caching;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    public abstract class OboTokenProvider : ITokenProvider
    {
        public const string ProviderName = "Obo";

        public OboTokenProvider(CacheContext cacheContext, IContainerResolve container, ILogger<OboTokenProvider> logger, IActivitySourceFactory activitySourceFactory)
        {
            _cacheContext = cacheContext;
            _container = container;
            _logger = logger;
            _activitySource = activitySourceFactory?.GetArc4u();
        }

        private readonly CacheContext _cacheContext;
        private readonly IContainerResolve _container;
        private readonly ILogger<OboTokenProvider> _logger;
        private readonly ActivitySource _activitySource;

        /// <summary>
        /// Create a token based on the current identity of the user.
        /// If the tokenInfo is given the call to extract the tokenInfo is not needed.
        /// </summary>
        /// <param name="settings">The Obo key values</param>
        /// <param name="tokenInfo">null or the TokenInfo</param>
        /// <returns></returns>
        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object tokenInfo)
        {
            var messages = new Messages();

            if (null == settings)
            {
                throw new AppException(new Message(ServiceModel.MessageCategory.Technical,
                                                   MessageType.Error,
                                                   "Settings parameter cannot be null."));
            }

            TokenInfo _tokenInfo = null;

            using var activity = _activitySource?.StartActivity("Get on behal of token", ActivityKind.Producer);

            var cache = string.IsNullOrEmpty(_cacheContext.Principal?.CacheName) ? _cacheContext.Default : _cacheContext[_cacheContext.Principal?.CacheName];

            var cacheKey = $"_Obo_{settings.Values[TokenKeys.ClientIdKey]}_{settings.GetHashCode()}";

            var tokenFromCache = await cache.GetAsync<TokenInfo>(cacheKey);

            if (null != tokenFromCache)
                return tokenFromCache;

            // retrieve the token and do an On behal-of scenario.
            if (tokenInfo is TokenInfo token)
            {
                _tokenInfo = token;
            }
            else
            {
                // Get token from the identity user.
                var settingsProviderName = settings.Values.ContainsKey("OpenIdSettingsReader") ? settings.Values["OpenIdSettingsReader"] : "OpenId";

                if (_container.TryResolve<IKeyValueSettings>(settingsProviderName, out var openIdSettings))
                {
                    if (!openIdSettings.Values.ContainsKey(TokenKeys.ProviderIdKey))
                        messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, "No Provider defined in OpenId Settings."));
                    else
                    {
                        var tokenProviderName = openIdSettings.Values[TokenKeys.ProviderIdKey];

                        if (_container.TryResolve<ITokenProvider>(tokenProviderName, out var provider))
                        {
                            _tokenInfo = await provider.GetTokenAsync(openIdSettings, null);
                        }
                        else
                            messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, $"Cannot resolve a token provider with name {tokenProviderName}."));
                    }
                }
                else
                    messages.Add(new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, $"Cannot resolve the KeyValues settings with name {settingsProviderName}."));

                messages.LogAndThrowIfNecessary(_logger);
            }

            var cca = CreateCca(settings);

            var builder = cca.AcquireTokenOnBehalfOf(settings.Values[TokenKeys.Scopes].Split(',', StringSplitOptions.RemoveEmptyEntries), new UserAssertion(_tokenInfo.AccessToken));

            var authenticationResult = await builder.ExecuteAsync();

            var jwtToken = new JwtSecurityToken(authenticationResult.AccessToken);

            _tokenInfo = new TokenInfo(authenticationResult.TokenType, authenticationResult.AccessToken, authenticationResult.IdToken, jwtToken.ValidTo);

            await cache.PutAsync(cacheKey, _tokenInfo.ExpiresOnUtc - DateTime.UtcNow, _tokenInfo);

            return _tokenInfo;
        }

        protected abstract IConfidentialClientApplication CreateCca(IKeyValueSettings valueSettings);

        public void SignOut(IKeyValueSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
