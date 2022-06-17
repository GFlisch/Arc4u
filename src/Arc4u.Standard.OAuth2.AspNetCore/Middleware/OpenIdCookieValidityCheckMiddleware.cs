using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public class OpenIdCookieValidityCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public OpenIdCookieValidityCheckMiddleware(RequestDelegate next, OpenIdCookieValidityCheckOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (null == options.CookieManager)
                throw new ArgumentNullException("CookieManager is not set.");
        }

        private readonly OpenIdCookieValidityCheckOptions _options;
        private ActivitySource _activitySource;
        private bool hasAlreadyTriedToResolve = false;


        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated && context.User.Identity.AuthenticationType.Equals(_options.AuthenticationType, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check we have a valid token Adal token in cache?
                // if not, force authentication.
                ILogger<OpenIdCookieValidityCheckMiddleware> logger = null;
                try
                {
                    IContainerResolve container = (IContainerResolve)context.RequestServices.GetService(typeof(IContainerResolve));

                    // Not thread safe => can call activity source factory more than one but factory implementation is thread safe => so few calls on the start of the service is a possibility.
                    if (!hasAlreadyTriedToResolve)
                    {
                        hasAlreadyTriedToResolve = true;
                        _activitySource = container.Resolve<IActivitySourceFactory>()?.GetArc4u();
                    }

                    TokenInfo tokenInfo = null;

                    using (var activity = _activitySource?.StartActivity("Validate bearer token expiration", ActivityKind.Producer))
                    {
                        activity?.SetTag("AuthenticationType", _options.AuthenticationType);

                        if (container.TryResolve<IApplicationContext>(out var applicationContext))
                        {
                            applicationContext.SetPrincipal(new AppPrincipal(new Authorization(), context.User.Identity, "S-1-0-0"));
                        }

                        logger = container.Resolve<ILogger<OpenIdCookieValidityCheckMiddleware>>();

                        ITokenProvider provider = container.Resolve<ITokenProvider>(_options.OpenIdSettings.Values[TokenKeys.ProviderIdKey]);

                        tokenInfo = await provider.GetTokenAsync(_options.OpenIdSettings, context.User.Identity);
                    }

                    if (null == tokenInfo)
                        throw new ApplicationException("Refresh Token is expired or invalid");

                    await _next.Invoke(context);
                }
                catch (Exception)
                {
                    _options.CookieManager.DeleteCookie(context, _options.CookieName, new CookieOptions());

                    logger?.Technical().System("Force an OpenId connection.").Log();
                    var cleanUri = new Uri(new Uri(context.Request.GetEncodedUrl()).GetLeftPart(UriPartial.Path));
                    if (Uri.TryCreate(_options.RedirectAuthority, UriKind.Absolute, out var authority))
                    {
                        cleanUri = new Uri(authority, cleanUri.AbsolutePath);
                    }
                    var properties = new AuthenticationProperties() { RedirectUri = cleanUri.ToString() };
                    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);
                    return;
                }
            }
            else
                await _next.Invoke(context);
        }
    }

}
