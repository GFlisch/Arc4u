using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Arc4u.OAuth2.Middleware;

public class ValidateSwaggerRightMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ValidateSwaggerRightMiddlewareOption _option;

    public ValidateSwaggerRightMiddleware(RequestDelegate next, ValidateSwaggerRightMiddlewareOption option)
    {
        ArgumentNullException.ThrowIfNull(option);

        _next = next ?? throw new ArgumentNullException(nameof(next));

        _option = option;
    }

    public async Task Invoke(HttpContext context)
    {
        var applicationContext = context.RequestServices.GetRequiredService<IApplicationContext>();

        if (context.Request.Path.HasValue &&
            string.Equals(context.Request.Path.Value.Trim('/'), _option.Path.Trim('/'), StringComparison.OrdinalIgnoreCase) &&
            applicationContext.Principal is not null)
        {
            if (applicationContext.Principal.IsAuthorized(_option.Access))
            {
                await _next.Invoke(context).ConfigureAwait(false);
            }
            else
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(_option.ContentToDisplay).ConfigureAwait(false);
            }
        }
        else
        {
            await _next.Invoke(context).ConfigureAwait(false);
        }
    }

}
