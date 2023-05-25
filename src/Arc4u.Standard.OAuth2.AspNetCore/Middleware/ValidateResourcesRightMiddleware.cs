using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using Arc4u.Security.Principal;
using Arc4u.OAuth2.Options;
using System.Linq;

namespace Arc4u.OAuth2.Middleware;
public class ValidateResourcesRightMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ValidateResourcesRightMiddlewareOptions _options;

    public ValidateResourcesRightMiddleware(RequestDelegate next, ValidateResourcesRightMiddlewareOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _next = next ?? throw new ArgumentNullException(nameof(next));

        _options = options;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.HasValue)
        {
            await _next.Invoke(context).ConfigureAwait(false);
            return;
        }

        var applicationContext = context.RequestServices.GetRequiredService<IApplicationContext>();
        var authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();

        var options = _options.ResourcesPolicies.Where(o => string.Equals(context.Request.Path.Value.Trim('/'), o.Value.Path.Trim('/'), StringComparison.OrdinalIgnoreCase)).Select(o => o.Value);

        if (!options.Any())
        {
            await _next.Invoke(context).ConfigureAwait(false);
            return;
        }

        // check if the user has the right to access the resources.
        foreach (var option in options)
        {
            var authResult = await authorizationService.AuthorizeAsync(applicationContext.Principal, option.AuthorizationPolicy).ConfigureAwait(false);

            if (!authResult.Succeeded)
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(option.ContentToDisplay).ConfigureAwait(false);
                return;
            }
        }
        await _next.Invoke(context).ConfigureAwait(false);
    }
}
