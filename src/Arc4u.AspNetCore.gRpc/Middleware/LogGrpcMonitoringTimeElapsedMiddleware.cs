using System.Diagnostics;
using Arc4u.Diagnostics.Monitoring;
using Arc4u.Diagnostics;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Arc4u.AspNetCore.Middleware;

public class LogGrpcMonitoringTimeElapsedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Action<Type, TimeSpan>? _log;

    public LogGrpcMonitoringTimeElapsedMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _log = null;
    }

    public LogGrpcMonitoringTimeElapsedMiddleware(RequestDelegate next, Action<Type, TimeSpan> extraLog)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _log = extraLog;
    }

    public async Task Invoke(HttpContext context, ILogger logger)
    {
        var startingTimestamp = Stopwatch.GetTimestamp();

        await _next(context).ConfigureAwait(false);

        var elapsed = Stopwatch.GetElapsedTime(startingTimestamp);

        try
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var descriptor = endpoint.Metadata.GetMetadata<GrpcMethodMetadata>();
                if (descriptor != null)
                {
                    logger.Technical(descriptor.ServiceType, descriptor.Method.Name)
                           .Add("Elapsed", elapsed.TotalMilliseconds)
                           .Add("StatusCode", context.Response.StatusCode)
                           .LogTimeToCompleteCall();

                    _log?.Invoke(descriptor.ServiceType, elapsed);
                }
            }

        }
        catch (Exception ex)
        {
            logger.Technical<LogGrpcMonitoringTimeElapsedMiddleware>().LogException(ex);
        }
    }
}
