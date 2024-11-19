using Arc4u.IdentityModel.Claims;

namespace Arc4u.OAuth2.Security
{
    /// <summary>
    /// The IClaimsProvider is used to implement a backend service to provide extra claims.
    /// By sample, custom rights or user information based on the user id (from a db call or other).
    /// </summary>
    public interface IClaimsProvider
    {
        Task<IEnumerable<ClaimDto>> GetAsync(String userIdentifier);
    }
}
