using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Arc4u.OAuth2.Msal.Token
{
    public class JwtHttpHandler : DelegatingHandler
    {
        #region Backend usage

        // on the Frontend, we have to retrieve the user context based the singleton instance of the application context
        // service if this was done in the context of a user (via rest api or gRPC service).

        /// <summary>
        /// This is a ctor to use only in a frontend scenario.
        /// No inner handler is defined because this will be done via the AddHttpClient method in a service!
        /// </summary>
        /// <param name="container">The scoped container</param>
        /// <param name="resolvingName">The name used to resolve the settings</param>
        public JwtHttpHandler(IContainerResolve container, ILogger<JwtHttpHandler> logger, string resolvingName)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            _logger = logger;

            if (!container.TryResolve(resolvingName, out _settings))
                _logger.Technical().System($"No settings for {resolvingName} is found.").Log();


            container.TryResolve(out _applicationContext);

        }

        #endregion

        private readonly IKeyValueSettings _settings;
        private readonly IContainerResolve _container;
        private readonly IApplicationContext _applicationContext;
        private readonly ILogger<JwtHttpHandler> _logger;

        private IContainerResolve GetResolver() => _container;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Technical().System($"{GetType().Name} delegate handler is called.").Log();

                var applicationContext = GetResolver().Resolve<IApplicationContext>();

                if (null == _settings || null == applicationContext)
                {
                    _logger.Technical().System($"{GetType().Name}, Check next Delegate Handler").Log();
                    return await base.SendAsync(request, cancellationToken);
                }

                // Or we have an OAuth token and we have to validate if the Authenication Type is 
                // well the same as the ClaimsPrincipal!
                // Or we inject in a header another kind of token and we just inject it (no other check).
                // By pass is provided with the AuthenticationType value = "Inject"

                if (!_settings.Values.TryGetValue(TokenKeys.AuthenticationTypeKey, out var authenticationType))
                {
                    _logger.Technical().System($"No antuentication type for {GetType().Name}, Check next Delegate Handler").Log();
                    return await base.SendAsync(request, cancellationToken);
                }

                var inject = authenticationType.Equals("inject", StringComparison.InvariantCultureIgnoreCase);

                // if we don't inject a bearer token, the AuthenticationType defined in the settings must be the same as the authentication type defined in the Identity.
                if (!inject
                    && null != applicationContext?.Principal?.Identity
                    && !authenticationType.ToLowerInvariant().Contains(applicationContext.Principal.Identity.AuthenticationType.ToLowerInvariant()))
                {
                    _logger.Technical().System($"Authentication type is not the same as the Identity for {GetType().Name}, Check next Delegate Handler").Log();
                    return await base.SendAsync(request, cancellationToken);
                }

                // if we inject more than one bearer token do it only if no one exist already.
                if (null != request.Headers.Authorization)
                {
                    _logger.Technical().System($"An authorization header already exist for handler {GetType().Name}, Check next Delegate Handler").Log();
                    return await base.SendAsync(request, cancellationToken);
                }

                _logger.Technical().System($"{GetType().Name} token provider is called.").Log();

                ITokenProvider provider = GetResolver().Resolve<ITokenProvider>(_settings.Values[TokenKeys.ProviderIdKey]);

                _logger.Technical().System("Requesting an authentication token.").Log();
                var tokenInfo = await provider.GetTokenAsync(_settings, null);

                // check if the token is still valid.
                // This is due to gRPC. It is possible that a gRPC streaming call is not closed and the token in the HttpContext is expired.
                // It is also possible this with OAuth where the token is added to the Identity and used like this => no refresh of the token is possible.
                if (tokenInfo.ExpiresOnUtc < DateTime.UtcNow)
                {
                    _logger.Technical().System($"Token is expired! Next Hanlder will be called.").Log();
                    return await base.SendAsync(request, cancellationToken);
                }

                _logger.Technical().System("Remove any Bearer token attached.").Log();
                request.Headers.Remove("Bearer");

                var scheme = inject ? tokenInfo.AccessTokenType : "Bearer";
                _logger.Technical().System($"Add the {scheme} token to provide authentication evidence.").Log();

                if (new string[] { "Bearer", "Basic" }.Any(s => s.Equals(scheme, StringComparison.InvariantCultureIgnoreCase)))
                    request.Headers.Authorization = new AuthenticationHeaderValue(scheme, tokenInfo.AccessToken);
                else
                    request.Headers.Add(scheme, tokenInfo.AccessToken);

                // Add ActivityId if founded!
                if (null != applicationContext?.Principal)
                {
                    if (null != applicationContext?.Principal?.ActivityID)
                    {
                        _logger.Technical().System($"Add the activity id to the request for tracing purpose: {applicationContext.Principal.ActivityID}.").Log();
                        request.Headers.Add("activityid", applicationContext.Principal.ActivityID.ToString());
                    }

                    _logger.Technical().System($"Add the current culture to the request: {applicationContext.Principal.Profile?.CurrentCulture?.TwoLetterISOLanguageName}").Log();
                    var culture = applicationContext.Principal.Profile?.CurrentCulture?.TwoLetterISOLanguageName;
                    if (null != culture)
                        request.Headers.Add("culture", culture);
                }

            }
            catch (Exception ex)
            {
                _logger.Technical().Exception(ex).Log();
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
