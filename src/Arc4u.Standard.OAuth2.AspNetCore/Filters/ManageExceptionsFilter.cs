using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Arc4u.AspNetCore.Results;
using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Arc4u.ServiceModel;
using Microsoft.AspNetCore.Http;
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
        var activityId = string.IsNullOrEmpty(_application?.ActivityID) ? Activity.Current?.Id ?? Guid.NewGuid().ToString() : _application?.ActivityID;

        // First log the exception.
        _logger.Technical().Exception(context.Exception).AddIf(string.IsNullOrEmpty(_application?.ActivityID), LoggingConstants.ActivityId, () => activityId).Log();

        switch (context.Exception)
        {
            case UnauthorizedAccessException:
                context.Result = new ObjectResult(new ProblemDetails()
                                                        .WithTitle("Unauthorized")
                                                        .WithDetail("You are not allowed to perform this operation")
                                                        .WithStatusCode(StatusCodes.Status403Forbidden)
                                                        .WithSeverity("Error")
                                                        .WithType(new Uri("https://github.com/GFlisch/Arc4u/wiki/StatusCodes#unauthorized")));
                break;
            case AppException appException:
                context.Result = new BadRequestObjectResult(Messages.FromEnum(appException.Messages.Where(m => m.Category == Arc4u.ServiceModel.MessageCategory.Business)));
                context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
            default:
                context.Result = new ObjectResult(new ProblemDetails()
                                                        .WithTitle("Unexpected error.")
                                                        .WithDetail($"A technical error occured, contact the application owner. A message has been logged with id: {activityId}")
                                                        .WithStatusCode(StatusCodes.Status500InternalServerError)
                                                        .WithSeverity("Error")
                                                        .WithType(new Uri("https://github.com/GFlisch/Arc4u/wiki/StatusCodes#unexpected-error")));
                break;
        }

        return Task.CompletedTask;
    }
}
