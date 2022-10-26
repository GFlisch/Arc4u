using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Token.Adal;
using Arc4u.Security.Principal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(AdalOboTokenProvider.ProviderName, typeof(ITokenProvider))]
    public class AdfsOboTokenProvider : AdalOboTokenProvider
    {
        
        public AdfsOboTokenProvider(OAuthConfig oAuthConfig, ILogger<AdfsOboTokenProvider> logger, IServiceProvider container, IApplicationContext applicationContext) : base(oAuthConfig, logger, container, applicationContext) { }

        protected override AuthenticationContext CreateAuthenticationContext(string authority, string cacheIdentifier)
        {
            return new AuthenticationContext(authority, false, new Cache(_logger, Container, cacheIdentifier));
        }
    }
}