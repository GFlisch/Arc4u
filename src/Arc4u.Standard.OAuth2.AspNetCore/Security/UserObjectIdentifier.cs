using System;
using System.Linq;
using System.Security.Claims;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Security;

[Export(typeof(IUserObjectIdentifier)), Shared]
public class UserObjectIdentifier : IUserObjectIdentifier
{
    public UserObjectIdentifier(IOptions<ClaimsIdentifierOption> identifierOptions, ILogger<UserObjectIdentifier> logger)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(identifierOptions);
#else
        if (identifierOptions is null)
        {
            throw new ArgumentNullException(nameof(identifierOptions));
        }
#endif

        _identifierOptions = identifierOptions.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private readonly ClaimsIdentifierOption _identifierOptions;
    private readonly ILogger<UserObjectIdentifier> _logger;

    public string? GetIdentifer(ClaimsIdentity identity)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(identity);
#else
        if (identity is null)
        {
            throw new ArgumentNullException(nameof(identity));
        }
#endif
        var id = UserClaimIdentifier(identity);

        if (string.IsNullOrEmpty(id))
        {
            _logger.Technical().LogError($"No claim type found equal to {string.Join(",", _identifierOptions)} in the current identity.");
            return null;
        }

        _logger.Technical().Debug($"Claim Type id used to identify the user is {id}.").Log();

        return id;
    }

    private string? UserClaimIdentifier(ClaimsIdentity claimsIdenitity)
    {
        var userObjectIdClaim = claimsIdenitity.Claims.FirstOrDefault(claim => _identifierOptions.Any(c => claim.Type.Equals(c, StringComparison.InvariantCultureIgnoreCase)));

        if (null != userObjectIdClaim)
        {
            return userObjectIdClaim.Value;
        }

        _logger.Technical().LogError($"No claims found with one of the keys: [{string.Join(",", _identifierOptions)}]");

        return null;
    }


}
