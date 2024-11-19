using System.Diagnostics;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Arc4u.AspNetCore.Middleware;

public class LogGrpcMonitoringTimeElapsedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Action<Type, TimeSpan> _log;

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

    public async Task Invoke(HttpContext context)
    {
        var logger = ((IContainerResolve)context.RequestServices.GetService(typeof(IContainerResolve))).Resolve<ILogger>();

        var stopwatch = Stopwatch.StartNew();

        await _next(context).ConfigureAwait(false);

        stopwatch.Stop();

        try
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var descriptor = endpoint.Metadata.GetMetadata<GrpcMethodMetadata>();
                if (descriptor != null)
                {
                    logger.Monitoring().From(descriptor.ServiceType, descriptor.Method.Name)
                        .Information($"Time to complete method call")
                        .Add("Elapsed", stopwatch.Elapsed.TotalMilliseconds)
                        .Add("StatusCode", context.Response.StatusCode)
                        .Log();

                    _log?.Invoke(descriptor.ServiceType, stopwatch.Elapsed);
                }
            }

        }
        catch (Exception ex)
        {
            logger.Technical().From<LogGrpcMonitoringTimeElapsedMiddleware>().Exception(ex).Log();
        }
    }
}
