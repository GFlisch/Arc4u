using Arc4u.Dependency.Attribute;
using Arc4u.OAuth2.Configuration;
using System;
using System.Security.Claims;

namespace Arc4u.OAuth2.Security.Principal;

/// <summary>
/// This class is intented to be used in a service scenario where multiple users are connected.
/// The claims are cached based on a unique identifier from the claims in an identity.
/// The claim used is define in the <see cref="ITokenUserCacheConfiguration"/> implementation.
/// </summary>
[Export(typeof(ICacheKeyGenerator)), Shared]
public class KeyGeneratorFromIdentity2 : ICacheKeyGenerator
{
    public KeyGeneratorFromIdentity2(IUserObjectIdentifier userKeyIdentifier)
    {
        _userKeyIdentifier = userKeyIdentifier ?? throw new ArgumentNullException(nameof(userKeyIdentifier));
    }

    private readonly IUserObjectIdentifier _userKeyIdentifier;


    public string GetClaimsKey(ClaimsIdentity identity)
    {
        var id = _userKeyIdentifier.GetIdentifer(identity);

        if (String.IsNullOrEmpty(id))
        {
            throw new NullReferenceException($"No distinguish key found for the identity {identity.Name}");
        }

        return _userKeyIdentifier.GetIdentifer(identity) + "_ClaimsCache";
    }
}