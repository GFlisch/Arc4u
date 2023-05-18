using Microsoft.AspNetCore.Builder;
using System;

namespace Arc4u.AspNetCore.Middleware
{
    public static class GrpcAuthenticationStatusCodeMiddlewareExtension
    {
        public static IApplicationBuilder AddGrpcAuthenticationControl(this IApplicationBuilder app)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<GrpcAuthenticationStatusCodeMiddleware>();
        }
    }
}
