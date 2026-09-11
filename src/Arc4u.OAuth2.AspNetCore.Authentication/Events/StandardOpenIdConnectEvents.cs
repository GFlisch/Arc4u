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

namespace Arc4u.OAuth2.Events
{
    public sealed partial class StandardOpenIdConnectEvents : OpenIdConnectEvents

    {
        private readonly ILogger<StandardOpenIdConnectEvents> _logger;
        private readonly OidcAuthenticationOptions _hybridOptions;
        private readonly IOptionsMonitor<SimpleKeyValueSettings> _openIdOptions;
        private readonly IOptionsMonitor<ApiExtraContextAuthenticationOption> _extraContextOptions;

        public StandardOpenIdConnectEvents(ILogger<StandardOpenIdConnectEvents> logger, IOptionsMonitor<OidcAuthenticationOptions> oidcOptions, IOptionsMonitor<SimpleKeyValueSettings> openIdOptions, IOptionsMonitor<ApiExtraContextAuthenticationOption> extraContextOptions)
        {
            _hybridOptions = oidcOptions.CurrentValue;
            _openIdOptions = openIdOptions;
            _logger = logger;
            _extraContextOptions = extraContextOptions;
        }

        [GeneratedRegex(@"\b(?:http:\/\/localhost|https:\/\/)\b", RegexOptions.IgnoreCase)]
        private static partial Regex HttpRegex();

        public override Task TokenResponseReceived(TokenResponseReceivedContext context)
        {
            if (string.IsNullOrWhiteSpace(context.TokenEndpointResponse.AccessToken))
            {
                return Task.CompletedTask;
            }

            var jwtToken = new JwtSecurityToken(context.TokenEndpointResponse.AccessToken);
            var options = _openIdOptions.Get(Constants.CookiesAuthenticationType);

            if (_hybridOptions.ValidateAudience)
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
            if (!_hybridOptions.ValidateAuthority)
            {
                return Task.CompletedTask;
            }

            if (jwtToken.Issuer.Equals(_hybridOptions.DefaultAuthority.Url.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            _logger.Technical()
                .LogError("Authority {authority} is not in the expected one: {authority_from_config}.",
                    jwtToken.Issuer,
                    _hybridOptions.DefaultAuthority.Url.AbsoluteUri);

            //context.HandleResponse();
            context.Fail("Invalid authority");

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

            // Some Idp require extra parameters to link application definition to the right Api context.
            foreach (var extra in _extraContextOptions.CurrentValue.AuthorizationParameters.Values)
            {
                context.ProtocolMessage.SetParameter(extra.Key, extra.Value);
            }

            return base.RedirectToIdentityProvider(context);
        }

        public override Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            // Some Idp require extra parameters to link application definition to the right Api context.
            foreach (var extra in _extraContextOptions.CurrentValue.TokenParameters.Values)
            {
                context.TokenEndpointRequest?.Parameters.Add(extra.Key, extra.Value);
            }

            return base.AuthorizationCodeReceived(context);
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
}
