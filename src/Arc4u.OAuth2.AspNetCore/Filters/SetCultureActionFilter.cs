using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.AspNetCore.Filters;
public class SetCultureActionFilter : IAsyncActionFilter
{
    public SetCultureActionFilter(ILogger logger, IApplicationContext application)
    {
        _logger = logger;
        _application = application;
    }

    private readonly ILogger _logger;
    private readonly IApplicationContext _application;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_application?.Principal?.Profile is not null)
        {
            Thread.CurrentThread.CurrentUICulture = _application.Principal.Profile.CurrentCulture;

            if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
            {
                if (null != descriptor.MethodInfo?.DeclaringType)
                {
                    _logger.Technical(descriptor.MethodInfo?.DeclaringType!, descriptor.MethodInfo?.Name ?? "MethodInfo Name")
                           .LogThreadCultureName(_application.Principal.Profile.CurrentCulture.Name);
                }
            }
        }

        await next().ConfigureAwait(false);
    }
}
