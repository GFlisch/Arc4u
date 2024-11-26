using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Middleware;
public class AddContextToPrincipalMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AddContextToPrincipalMiddleware> _logger;
    private readonly ActivitySource? _activitySource;

    public AddContextToPrincipalMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = serviceProvider.GetRequiredService<ILogger<AddContextToPrincipalMiddleware>>();
        _activitySource = serviceProvider.GetService<IActivitySourceFactory>()?.GetArc4u();
    }

    public async Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Note that the action of setting a traceparent and adding context to the principal are conceptually separate, but they are viewed as one "activity" for the purposes of logging.

        using var activity = _activitySource?.StartActivity("Add context to Arc4u Principal", ActivityKind.Producer);

        // We may have a null current activity if we are not in the context of an activity.
        // we may not be in the context of an activity if either (1) there is no _activitySource or (2) the activity has no active listeners
        if (Activity.Current is not null)
        {
            // Ensure we have a traceparent, whether we are authenticated or not, since it's always useful to have a traceparent.
            if (!context.Request.Headers.ContainsKey("traceparent"))
            {
                // Activity.Current is not null because we are in the context of an activity.
                context.Request.Headers.Add("traceparent", Activity.Current.Id);
            }

            activity?.SetTag(LoggingConstants.ActivityId, Activity.Current.Id);
        }

        if (context.User is not null && context.User is AppPrincipal principal && principal.Identity is not null && principal.Identity.IsAuthenticated)
        {
            // Check for a culture. The header dictionary takes care of the proper matching of the header name.
            if ((context.Request?.Headers?.TryGetValue("culture", out var cultureHeader) ?? false) && cultureHeader.Any())
            {
                try
                {
                    principal.Profile.CurrentCulture = new CultureInfo(cultureHeader[0]);
                }
                catch (Exception ex)
                {
                    _logger.Technical().LogException(ex);
                }
            }
        }

        await _next(context).ConfigureAwait(false);
    }

}
