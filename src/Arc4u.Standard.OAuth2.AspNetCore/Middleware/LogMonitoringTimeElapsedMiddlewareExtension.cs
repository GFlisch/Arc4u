using System;
using Microsoft.AspNetCore.Builder;

namespace Arc4u.OAuth2.Middleware
{
    public static class LogMonitoringTimeElapsedMiddlewareExtension
    {
        public static IApplicationBuilder AddMonitoringTimeElapsed(this IApplicationBuilder app, Action<Type, TimeSpan> extraLog = null)
        {
            if (null == app)
                throw new ArgumentNullException(nameof(app));

            if (null != extraLog)
                return app.UseMiddleware<LogMonitoringTimeElapsedMiddleware>(extraLog);

            return app.UseMiddleware<LogMonitoringTimeElapsedMiddleware>();
        }
    }
}
