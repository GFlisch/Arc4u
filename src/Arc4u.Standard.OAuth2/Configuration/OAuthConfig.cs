using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Arc4u.OAuth2.Configuration
{
    [Export, Shared]
    public class OAuthConfig
    {
        public string UserIdentifier { get; }

        private readonly HashSet<string> _identityClaimNames;
        private readonly ILogger<OAuthConfig> _logger;

        public OAuthConfig(IConfiguration configuration, ILogger<OAuthConfig> logger)
        {
            _logger = logger;

            var userConfig = new UserConfig();
            configuration.Bind("Application.TokenCacheConfiguration:User", userConfig);

            UserIdentifier = userConfig.Identifier;

            _identityClaimNames = new HashSet<string>(userConfig.Claims.Select(claimName => claimName.ToLowerInvariant()).Distinct());

            // Add default claims to check for AzureAD!
            if (_identityClaimNames.Count == 0)
            {
                _identityClaimNames.Add("http://schemas.microsoft.com/identity/claims/objectidentifier");
                _identityClaimNames.Add("oid");
            }
        }

        public String GetClaimsKey(ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity == null)
            {
                _logger.Technical().LogError($"Specified identity is null");
                return null;
            }

            var id = ExtractUserClaimIdentifier(claimsIdentity);
            if (id == null)
            {
                _logger.Technical().LogError($"No claim type found equal to {String.Join(",", _identityClaimNames)} in the specified identity.");
                return null;
            }

            _logger.Technical().System($"Claim Type id used to identify the user is {id}.").Log();

            return id.ToLowerInvariant() + "_ClaimsCache";
        }

        private string ExtractUserClaimIdentifier(ClaimsIdentity claimsIdentity)
        {
            var identityClaim = claimsIdentity.Claims.FirstOrDefault(claim => _identityClaimNames.Contains(claim.Type.ToLowerInvariant()));
            return identityClaim?.Value;
        }
    }
}
