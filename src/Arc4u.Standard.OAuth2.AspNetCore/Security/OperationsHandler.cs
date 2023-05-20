using System.Threading.Tasks;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;

namespace Arc4u.Security
{
    public class OperationsHandler : AuthorizationHandler<OperationRequirement>
    {
        public OperationsHandler(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        private readonly IApplicationContext _applicationContext;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationRequirement requirement)
        {
            if (null == _applicationContext)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (_applicationContext.Principal.IsAuthorized(requirement.Operations))
                context.Succeed(requirement);
            else
                context.Fail();

            return Task.CompletedTask;
        }
    }
}
