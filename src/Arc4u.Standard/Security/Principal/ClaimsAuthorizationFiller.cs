using System.Runtime.Serialization.Json;
using System.Security.Claims;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.Security.Principal;

[Export(typeof(IClaimAuthorizationFiller)), Shared]
public class ClaimsAuthorizationFiller : IClaimAuthorizationFiller
{
    public ClaimsAuthorizationFiller(ILogger<ClaimsAuthorizationFiller> logger)
    {
        _logger = logger;
    }

    private readonly ILogger<ClaimsAuthorizationFiller> _logger;

    public Authorization GetAuthorization(System.Security.Principal.IIdentity identity)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(identity);
#else
        if (null == identity)
        {
            throw new ArgumentNullException("identity");
        }
#endif
        if (!(identity is ClaimsIdentity))
        {
            throw new NotSupportedException("Only identity from ClaimsIdentity are allowed.");
        }

        var claimsIdentity = (ClaimsIdentity)identity;

        // Create a UserProfile based on the identity received.
        var claimAuthorization = ExtractClaimValue(IdentityModel.Claims.ClaimTypes.Authorization, claimsIdentity.Claims);

        if (!string.IsNullOrWhiteSpace(claimAuthorization))
        {
            return GetAuthorization(claimAuthorization) ?? new Authorization();
        }

        return new Authorization();

    }

    private Authorization? GetAuthorization(string claimAuthorization)
    {
        try
        {
            var serializer = new DataContractJsonSerializer(typeof(Authorization));
            return serializer.ReadObject<Authorization>(claimAuthorization);
        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
        }

        return new Authorization();
    }

    private static string ExtractClaimValue(string claimType, IEnumerable<Claim> claims)
    {
        var claim = claims.SingleOrDefault(c => c.Type.Equals(claimType, StringComparison.CurrentCultureIgnoreCase));
        try
        {
            if (null != claim)
            {
                return claim.Value ?? string.Empty;
            }

            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

}
