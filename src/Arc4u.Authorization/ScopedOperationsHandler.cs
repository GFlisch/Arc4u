using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Authorization;

public class ScopedOperationsHandler : AuthorizationHandler<ScopedOperationsRequirement>
{
    public ScopedOperationsHandler(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    private readonly IApplicationContext _applicationContext;

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopedOperationsRequirement requirement)
    {
        // on Blazor WASM, IApplicationContext.Principal can be null
        if (_applicationContext?.Principal is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (_applicationContext.Principal.IsAuthorized(requirement.Scope, requirement.Operations))
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
