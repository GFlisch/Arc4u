using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Middleware;

public class OpenIdBearerInjectorMiddleware
{
    public OpenIdBearerInjectorMiddleware([DisallowNull] RequestDelegate next, [DisallowNull] OpenIdBearerInjectorSettingsOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    private readonly OpenIdBearerInjectorSettingsOptions _options;
    private readonly RequestDelegate _next;
    private ActivitySource? _activitySource;
    private ILogger<OpenIdBearerInjectorMiddleware> _logger = null;

    public async Task Invoke([DisallowNull] HttpContext context)
    {
        if (context.User is not null && context.User.Identity is not null && context.User.Identity.IsAuthenticated && context.User.Identity.AuthenticationType!.Equals(_options.OpenIdSettings.Values[TokenKeys.AuthenticationTypeKey], StringComparison.InvariantCultureIgnoreCase))
        {
            if (context.User is AppPrincipal principal)
            {
                context.Request?.Headers?.Add("activityid", principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString());
                if (null != principal?.Profile?.CurrentCulture)
                {
                    context.Request?.Headers?.Add("culture", principal.Profile.CurrentCulture.TwoLetterISOLanguageName);
                }
            }

            var container = context.RequestServices.GetRequiredService<IContainerResolve>();

            _activitySource ??= container.Resolve<IActivitySourceFactory>()?.GetArc4u();
            _logger ??= container.Resolve<ILogger<OpenIdBearerInjectorMiddleware>>();


            using (var activity = _activitySource?.StartActivity("Inject bearer token in header", ActivityKind.Producer))
            {
                TokenInfo? tokenInfo = null;

                if (_options.OnBehalfOfOpenIdSettings is not null && _options.OnBehalfOfOpenIdSettings.Values.Any())
                {
                    try
                    {
                        var provider = container.Resolve<ITokenProvider>(_options.OboProviderKey);

                        tokenInfo = await provider.GetTokenAsync(_options.OnBehalfOfOpenIdSettings, null).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Technical().Exception(ex).Log();
                    }

                }
                else
                {
                    try
                    {
                        var provider = container.Resolve<ITokenProvider>(_options.OpenIdSettings.Values[TokenKeys.ProviderIdKey]);

                        tokenInfo = await provider.GetTokenAsync(_options.OpenIdSettings, null).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Technical().Exception(ex).Log();
                    }
                }

                if (tokenInfo is not null)
                {
                    var authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token).ToString();
                    context.Request!.Headers.Remove("Authorization");
                    context.Request.Headers.Add("Authorization", authorization);
                }
            }
        }

        await _next.Invoke(context).ConfigureAwait(false);
    }
}

