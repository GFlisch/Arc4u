using Arc4u.Diagnostics;
using Arc4u.ServiceModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Arc4u.Standard.Diagnostics.AspNetCore
{
    /// <summary>
    /// A handler for exceptions thrown by back-end methods.
    /// This class is internal to this assembly, since it's an implementation detail.
    /// </summary>
    class DynamicExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DynamicExceptionHandler> _logger;
        private readonly IDynamicExceptionMap _map;

        public DynamicExceptionHandler(RequestDelegate next, IDynamicExceptionMap map, ILogger<DynamicExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
            _map = map;
        }



        /// <summary>
        /// Handle an exception.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                // generate a unique id to allow correlation between the output message (if any) and the log information
                var exceptionUid = Guid.NewGuid();
                // regardless of what happened, log the exception, and add the path and the Uid as attributes.
                var logger = _logger.Technical().Exception(error)
                    .Add(DefaultExceptionHandler.ExceptionUidKey, exceptionUid.ToString());

                // optionally add the controller method if it is available
                var methodInfo = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>()?.MethodInfo;
                if (methodInfo is not null)
                    logger.Add(nameof(methodInfo.DeclaringType), methodInfo.DeclaringType.Name)
                        .Add(nameof(methodInfo.Name), methodInfo.Name);
                // log everthing now
                logger.Log();

                if (_map.TryGetValue(error.GetType(), out var item))
                {
                    (int StatusCode, object Value) result;
                    // a handler was found. Trigger either the synchronous or the asynchronous variant (we can't have both)
                    if (item.Handler != null)
                        result = ((int StatusCode, object Value))item.Handler.DynamicInvoke(context.Request.Path.ToString(), error, exceptionUid);
                    else
                    {
                        var task = (Task<(int StatusCode, object Value)>)item.HandlerAsync.DynamicInvoke(context.Request.Path.ToString(), error, exceptionUid, context.RequestAborted);
                        result = await task;
                    }
                    context.Response.StatusCode = result.StatusCode;
                    if (result.Value is not null)
                        await context.Response.WriteAsJsonAsync(result.Value);
                }
                else
                {
                    // if none of the handlers were triggered, use our default reply
                    var messages = new Messages { new Message(ServiceModel.MessageCategory.Technical, MessageType.Error, $"A technical error occured. {DefaultExceptionHandler.ExceptionUidKey} = {exceptionUid}") };
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(messages);
                }
            }
        }
    }
}

