using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Middleware
{

    public class LogMonitoringTimeElapsedMiddleware
    {
        private readonly RequestDelegate _next;
        private Action<Type, TimeSpan> _log;

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
            var logger = ((IContainerResolve)context.RequestServices.GetService(typeof(IContainerResolve))).Resolve<ILogger>();

            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            try
            {
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                    if (descriptor != null)
                    {
                        logger.Monitoring().From(descriptor.MethodInfo.DeclaringType, descriptor.MethodInfo.Name)
                            .Information($"Time to complete method call")
                            .Add("Elapsed", stopwatch.Elapsed.TotalMilliseconds)
                            .Add("StatusCode", context.Response.StatusCode)
                            .Log();

                        _log?.Invoke(descriptor.MethodInfo.DeclaringType, stopwatch.Elapsed);
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Technical().From<LogMonitoringTimeElapsedMiddleware>().Exception(ex).Log();
            }
        }
    }
}
