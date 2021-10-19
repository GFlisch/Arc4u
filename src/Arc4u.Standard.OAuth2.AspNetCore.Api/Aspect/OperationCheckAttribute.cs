using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Arc4u.OAuth2.Aspect
{
    public class OperationCheckAttribute : ServiceAspectBase
    {
        public OperationCheckAttribute(ILogger logger, IApplicationContext applicationContext, String scope, params int[] operations) : base(logger, applicationContext, scope, operations)
        {
        }

        public override void SetCultureInfo(ActionExecutingContext context)
        {
            if (null != ApplicationContext.Principal && null != ApplicationContext.Principal.Profile)
            {
                Thread.CurrentThread.CurrentUICulture = ApplicationContext.Principal.Profile.CurrentCulture;

                if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
                {
                    Logger.Technical().From(descriptor.MethodInfo.DeclaringType, descriptor.MethodInfo.Name).System($"Thread UI Culture is set to {ApplicationContext.Principal.Profile.CurrentCulture.Name}").Log();
                }
            }
        }
    }
}
