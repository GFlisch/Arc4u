using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public static class OpenIdBearerInjectorMiddlewareExtension
    {
        public static IApplicationBuilder UseOpenIdBearerInjector(this IApplicationBuilder app, OpenIdBearerInjectorOptions options)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            if (null == options)
                throw new ArgumentNullException(nameof(options));

            return app.UseMiddleware<OpenIdBearerInjectorMiddleware>(options);
        }
    }
}
