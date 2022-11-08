using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProviders
{
    [Export(OidcTokenProvider.ProviderName, typeof(ITokenProvider))]
    public class OidcTokenProvider : ITokenProvider
    {
        const string ProviderName = "Oidc";

        public OidcTokenProvider(ILogger<OidcTokenProvider> logger, TokenRefreshInfo tokenRefreshInfo, IOptions<AuthenticationOidcOptions> oidcOptions)
        {
            _logger = logger;
            _tokenRefreshInfo = tokenRefreshInfo;
            _oidcOptions = oidcOptions.Value;
        }

        private readonly ILogger<OidcTokenProvider> _logger;
        private readonly TokenRefreshInfo _tokenRefreshInfo;
        private readonly AuthenticationOidcOptions _oidcOptions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="platformParameters"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException" />
        /// <returns><see cref="TokenInfo"/></returns>
        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));

            // throw a ArgumentNullException if null.
            var jwtToken = new JwtSecurityToken(_tokenRefreshInfo.AccessToken);

            var timeRemaining = jwtToken.ValidTo.Subtract(DateTime.UtcNow);

            if (timeRemaining > _oidcOptions.ForceRefreshTimeoutTimeSpan)
                return new TokenInfo("Bearer", _tokenRefreshInfo.AccessToken, String.Empty, jwtToken.ValidTo);

            // Refresh the token!

            await Task.Delay(1);

            throw new NotImplementedException();
        }

        public void SignOut(IKeyValueSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
