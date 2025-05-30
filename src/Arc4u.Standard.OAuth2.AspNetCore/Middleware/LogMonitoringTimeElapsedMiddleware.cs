using System.Diagnostics;
using System.Reflection;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
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

    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var container = context.RequestServices.GetRequiredService<IContainerResolve>();
        var logger = container.Resolve<ILogger>();

        var startTimestamp = Stopwatch.GetTimestamp();

        await _next(context).ConfigureAwait(false);

        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

        if (logger == null)
        {
            return;
        }

        try
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                return;
            }

            MemberInfo? methodInfo = null;

            var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (descriptor?.MethodInfo?.DeclaringType is not null)
            {
                // Try to extract MethodInfo for MVC Controller endpoint
                methodInfo = descriptor.MethodInfo;
            }
            else
            {
                // Try to extract MethodInfo for Minimal API endpoints
                methodInfo = endpoint.Metadata.GetMetadata<MethodInfo>();
            }

            if (methodInfo?.DeclaringType is not null)
            {
                var properties = logger.Monitoring()
                    .From(methodInfo.DeclaringType, methodInfo.Name)
                    .Information("Time to complete method call")
                    .Add("Elapsed", elapsed.TotalMilliseconds)
                    .Add("StatusCode", context.Response.StatusCode);

                if (!string.IsNullOrWhiteSpace(endpoint.DisplayName))
                {
                    properties.Add("Endpoint", endpoint.DisplayName);
                }

                properties.Log();

                _log?.Invoke(methodInfo.DeclaringType, elapsed);
            }
        }
        catch (Exception ex)
        {
            logger.Technical()
                .From<LogMonitoringTimeElapsedMiddleware>()
                .Exception(ex)
                .Log();
        }
    }
}
