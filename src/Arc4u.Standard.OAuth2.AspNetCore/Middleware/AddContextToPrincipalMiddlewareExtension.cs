using System;
using Microsoft.AspNetCore.Builder;

namespace Arc4u.OAuth2.Middleware;
public static class AddContextToPrincipalMiddlewareExtension
{
    public static IApplicationBuilder UseAddContextToPrincipal(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<AddContextToPrincipalMiddleware>(app.ApplicationServices);
    }
}
