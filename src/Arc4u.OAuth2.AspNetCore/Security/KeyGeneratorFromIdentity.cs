using System.Security.Claims;
using Arc4u.Dependency.Attribute;

namespace Arc4u.OAuth2.Security.Principal;

/// <summary>
/// This class is intented to be used in a service scenario where multiple users are connected.
/// The claims are cached based on a unique identifier from the claims in an identity.
/// </summary>
[Export(typeof(ICacheKeyGenerator)), Shared]
public class KeyGeneratorFromIdentity : ICacheKeyGenerator
{
    public KeyGeneratorFromIdentity(IUserObjectIdentifier userKeyIdentifier)
    {
        _userKeyIdentifier = userKeyIdentifier ?? throw new ArgumentNullException(nameof(userKeyIdentifier));
    }

    private readonly IUserObjectIdentifier _userKeyIdentifier;

    public string GetClaimsKey(ClaimsIdentity identity)
    {
        var id = _userKeyIdentifier.Getidentifier(identity);

        if (string.IsNullOrEmpty(id))
        {
            throw new NullReferenceException($"No distinguish key found for the identity {identity.Name}");
        }

        return id + "_ClaimsCache";
    }
}
