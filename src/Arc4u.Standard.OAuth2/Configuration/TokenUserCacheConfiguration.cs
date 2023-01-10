using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Arc4u.OAuth2.Configuration
{
    [Export(typeof(ITokenUserCacheConfiguration)), Shared]
    public class TokenUserCacheConfiguration : ITokenUserCacheConfiguration
    {
        public TokenUserCacheConfiguration(IConfiguration configuration, ILogger<TokenUserCacheConfiguration> logger)
        {
            if (configuration is null) throw new ArgumentNullException(nameof(configuration));
            if (logger is null) throw new ArgumentNullException(nameof(logger));

            User = new UserConfig();

            configuration.Bind("Application.TokenCacheConfiguration", this);

            // Add default claims to check for AzureAD!
            if (User.Claims.Count == 0)
            {
                logger.Technical().System("Retrieve user identifier based on the oid claims.").Log();
                User.Claims.Add("http://schemas.microsoft.com/identity/claims/objectidentifier");
                User.Claims.Add("oid");
            }
        }

        public UserConfig User { get; set; }
    }
}