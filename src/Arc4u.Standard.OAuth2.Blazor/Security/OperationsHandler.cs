using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Arc4u.Standard.Security
{
    public class OperationsHandler : AuthorizationHandler<OperationRequirement>
    {
        public OperationsHandler(IApplicationContext applicationContext, ILogger<OperationsHandler> logger)
        {
            _applicationContext = applicationContext;
            _logger = logger;
        }

        private readonly IApplicationContext _applicationContext;
        private readonly ILogger<OperationsHandler> _logger;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationRequirement requirement)
        {
            if (null == _applicationContext.Principal)
            {
                _logger.Technical().Debug("Application context is null").Log();
                context.Fail();
                return Task.CompletedTask;
            }

            if (_applicationContext.Principal.IsAuthorized(requirement.Operations))
            {
                _logger.Technical().Debug("Is authorized").Log();
                context.Succeed(requirement);
            }
            else
            {
                _logger.Technical().Debug("Is not authorized").Log();
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
