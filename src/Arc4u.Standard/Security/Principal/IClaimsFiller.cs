using System.Security.Principal;
using Arc4u.IdentityModel.Claims;

namespace Arc4u.Security.Principal;

public interface IClaimsFiller
{
    Task<IEnumerable<ClaimDto>> GetAsync(IIdentity identity, IEnumerable<IKeyValueSettings> settings, object? parameter);
}
