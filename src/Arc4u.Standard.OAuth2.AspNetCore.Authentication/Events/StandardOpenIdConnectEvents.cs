using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Arc4u.Configuration;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Events;

public sealed partial class StandardOpenIdConnectEvents : OpenIdConnectEvents

{
    private readonly ILogger<StandardOpenIdConnectEvents> _logger;
    private readonly OidcAuthenticationOptions _oidcOptions;
    private readonly IOptionsMonitor<SimpleKeyValueSettings> _openIdOptions;

    public StandardOpenIdConnectEvents(ILogger<StandardOpenIdConnectEvents> logger, IOptionsMonitor<OidcAuthenticationOptions> oidcOptions, IOptionsMonitor<SimpleKeyValueSettings> openIdOptions)
    {
        _logger = logger;
        _oidcOptions = oidcOptions.CurrentValue;
        _openIdOptions = openIdOptions;

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

    public override Task TokenResponseReceived(TokenResponseReceivedContext context)
    {
        if (string.IsNullOrWhiteSpace(context.TokenEndpointResponse.AccessToken))
        {
            return Task.CompletedTask;
        }

        var jwtToken = new JwtSecurityToken(context.TokenEndpointResponse.AccessToken);
        var options = _openIdOptions.Get(_oidcOptions.OpenIdSettingsKey);

        if (_oidcOptions.ValidateAudience)
        {
            if (!jwtToken.Audiences.Any(aud => options.Values[TokenKeys.Audiences].Contains(aud)))
            {
                _logger.Technical()
                    .LogError("Audience(s) {audience} is not in the list of allowed audience(s): {audiences}.",
                        jwtToken.Audiences,
                        options.Values[TokenKeys.Audiences]);

                context.Fail("Invalid audience");

                return Task.CompletedTask;
            }
        }

        // when a default authority is not defined...
        if (_oidcOptions.ValidateAuthority)
        {
            if (jwtToken.Issuer.Equals(_oidcOptions.DefaultAuthority.Url.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            _logger.Technical()
                .LogError("Authority {authority} is not in the expected one: {authority_from_config}.",
                    jwtToken.Issuer,
                    _oidcOptions.DefaultAuthority.Url.AbsoluteUri);

            //context.HandleResponse();
            context.Fail("Invalid authority");

            return Task.CompletedTask;
        }

        return Task.CompletedTask;

    }

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

        return context.Response.WriteAsync($"<html><p>There was an issue during the request: {context.Failure?.Message ?? "Unknown error"}.</p></html>");
    }
}
