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
                _logger.Technical().From(descriptor.MethodInfo.DeclaringType, descriptor.MethodInfo.Name).Debug($"Thread UI Culture is set to {_application.Principal.Profile.CurrentCulture.Name}").Log();
            }
        }

        await next().ConfigureAwait(false);
    }
}
