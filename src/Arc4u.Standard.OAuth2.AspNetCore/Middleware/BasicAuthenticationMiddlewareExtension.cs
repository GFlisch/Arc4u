using Arc4u.Dependency;
using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.Standard.OAuth2.Middleware;

public static class BasicAuthenticationMiddlewareExtension
{
    public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, IContainerResolve container, BasicAuthenticationContextOption option)
    {
        ArgumentNullException.ThrowIfNull(app);

        ArgumentNullException.ThrowIfNull(option);

        ArgumentNullException.ThrowIfNull(container);

        return app.UseMiddleware<BasicAuthenticationMiddleware>(option);
    }
}
