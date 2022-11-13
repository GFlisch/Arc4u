using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Events;

public class CustomOpenIdConnectEvents : OpenIdConnectEvents
{
    private readonly ILogger<CustomOpenIdConnectEvents> _logger;
    public CustomOpenIdConnectEvents(ILogger<CustomOpenIdConnectEvents> logger)
    {
        _logger = logger;
    }

    public override async Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.HandleResponse();

        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";

        _logger.Technical().Exception(context.Exception);

        await context.Response.WriteAsync("<html><p>You are not authenticated.</p></html>");

    }

    public override Task AccessDenied(AccessDeniedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.HandleResponse();

        return context.Response.WriteAsync("<html><p>You are not authorized to use this api</p></html>");

    }

    public override Task RemoteFailure(RemoteFailureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.HandleResponse();

        return context.Response.WriteAsync("<html><p>There was an issue to contact the remote authority.</p></html>");
    }
}
