using System.Text.RegularExpressions;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Events;

public sealed partial class StandardOpenIdConnectEvents : OpenIdConnectEvents

{
    private readonly ILogger<StandardOpenIdConnectEvents> _logger;
    public StandardOpenIdConnectEvents(ILogger<StandardOpenIdConnectEvents> logger)
    {
        _logger = logger;
    }

#if NET8_0_OR_GREATER
    [GeneratedRegex(@"\b(?:http:\/\/localhost|https:\/\/)\b", RegexOptions.IgnoreCase)]
    public static partial Regex HttpRegex();
#endif
#if NET6_0
    private static readonly Regex httpRegex = new Regex(@"\b(?:http:\/\/localhost|https:\/\/)\b", RegexOptions.IgnoreCase);

    public static Regex HttpRegex()
    {
        return httpRegex;
    }
#endif
    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        // force https for redirect uri but for localhost.
        if (!HttpRegex().IsMatch(context.ProtocolMessage.RedirectUri))
        {
            context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace("http://", "https://");
        }

        // Has been introduced for AzureAD => works also for Keykloack.
        context.ProtocolMessage.State = Guid.NewGuid().ToString();
        return base.RedirectToIdentityProvider(context);
    }

    public override async Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.HandleResponse();

        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";

        _logger.Technical().LogException(context.Exception);

        await context.Response.WriteAsync("<html><p>You are not authenticated.</p></html>").ConfigureAwait(false);

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
