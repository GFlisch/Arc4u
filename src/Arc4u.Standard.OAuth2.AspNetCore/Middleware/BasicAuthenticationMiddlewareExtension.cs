using Arc4u.Dependency;
using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public static class BasicAuthenticationMiddlewareExtension
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, IContainerResolve container, BasicAuthenticationContextOption option)
        {
            if (null == app)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (null == option)
                throw new ArgumentNullException(nameof(option));

            return app.UseMiddleware<BasicAuthenticationMiddleware>(option);
        }
    }
}
