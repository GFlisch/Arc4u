using Arc4u.Dependency.Attribute;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

/// <summary>
/// Handles <see cref="ScopedOperationsRequirement"/> by verifying that the current principal
/// possesses <b>all</b> of the required scoped permissions (AND logic).
/// </summary>
/// <remarks>
/// Registered as a scoped <see cref="IAuthorizationHandler"/> via dependency injection.
/// If the principal is <c>null</c> (e.g. on Blazor WASM before authentication), the requirement fails immediately.
/// </remarks>
[Export(typeof(IAuthorizationHandler)), Scoped]
public sealed class ScopedOperationsHandler(IApplicationContext applicationContext)
    : AuthorizationHandler<ScopedOperationsRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopedOperationsRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (applicationContext.Principal is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        foreach (var operation in requirement.Permissions)
        {
            if (!applicationContext.Principal.IsAuthorized(operation.scope, operation.permission))
            {
                context.Fail();
                return Task.CompletedTask;
            }
        }
        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
