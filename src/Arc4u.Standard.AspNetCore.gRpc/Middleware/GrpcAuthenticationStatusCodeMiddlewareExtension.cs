using Microsoft.AspNetCore.Builder;

namespace Arc4u.AspNetCore.Middleware;

public static class GrpcAuthenticationStatusCodeMiddlewareExtension
{
    public static IApplicationBuilder AddGrpcAuthenticationControl(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<GrpcAuthenticationStatusCodeMiddleware>();
    }
}
