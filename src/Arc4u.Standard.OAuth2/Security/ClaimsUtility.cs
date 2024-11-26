#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Globalization;
using System.Security.Claims;
using Arc4u.IdentityModel.Claims;

namespace Arc4u.Standard.OAuth2.Security;

/// <summary>
/// Claims utilities and extension methods, used in AppPrincipalFactory and AppPrincipalTransform.
/// </summary>
/// <remarks>
/// Claim type comparison is always case-sensitive. See https://www.rfc-editor.org/rfc/rfc7519 section 10.1.
/// </remarks>
public static class ClaimsUtility
{
    private const string ExpirationClaimType = "exp";
    private static readonly HashSet<string> ClaimsToExclude = ["aud", "iss", "iat", "nbf", "acr", "aio", "appidacr", "ipaddr", "scp", "sub", "tid", "uti", "unique_name", "apptype", "appid", "ver", "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant", "http://schemas.microsoft.com/identity/claims/scope"];

    /// <summary>
    /// Sanitize the list of external claims by removing the claims we don't need.
    /// The <paramref name="claims"/> may come from an external system. If there is an "exp" claim in the list, it will be included.
    /// </summary>
    /// <param name="claims"></param>
    /// <returns></returns>
    public static IEnumerable<ClaimDto> Sanitize(this IEnumerable<ClaimDto> claims)
    {
        foreach (var claim in claims)
        {
            if (ClaimsToExclude.Contains(claim.ClaimType))
            {
                continue;
            }
            yield return claim;
        }
    }

    /// <summary>
    /// Sanitize the list of internal claims by removing the claims we don't need. 
    /// The <paramref name="claims"/> come from the token claims . If there is an "exp" claim in the list, it will be *excluded* as well
    /// since it may be replaced by the equivalent external claim.
    /// </summary>
    /// <param name="claims"></param>
    /// <returns></returns>
    public static IEnumerable<Claim> Sanitize(this IEnumerable<Claim> claims)
    {
        foreach (var claim in claims)
        {
            if (ClaimsToExclude.Contains(claim.Type) || claim.Type == ExpirationClaimType)
            {
                continue;
            }
            yield return claim;
        }
    }

    /// <summary>
    /// Merge the <paramref name="claimsToMerge"/> into the <paramref name="claimsIdentity"/>, but only the claims that are not already present.
    /// The "exp" claim is always excluded from the merge.
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <param name="claimsToMerge"></param>
    public static void MergeClaims(this ClaimsIdentity claimsIdentity, IEnumerable<ClaimDto> claimsToMerge)
    {
        claimsIdentity.AddClaims(claimsToMerge.Where(c => c.ClaimType != ExpirationClaimType && !claimsIdentity.HasClaim(c1 => c1.Type == c.ClaimType)).Select(c => new Claim(c.ClaimType, c.Value)));
    }

    /// <summary>
    /// Merge the <paramref name="claimsToMerge"/> into the <paramref name="claimsIdentity"/>, but only the claims that are not already present.
    /// The "exp" claim is always excluded from the merge.
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <param name="claimsToMerge"></param>
    public static void MergeClaims(this ClaimsIdentity claimsIdentity, IEnumerable<Claim> claimsToMerge)
    {
        claimsIdentity.AddClaims(claimsToMerge.Where(c => c.Type != ExpirationClaimType && !claimsIdentity.HasClaim(c1 => c1.Type == c.Type)).Select(c => new Claim(c.Type, c.Value)));
    }

#if NET6_0_OR_GREATER
    public static bool TryGetExpiration(this IEnumerable<Claim> claims, [NotNullWhen(true)] out Claim? expClaim)
#else
    public static bool TryGetExpiration(this IEnumerable<Claim> claims, out Claim expClaim)
#endif
    {
        expClaim = claims.FirstOrDefault(c => c.Type == ExpirationClaimType);
        return expClaim is not null;
    }

    public static bool TryGetExpiration(this IEnumerable<Claim> claims, out long ticks)
    {
        var claim = claims.FirstOrDefault(c => c.Type == ExpirationClaimType);

        if (claim is null)
        {
            ticks = 0;
            return false;
        }

        return claim.Value.TryParseTicks(out ticks);
    }

    public static bool TryGetExpiration(this IEnumerable<ClaimDto> claims, out long ticks)
    {
        var claim = claims.FirstOrDefault(c => c.ClaimType == ExpirationClaimType);

        if (claim is null)
        {
            ticks = 0;
            return false;
        }

        return claim.Value.TryParseTicks(out ticks);
    }

    public static bool TryParseTicks(this string s, out long ticks) => long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks);
}
