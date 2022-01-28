using Arc4u;
using Arc4u.Dependency;
using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Identity.Web;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(ProviderName, typeof(ITokenProvider)), Scoped]
    public class MsalTokenProvider : ITokenProvider
    {
        const string ProviderName = "Msal";

        public MsalTokenProvider(IContainerResolve container, IApplicationContext applicationContext)
        {
            _container = container;
            _applicationContext = applicationContext;
        }

        private readonly IContainerResolve _container;
        private readonly IApplicationContext _applicationContext;

        public async Task<TokenInfo> GetTokenAsync(IKeyValueSettings settings, object platformParameters)
        {
            var authenticationType = _applicationContext.Principal.Identity.AuthenticationType;

            if (authenticationType.Equals(Arc4u.Standard.OAuth2.Constants.BearerAuthenticationType))
            {
                var identity = _applicationContext.Principal?.Identity as ClaimsIdentity;

                if (null != identity?.BootstrapContext)
                {
                    var tokenInfo = TokenIsValid(settings, identity.BootstrapContext.ToString());

                    if (null != tokenInfo)
                        return tokenInfo;
                }
            }

            if (authenticationType.Equals(Arc4u.Standard.OAuth2.Constants.CookiesAuthenticationType))
            {
                if (!settings.Values.ContainsKey("Scopes"))
                    throw new ArgumentException("Scopes field is missing. Cannot process the request.");

                var scopes = settings.Values["Scopes"].Split(',', ';');

                var tokenAcquisition = _container.Resolve<ITokenAcquisition>();

                var result = await tokenAcquisition.GetAuthenticationResultForUserAsync(scopes);

                return new TokenInfo("Bearer", result.AccessToken, result.IdToken, result.ExpiresOn.UtcDateTime);
            }

            return null;

        }

        public void SignOut(IKeyValueSettings settings)
        {
        }

        static TokenInfo TokenIsValid(IKeyValueSettings settings, string token)
        {
            // Check we have all the data available: a token, settings and the ServiceApplicationId.
            if (null == settings)
                return null;

            if (String.IsNullOrWhiteSpace(token))
                return null;

            if (!settings.Values.ContainsKey(TokenKeys.ServiceApplicationIdKey))
                return null;

            var audience = settings.Values[TokenKeys.ServiceApplicationIdKey];

            var jwtToken = new JwtSecurityToken(token);

            if (jwtToken.Audiences.Any(a => a.StartsWith(audience, StringComparison.InvariantCultureIgnoreCase)))
            {
                return new TokenInfo("Bearer", token, String.Empty, jwtToken.ValidTo);
            }

            return null;
        }
    }
}
