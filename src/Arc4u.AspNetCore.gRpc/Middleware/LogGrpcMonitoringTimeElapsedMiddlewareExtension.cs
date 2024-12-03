using Microsoft.AspNetCore.Builder;

namespace Arc4u.AspNetCore.Middleware;

public static class LogGrpcMonitoringTimeElapsedMiddlewareExtension
{
    public static IApplicationBuilder AddGrpcMonitoringTimeElapsed(this IApplicationBuilder app, Action<Type, TimeSpan>? extraLog = null)
    {
        if (null != extraLog)
        {
            return app.UseMiddleware<LogGrpcMonitoringTimeElapsedMiddleware>(extraLog);
        }

        return app.UseMiddleware<LogGrpcMonitoringTimeElapsedMiddleware>();
    }
}
