using System.Security.Claims;

namespace Arc4u.OAuth2.Security;

public interface IUserObjectIdentifier
{
    public string? GetIdentifer(ClaimsIdentity identity);
}
