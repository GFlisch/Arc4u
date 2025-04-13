using System.Text.RegularExpressions;
using System.Text;
using Arc4u.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;
using Arc4u.OAuth2.AspNetCore;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Middleware;

public class ForceOpenIdMiddleWare
{
    private readonly RequestDelegate _next;
    private readonly ForceOpenIdMiddleWareOptions _options;
    private readonly Regex _pathsRegex;

    public ForceOpenIdMiddleWare(RequestDelegate next, IOptions<ForceOpenIdMiddleWareOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _pathsRegex = PathsRegex(_options.ForceAuthenticationForPaths);
    }

    // Create a regex expression based on the array of string passed.
    // Avoid string manipulation during the request.
    private static Regex PathsRegex(IList<string> paths)
    {
        var sb = new StringBuilder();
        sb.Append("\\S*(");

        foreach (var path in paths)
        {
            sb.Append(path.Replace("*", "\\S+"));
            sb.Append('|');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(')');

        return new Regex(sb.ToString(), RegexOptions.IgnoreCase);

    }

    public async Task InvokeAsync(HttpContext context, ILogger<ForceOpenIdMiddleWare> logger)
    {
        try
        {
            // if we have some part of the site working like a web page (like swagger, hangfire, etc...) and we need to force
            // authentication. We can add the start of the path to check and in this case we force a login!
            if (context.User is not null && context.User.Identity is not null && context.User.Identity.IsAuthenticated is false)
            {
                if (_options.ForceAuthenticationForPaths.Any() &&
                    context.Request.Path.HasValue &&
                    _pathsRegex.IsMatch(context.Request.Path.Value))
                {
                    logger.Technical().LogForceOpenIdConnect();
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
            logger.Technical().LogException(ex);
        }

        await _next(context).ConfigureAwait(false);
    }
}
