using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

    [RequiresDynamicCode("Passing a delegate is not compatible with Native AOT.")]
    public LogMonitoringTimeElapsedMiddleware(RequestDelegate next, Action<Type, TimeSpan> extraLog)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _log = extraLog;
    }

    public async Task InvokeAsync(HttpContext context, ILogger logger)
    {
        var start = Stopwatch.GetTimestamp();
        await _next(context).ConfigureAwait(false);
        var elapsed = Stopwatch.GetElapsedTime(start);

        try
        {
            var endpoint = context.GetEndpoint();
            var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (descriptor?.MethodInfo.DeclaringType is { } type)
            {
                logger.Monitoring(type, descriptor.MethodInfo.Name)
                      .Add("Elapsed", elapsed.TotalMilliseconds)
                      .Add("StatusCode", context.Response.StatusCode)
                      .LogTimeToCompleteCall();

                // ✅ Inline safe call — no method marked with [RequiresDynamicCode]
                _log?.Invoke(type, elapsed);
            }
        }
        catch (Exception ex)
        {
            logger.Technical<LogMonitoringTimeElapsedMiddleware>().LogException(ex);
        }
    }
}
