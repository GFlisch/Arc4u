using System.Net;
using System.Security.Claims;
using System.Text.Json;
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

    public override Task Challenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        context.Error = "invalid_or_missing_token";
        context.ErrorDescription = "This request requires a valid JWT access token to be provided";

        // Add some extra context for expired tokens.
        if (context.AuthenticateFailure is not null && context.AuthenticateFailure is SecurityTokenExpiredException authenticationException)
        {
            var expires = authenticationException.Expires.ToString("o");
            context.Response.Headers.Add("x-token-expired", expires);
            context.ErrorDescription = $"The token expired on {expires}";
        }
        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            error = context.Error,
            error_description = context.ErrorDescription
        }));
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
