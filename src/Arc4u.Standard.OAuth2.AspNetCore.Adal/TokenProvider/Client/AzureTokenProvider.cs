using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Token.Adal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Arc4u.Dependency.Attribute;
using System;

namespace Arc4u.OAuth2.TokenProvider.Client
{
    [Export(AzureTokenProvider.ProviderName, typeof(ITokenProvider))]
    public class AzureTokenProvider : AdalTokenProvider
    {
        public const string ProviderName = "azureAD";

        
        public AzureTokenProvider(ILogger<AzureTokenProvider> logger, IServiceProvider container) : base(logger, container)
        {
        }

        protected override AuthenticationContext CreateAuthenticationContext(string authority, string cacheIdentifier)
        {
            return new AuthenticationContext(authority, new Cache(_logger, Container, cacheIdentifier));
        }
    }
}
