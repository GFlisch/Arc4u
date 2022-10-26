using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public static class BasicAuthenticationMiddlewareExtension
    {
        public static IApplicationBuilder UseBasicAuthentication(this IApplicationBuilder app, IServiceProvider container, BasicAuthenticationContextOption option)
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
