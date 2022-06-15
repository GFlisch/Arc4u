using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;

namespace Arc4u.OAuth2.Configuration
{
    [Export(typeof(OAuthConfig)), Shared]
    public class OAuthConfig
    {
        public OAuthConfig(IConfiguration configuration, ILogger<OAuthConfig> logger)
        {
            _logger = logger;
            
            User = new UserConfig();

            configuration.Bind("Application.TokenCacheConfiguration", this);

            // Add default claims to check for AzureAD!
            if (User.Claims.Count == 0)
            {
                User.Claims.Add("http://schemas.microsoft.com/identity/claims/objectidentifier");
                User.Claims.Add("oid");
            }
        }


        public UserConfig User { get; set; }

        private readonly ILogger<OAuthConfig> _logger;

        public String GetClaimsKey(ClaimsIdentity identity)
        {
            var id = UserClaimIdentifier(identity);

            if (null == id)
            {
                _logger.Technical().LogError($"No claim type found equal to {String.Join(",", User.Claims)} in the current identity.");
                return null;
            }

            _logger.Technical().System($"Claim Type id used to identify the user is {id}.").Log();

            return GetClaimsKey(id);
        }

        public static String GetClaimsKey(string id)
        {
            return id.ToLowerInvariant() + "_ClaimsCache";
        }

        public string UserClaimIdentifier(ClaimsIdentity claimsIdenitity)
        {

            if (null == claimsIdenitity) return null;

            var userObjectIdClaim = claimsIdenitity.Claims.FirstOrDefault(claim => User.Claims.Any(c => claim.Type.Equals(c, StringComparison.InvariantCultureIgnoreCase)));

            if (null != userObjectIdClaim) return userObjectIdClaim.Value;

            return null;
        }
    }
}
