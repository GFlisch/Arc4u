﻿using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Configuration;
using Arc4u.OAuth2.Token;
using Arc4u.OAuth2.Token.Adal;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;

namespace Arc4u.OAuth2.TokenProvider
{
    [Export(AdalTokenProvider.ProviderName, typeof(ITokenProvider))]
    class AzureADTokenProvider : AdalTokenProvider
    {
        
        public AzureADTokenProvider(OAuthConfig oAuthConfig, ILogger<AzureADTokenProvider> logger, IServiceProvider container) : base(oAuthConfig, logger, container) { }


        protected override AuthenticationContext CreateAuthenticationContext(string authority, string cacheIdentifier)
        {
            return new AuthenticationContext(authority, true, new Cache(_logger, Container, cacheIdentifier));
        }
    }
}
