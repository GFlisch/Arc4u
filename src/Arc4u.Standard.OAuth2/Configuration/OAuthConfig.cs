using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Claims;

namespace Arc4u.OAuth2.Configuration
{
    [Export(typeof(OAuthConfig)), Shared]
    public class OAuthConfig
    {
        public OAuthConfig(IConfiguration configuration)
        {
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



        public String GetClaimsKey(ClaimsIdentity identity)
        {
            var id = UserClaimIdentifier(identity);

            if (null == id)
            {
                Logger.Technical.From<OAuthConfig>().Error($"No claim type found equal to {String.Join(",", User.Claims)} in the current identity.").Log();
                return null;
            }

            Logger.Technical.From<OAuthConfig>().System($"Claim Type id used to identify the user is {id}.").Log();

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
