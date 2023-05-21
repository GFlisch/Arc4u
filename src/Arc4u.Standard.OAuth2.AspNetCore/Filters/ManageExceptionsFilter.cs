using System;
using System.Linq;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.AspNetCore.Filters;

/// <summary>
/// Global filter to manage exceptions.
/// Will log the exception and return a BadRequestObjectResult with the ActivityId to use to retrieve the information in the log.
/// Only business messages are returned to the client assoicated with the <see cref="AppException"/>."/>
/// </summary>
public class ManageExceptionsFilter : IAsyncExceptionFilter
{
    public ManageExceptionsFilter(ILogger<ManageExceptionsFilter> logger, IApplicationContext application)
    {
        _logger = logger;
        _application = application;
    }

    private readonly ILogger<ManageExceptionsFilter> _logger;
    private readonly IApplicationContext _application;

    public Task OnExceptionAsync(ExceptionContext context)
    {
        // If the activity id is not set, create one. This is the case for anonymous users.
        var activityId = _application?.Principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString();

        // First log the exception.
        _logger.Technical().Exception(context.Exception).AddIf(_application?.Principal?.ActivityID is null, LoggingConstants.ActivityId, activityId).Log();

        switch (context.Exception)
        {
            case UnauthorizedAccessException:
                context.HttpContext.Response.StatusCode = 403;
                context.ExceptionHandled = true;
                break;
            case AppException appException:
                context.Result = new BadRequestObjectResult(Messages.FromEnum(appException.Messages.Where(m => m.Category == Arc4u.ServiceModel.MessageCategory.Business)));
                break;
            default:
                context.Result = new BadRequestObjectResult(new Message(Arc4u.ServiceModel.MessageCategory.Technical, Arc4u.ServiceModel.MessageType.Error, $"A technical error occured, contact the application owner. A message has been logged with id: {activityId}"));
                break;
        }

        return Task.CompletedTask;
    }
}
