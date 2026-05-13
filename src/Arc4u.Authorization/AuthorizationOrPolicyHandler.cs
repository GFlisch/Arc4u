using Arc4u.Dependency.Attribute;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

[Export(typeof(IAuthorizationHandler)), Scoped]
public sealed class AuthorizationOrPolicyHandler(IApplicationContext applicationContext)
    : AuthorizationHandler<AuthorizationOrRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationOrRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (applicationContext?.Principal is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        foreach (var operation in requirement.Permissions)
        {
            if (applicationContext.Principal.IsAuthorized(operation))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }
        context.Fail();

        return Task.CompletedTask;
    }
}
