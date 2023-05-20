using System;
using System.Linq;
using System.Threading.Tasks;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;

namespace Arc4u.OAuth2.Middleware;

public class ForceOpenIdMiddleWare
{
    private readonly RequestDelegate _next;
    private readonly ForceOpenIdMiddleWareOptions _options;
    private ILogger<ForceOpenIdMiddleWare>? _logger;

    public ForceOpenIdMiddleWare(RequestDelegate next, ForceOpenIdMiddleWareOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public async Task Invoke(HttpContext context)
    {
        // Get the scoped instance of the container!
        _logger ??= context.RequestServices.GetRequiredService<ILogger<ForceOpenIdMiddleWare>>();

        try
        {
            // if we have some part of the site working like a web page (like swagger, hangfire, etc...) and we need to force
            // authentication. We can add the start of the path to check and in this case we force a login!
            if (context.User is not null && context.User.Identity is not null && context.User.Identity.IsAuthenticated is false)
            {
                if (_options.ForceAuthenticationForPaths.Any(r =>
                {
                    return context.Request.Path.HasValue
                           && (r.Last().Equals('*') ?
                                context.Request.Path.Value.StartsWith(r.Remove(r.Length - 1), StringComparison.OrdinalIgnoreCase)
                                :
                                 context.Request.Path.Value.Equals(r, StringComparison.OrdinalIgnoreCase));
                }))
                {
                    _logger.Technical().LogDebug("Force an OpenId connection.");
                    var cleanUri = new Uri(new Uri(context.Request.GetEncodedUrl()).GetLeftPart(UriPartial.Path));
                    if (Uri.TryCreate(_options.RedirectUrlForAuthority, UriKind.Absolute, out var authority))
                    {
                        cleanUri = new Uri(authority, cleanUri.AbsolutePath);
                    }
                    var properties = new AuthenticationProperties() { RedirectUri = cleanUri.ToString() };
                    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties).ConfigureAwait(false);
                    return;
                }
            }

        }
        catch (Exception ex)
        {
            _logger.Technical().LogException(ex);
        }

        await _next(context).ConfigureAwait(false);
    }
}

