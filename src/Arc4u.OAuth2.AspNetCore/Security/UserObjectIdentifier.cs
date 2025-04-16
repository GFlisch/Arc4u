using System.Security.Claims;
using Arc4u.Dependency.Attribute;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.AspNetCore;
using Arc4u.OAuth2.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Security;

[Export(typeof(IUserObjectIdentifier)), Shared]
public class UserObjectIdentifier : IUserObjectIdentifier
{
    public UserObjectIdentifier(IOptions<ClaimsIdentifierOption> identifierOptions, ILogger<UserObjectIdentifier> logger)
    {
        ArgumentNullException.ThrowIfNull(identifierOptions);

        _identifierOptions = identifierOptions.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private readonly ClaimsIdentifierOption _identifierOptions;
    private readonly ILogger<UserObjectIdentifier> _logger;

    public string? Getidentifier(ClaimsIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var id = UserClaimIdentifier(identity);

        if (string.IsNullOrEmpty(id))
        {
            _logger.Technical().LogNoClaimTypeFound(string.Join(",", _identifierOptions));
            return null;
        }

        _logger.Technical().LogClaimTypeIdFound(id);

        return id;
    }

    private string? UserClaimIdentifier(ClaimsIdentity claimsIdenitity)
    {
        var userObjectIdClaim = claimsIdenitity.Claims.FirstOrDefault(claim => _identifierOptions.Any(c => claim.Type.Equals(c, StringComparison.InvariantCultureIgnoreCase)));

        if (null != userObjectIdClaim)
        {
            return userObjectIdClaim.Value;
        }

        _logger.Technical().LogNoClaimTypeFound(string.Join(",", _identifierOptions));

        return null;
    }

}
