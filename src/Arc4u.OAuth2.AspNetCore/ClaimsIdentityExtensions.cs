using System.Security.Claims;

namespace Arc4u.OAuth2;

internal static class ClaimsIdentityExtensions
{
    /// <summary>
    /// Add the claims to the identity, skipping any claim type already present on the identity.
    /// </summary>
    public static void AddMissingClaims(this ClaimsIdentity identity, IEnumerable<(string Type, string Value)> claims)
    {
        var existingTypes = identity.Claims.Select(c => c.Type).ToHashSet(StringComparer.Ordinal);
        identity.AddClaims(claims.Where(c => !existingTypes.Contains(c.Type)).Select(c => new Claim(c.Type, c.Value)));
    }
}
