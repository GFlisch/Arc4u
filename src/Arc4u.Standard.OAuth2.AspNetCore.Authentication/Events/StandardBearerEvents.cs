using System;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Arc4u.OAuth2.Events;

public class StandardBearerEvents : JwtBearerEvents
{
    private readonly ILogger<StandardBearerEvents> _logger;
    public StandardBearerEvents(ILogger<StandardBearerEvents> logger)
    {
        _logger = logger;
    }


    public override Task MessageReceived(MessageReceivedContext context)
    {
        return base.MessageReceived(context);
    }

    /// <summary>
    /// Log the exception, reason why the authentication is failing.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        _logger.Technical().Exception(context.Exception);

        context.Fail(context.Exception);
        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Save the access token in the identity.BootstrapContext
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public override Task TokenValidated(TokenValidatedContext context)
    {
        if (context.SecurityToken is SecurityToken)
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                var sToken = context.Request.Headers.Authorization.ToString();
                if (sToken.StartsWith("Bearer ", StringComparison.InvariantCultureIgnoreCase))
                {
                    sToken = sToken.Substring(7);
                    identity.BootstrapContext = sToken;
                }
                else
                {
                    context.Fail("A Bearer token is expected!");
                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
        }

        return Task.CompletedTask;
    }
}
