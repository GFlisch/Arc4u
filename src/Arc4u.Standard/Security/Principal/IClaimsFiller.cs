using Arc4u.IdentityModel.Claims;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Arc4u.Security.Principal
{
    public interface IClaimsFiller
    {
        Task<IEnumerable<ClaimDto>> GetAsync(IIdentity identity, IEnumerable<IKeyValueSettings> settings, object paramter);
    }
}
