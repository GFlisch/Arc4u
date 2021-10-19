﻿using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Middleware
{
    public class OpenIdBearerInjectorMiddleware
    {
        public OpenIdBearerInjectorMiddleware(RequestDelegate next, OpenIdBearerInjectorOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));

            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (null == options.OpenIdSettings)
                throw new ArgumentNullException(nameof(options.OpenIdSettings));
        }

        private readonly OpenIdBearerInjectorOptions _options;
        private readonly RequestDelegate _next;
        private ActivitySource _activitySource;
        private bool hasAlreadyTriedToResolve = false;

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated && context.User.Identity.AuthenticationType.Equals(_options.OpenIdSettings.Values[TokenKeys.AuthenticationTypeKey], StringComparison.InvariantCultureIgnoreCase))
            {
                if (context.User is AppPrincipal principal)
                {
                    context.Request?.Headers?.Add("activityid", principal?.ActivityID.ToString() ?? Guid.NewGuid().ToString());
                    if (null != principal?.Profile?.CurrentCulture)
                        context.Request?.Headers?.Add("culture", principal.Profile.CurrentCulture.TwoLetterISOLanguageName);
                }

                IContainerResolve container = (IContainerResolve)context.RequestServices.GetService(typeof(IContainerResolve));

                // Not thread safe => can call activity source factory more than one but factory implementation is thread safe => so few calls on the start of the service is a possibility.
                if (!hasAlreadyTriedToResolve)
                {
                    hasAlreadyTriedToResolve = true;
                    _activitySource = container.Resolve<IActivitySourceFactory>()?.GetArc4u();
                }

                using (var activity = _activitySource?.StartActivity("Inject bearer token in header", ActivityKind.Producer))
                {
                    ITokenProvider provider = container.Resolve<ITokenProvider>(_options.OpenIdSettings.Values[TokenKeys.ProviderIdKey]);

                    var tokenInfo = await provider.GetTokenAsync(_options.OpenIdSettings, context.User.Identity);

                    var authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.AccessToken).ToString();
                    context.Request.Headers.Remove("Authorization");
                    context.Request.Headers.Add("Authorization", authorization);
                }
            }
            await _next.Invoke(context);
        }
    }
}