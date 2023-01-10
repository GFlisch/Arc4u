using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;

namespace Arc4u.OAuth2.Security;

[Export(typeof(IUserObjectIdentifier)), Shared]
public class UserKeyIdentifier : IUserObjectIdentifier
{
    public UserKeyIdentifier(ITokenUserCacheConfiguration userCacheConfiguration, ILogger<UserKeyIdentifier> logger)
    {
        _userCacheConfiguration = userCacheConfiguration ?? throw new ArgumentNullException(nameof(userCacheConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private readonly ITokenUserCacheConfiguration _userCacheConfiguration;
    private readonly ILogger<UserKeyIdentifier> _logger;

    public string GetIdentifer(ClaimsIdentity identity)
    {
        if (identity is null) throw new ArgumentNullException(nameof(identity));

        var id = UserClaimIdentifier(identity);

        if (String.IsNullOrEmpty(id))
        {
            _logger.Technical().LogError($"No claim type found equal to {String.Join(",", _userCacheConfiguration.User.Claims)} in the current identity.");
            return null;
        }

        _logger.Technical().Debug($"Claim Type id used to identify the user is {id}.").Log();

        return id;
    }

    private string UserClaimIdentifier(ClaimsIdentity claimsIdenitity)
    {
        var userObjectIdClaim = claimsIdenitity.Claims.FirstOrDefault(claim => _userCacheConfiguration.User.Claims.Any(c => claim.Type.Equals(c, StringComparison.InvariantCultureIgnoreCase)));

        if (null != userObjectIdClaim) return userObjectIdClaim.Value;

        _logger.Technical().LogError($"No claims found with one of the keys: [{String.Join(',', _userCacheConfiguration.User.Claims)}]");

        return null;
    }
}
