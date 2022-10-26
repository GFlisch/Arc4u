using Arc4u.Dependency;
using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public static class ClaimsPrincipalMiddlewareExtension
    {
        public static IApplicationBuilder UseCreatePrincipal(this IApplicationBuilder app, IServiceProvider container, ClaimsPrincipalMiddlewareOption option)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            if (null == container)
                throw new ArgumentNullException(nameof(container));

            if (null == option)
                throw new ArgumentNullException(nameof(option));


            return app.UseMiddleware<ClaimsPrincipalMiddleware>(container, option);
        }
    }
}
