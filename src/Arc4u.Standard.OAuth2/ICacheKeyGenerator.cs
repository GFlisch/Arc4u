using System.Security.Claims;

namespace Arc4u.OAuth2;

public interface ICacheKeyGenerator
{
    string GetClaimsKey(ClaimsIdentity identity);
}
