using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

/// <summary>
/// An authorization requirement that succeeds if any of the specified permissions are present.
/// </summary>
public class AuthorizationOrRequirement : IAuthorizationRequirement
{
    public AuthorizationOrRequirement(params int[] permissions)
    {
        Permissions = permissions;
    }

    public int[] Permissions { get; private set; }
}
