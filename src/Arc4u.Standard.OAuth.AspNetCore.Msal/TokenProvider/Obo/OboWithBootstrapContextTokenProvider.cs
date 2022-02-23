//    internal class OboWithBootstrapContextTokenProvider
using Arc4u.Caching;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    /// <summary>
    /// Assume the jwtBearerToken is on the Identity.BootstrapContext.
    /// </summary>
    public abstract class OboWithBootstrapContextTokenProvider : ITokenProvider
    {
        public const string ProviderName = "Obo";

        public OboWithBootstrapContextTokenProvider(CacheContext cacheContext, IContainerResolve container, ILogger<OboWithBootstrapContextTokenProvider> logger, IActivitySourceFactory activitySourceFactory)
        {
            _cacheContext = cacheContext;
            _container = container;
            _logger = logger;
            _activitySource = activitySourceFactory?.GetArc4u();
        }

        private readonly CacheContext _cacheContext;
        private readonly IContainerResolve _container;
        private readonly ILogger<OboWithBootstrapContextTokenProvider> _logger;
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

            // In case a token is providing by calling the method to do an On behal-of scenario.
            if (tokenInfo is TokenInfo token)
            {
                _tokenInfo = token;
            }
            else
            {
                if (!_container.TryResolve<IApplicationContext>(out IApplicationContext applicationContext))
                {
                    throw new AppException("No user context exists!");
                }

                var rawToken = ((ClaimsIdentity)applicationContext.Principal.Identity).BootstrapContext.ToString();
                var jwtUserToken = new JwtSecurityToken(rawToken);

                _tokenInfo = new TokenInfo("Bearer", rawToken, string.Empty, jwtUserToken.ValidTo);
            }

            // if the settings contains the OAuth2 field we will complete the Obo ones by the OAuth2.
            // This is to avoid the copy in the appSettings of the Authority, ClientId, ApplicationKey, etc...
            var oboSettings = SimpleKeyValueSettings.CreateFrom(settings);

            if (settings.Values.ContainsKey("OAuth2"))
            {
                var oauth2Settings = _container.Resolve<IKeyValueSettings>(settings.Values["OAuth2"]);
                if (!oboSettings.Values.ContainsKey(TokenKeys.ClientIdKey))
                    oboSettings.Add(TokenKeys.ClientIdKey, oauth2Settings.Values[TokenKeys.ClientIdKey]);
                if (!oboSettings.Values.ContainsKey(TokenKeys.AuthorityKey))
                    oboSettings.Add(TokenKeys.AuthorityKey, oauth2Settings.Values[TokenKeys.AuthorityKey]);
                if (!oboSettings.Values.ContainsKey(TokenKeys.ApplicationKey))
                    oboSettings.Add(TokenKeys.ApplicationKey, oauth2Settings.Values[TokenKeys.ApplicationKey]);
            }

            var cca = CreateCca(oboSettings);

            var builder = cca.AcquireTokenOnBehalfOf(oboSettings.Values[TokenKeys.Scopes].Split(',', StringSplitOptions.RemoveEmptyEntries), new UserAssertion(_tokenInfo.AccessToken));

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

