using System;
using Microsoft.AspNetCore.Builder;

namespace Arc4u.OAuth2.Middleware
{
    public static class OpenIdCookieValidityCheckMiddlewareExtension
    {
        public static IApplicationBuilder UseOpenIdCookieValidityCheck(this IApplicationBuilder app, OpenIdCookieValidityCheckOptions options)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            if (null == options)
                throw new ArgumentNullException(nameof(options));

            return app.UseMiddleware<OpenIdCookieValidityCheckMiddleware>(options);
        }
    }
}
