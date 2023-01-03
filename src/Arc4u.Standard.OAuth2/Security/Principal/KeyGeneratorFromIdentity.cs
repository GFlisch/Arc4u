using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Configuration;
using System.Security.Claims;

namespace Arc4u.OAuth2.Security.Principal
{
    /// <summary>
    /// This class is intented to be used in a service scenario where multiple users are connected.
    /// The claims and token are cached based on a unique identifier from the claims in an identity.
    /// The claim used is define in the OAuthConfig section.
    /// </summary>
    [Export(typeof(ICacheKeyGenerator)), Shared]
    public class KeyGeneratorFromIdentity : ICacheKeyGenerator
    {
        private readonly OAuthConfig _config;

        public KeyGeneratorFromIdentity(OAuthConfig oAuthConfig)
        {
            _config = oAuthConfig;
        }

        public string GetClaimsKey(ClaimsIdentity identity)
        {
            return _config.GetClaimsKey(identity);
        }
    }
}
