using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Arc4u.Standard.Security
{
    /// <summary>
    /// Provides an authorization handler for operation requirements.
    /// </summary>
    public class OperationsHandler : AuthorizationHandler<OperationRequirement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationsHandler"/> class with the specified application context and logger.
        /// </summary>
        /// <param name="applicationContext">The application context to use for authorization checks.</param>
        /// <param name="logger">The logger to use for logging information about authorization checks.</param>
        public OperationsHandler(IApplicationContext applicationContext, ILogger<OperationsHandler> logger)
        {
            _applicationContext = applicationContext;
            _logger = logger;
        }

        private readonly IApplicationContext _applicationContext;
        private readonly ILogger<OperationsHandler> _logger;

        /// <summary>
        /// Makes a decision if authorization is allowed based on the requirements of the operation.
        /// </summary>
        /// <param name="context">The authorization handler context.</param>
        /// <param name="requirement">The operation requirement to be handled.</param>
        /// <returns>A task that completes when the authorization handler has finished processing.</returns>
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
