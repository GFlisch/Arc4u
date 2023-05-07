using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.Standard.OAuth2.Middleware;

public static class OpenIdBearerInjectorMiddlewareExtension
{
    public static IApplicationBuilder UseOpenIdBearerInjector(this IApplicationBuilder app, OpenIdBearerInjectorOptions options)
    {
        ArgumentNullException.ThrowIfNull(app);

        ArgumentNullException.ThrowIfNull(options);

        return app.UseMiddleware<OpenIdBearerInjectorMiddleware>(options);
    }
}
