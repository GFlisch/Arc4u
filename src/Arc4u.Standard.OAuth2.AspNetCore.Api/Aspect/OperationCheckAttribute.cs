using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Arc4u.OAuth2.Aspect
{
    [Obsolete("Use ManageExceptionsFilter and SetCultureActionFilter instead.")]
    public class OperationCheckAttribute : ServiceAspectBase
    {
        public OperationCheckAttribute(ILogger logger, IHttpContextAccessor httpContextAccessor, String scope, params int[] operations) : base(logger, httpContextAccessor, scope, operations)
        {
        }

        public override void SetCultureInfo(ActionExecutingContext context)
        {
            var principal = _httpContextAccessor.HttpContext.User as Arc4u.Security.Principal.AppPrincipal;

            if (null != principal && null != principal.Profile)
            {
                Thread.CurrentThread.CurrentUICulture = principal.Profile.CurrentCulture;

                if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
                {
                    Logger.Technical().From(descriptor.MethodInfo.DeclaringType, descriptor.MethodInfo.Name).System($"Thread UI Culture is set to {principal.Profile.CurrentCulture.Name}").Log();
                }
            }
        }
    }
}
