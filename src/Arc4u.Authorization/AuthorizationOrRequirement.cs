using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

/// <summary>
/// An authorization requirement that succeeds if <b>any</b> of the specified permissions are present
/// on the current principal (OR logic).
/// </summary>
/// <remarks>
/// Three constructor overloads allow flexible permission specification:
/// <list type="bullet">
///   <item>Scope-less: permissions are matched against the default (empty) scope.</item>
///   <item>Single scope: all permissions share the same scope.</item>
///   <item>Mixed: each permission carries its own scope.</item>
/// </list>
/// </remarks>
public class AuthorizationOrRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance using the default (empty) scope for every permission.
    /// </summary>
    /// <param name="permissions">One or more permission identifiers.</param>
    public AuthorizationOrRequirement(params int[] permissions)
    {
        Permissions = permissions.Select(p => (string.Empty, p)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance where all permissions share a single <paramref name="scope"/>.
    /// </summary>
    /// <param name="scope">The scope applied to every permission.</param>
    /// <param name="permissions">One or more permission identifiers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="scope"/> is <c>null</c>.</exception>
    public AuthorizationOrRequirement(string scope, params int[] permissions)
    {
        ArgumentNullException.ThrowIfNull(scope);

        Permissions = permissions.Select(p => (scope, p)).ToArray();
    }

    /// <summary>
    /// Initializes a new instance with mixed scope/permission pairs,
    /// allowing each permission to belong to a different scope.
    /// </summary>
    /// <param name="permissions">One or more (scope, permission) tuples.</param>
    public AuthorizationOrRequirement(params (string, int)[] permissions)
    {
        Permissions = permissions;
    }

    /// <summary>
    /// Gets the set of (scope, permission) pairs where at least <b>one</b> must be satisfied.
    /// </summary>
    public (string scope, int permission)[] Permissions { get; private set; }

}
