using System.Diagnostics;
using Arc4u.Diagnostics;
using Arc4u.Diagnostics.Monitoring;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Middleware;

public class LogMonitoringTimeElapsedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Action<Type, TimeSpan>? _log;

    public LogMonitoringTimeElapsedMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _log = null;
    }

    public LogMonitoringTimeElapsedMiddleware(RequestDelegate next, Action<Type, TimeSpan> extraLog)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _log = extraLog;
    }

    public async Task InvokeAsync(HttpContext context, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(context);

        var startingTimestamp = Stopwatch.GetTimestamp();

        await _next(context).ConfigureAwait(false);

        var elapsed = Stopwatch.GetElapsedTime(startingTimestamp);

        try
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (descriptor != null && descriptor.MethodInfo.DeclaringType is not null)
                {               
                    logger.Monitoring(descriptor.MethodInfo.DeclaringType, descriptor.MethodInfo.Name)
                           .Add("Elapsed", elapsed.TotalMilliseconds)
                           .Add("StatusCode", context.Response.StatusCode)
                           .LogTimeToCompleteCall();

                    _log?.Invoke(descriptor.MethodInfo.DeclaringType, elapsed);
                }
            }

        }
        catch (Exception ex)
        {
            logger.Technical<LogMonitoringTimeElapsedMiddleware>().LogException(ex);
        }
    }
}
