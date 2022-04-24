using Arc4u.Dependency.Attribute;
using System.Security.Claims;

namespace Arc4u.OAuth2.Security.Principal
{
    /// <summary>
    /// This class is used with UI application which is user based already (by design).
    /// So the key to used can be simple as a fix string.
    /// </summary>
    [Export(typeof(ICacheKeyGenerator)), Shared]
    public class FixKeyGenerator : ICacheKeyGenerator
    {
        public string GetClaimsKey(ClaimsIdentity identity)
        {
            return "ClaimsVaultRef";
        }

        public string UserClaimIdentifier(ClaimsIdentity claimsIdenitity)
        {
            throw new System.NotImplementedException();
        }
    }
}
