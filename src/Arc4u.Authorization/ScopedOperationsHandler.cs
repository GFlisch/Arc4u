using Arc4u.Dependency.Attribute;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

[Export(typeof(IAuthorizationHandler)), Scoped]
public class ScopedOperationsHandler(IApplicationContext applicationContext)
    : AuthorizationHandler<ScopedOperationsRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopedOperationsRequirement requirement)
    {
        // on Blazor WASM, IApplicationContext.Principal can be null
        if (applicationContext?.Principal is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (applicationContext.Principal.IsAuthorized(requirement.Scope, requirement.Operations))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
