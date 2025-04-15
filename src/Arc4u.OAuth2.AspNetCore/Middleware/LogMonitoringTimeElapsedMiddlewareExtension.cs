using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

namespace Arc4u.OAuth2.Middleware;

public static class LogMonitoringTimeElapsedMiddlewareExtension
{
    // ✅ AOT-compatible
    public static IApplicationBuilder AddMonitoringTimeElapsed(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<LogMonitoringTimeElapsedMiddleware>();
    }

    // ❌ Not AOT-compatible
    [RequiresDynamicCode("Using a delegate in UseMiddleware requires reflection which is not supported in AOT.")]
    public static IApplicationBuilder AddMonitoringTimeElapsed(this IApplicationBuilder app, Action<Type, TimeSpan> extraLog)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<LogMonitoringTimeElapsedMiddleware>(extraLog);
    }
}

