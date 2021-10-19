using System;
using System.Security.Claims;

namespace Arc4u.OAuth2
{
    public interface ICacheKeyGenerator
    {
        String GetClaimsKey(ClaimsIdentity identity);

        String UserClaimIdentifier(ClaimsIdentity claimsIdenitity);
    }
}
