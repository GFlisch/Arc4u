using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.Security.Principal;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata.Ecma335;

namespace Arc4u.OAuth2.Token
{
    public class JwtHttpHandler : DelegatingHandler
    {
        #region Backend usage

        // on the backend, we have to retrieve the user context based on his scoped context when we do a request to another
        // service if this was done in the context of a user (via rest api or gRPC service).
        // When we do a call from a service account, in this case the user is fixed and not scoped => so the creation of the user in this
        // case can be a singleton because we do an impersonation!

        /// <summary>
        /// This is a ctor to use only in a backend scenario => where <see cref="IPlatformParameters"/> it is not used!
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
            _parameters = _applicationContext;
            _settingsName = null;

        }

        public JwtHttpHandler(IHttpContextAccessor accessor, ILogger<JwtHttpHandler> logger, string? resolvingName)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

            _logger = logger;

            _settingsName = resolvingName;

            _parameters = null; // will be assigned in the context of the user (scoped).
            _container = null;
        }

        #endregion

        private IKeyValueSettings? _settings;
        private readonly object? _parameters;
        private readonly string? _settingsName;
        private readonly IContainerResolve? _container;
        private readonly IApplicationContext? _applicationContext;
        private readonly IHttpContextAccessor? _accessor;
        private readonly ILogger<JwtHttpHandler> _logger;

        private IContainerResolve? GetResolver() => _accessor is null ? _container : _accessor.HttpContext?.RequestServices.GetService<IContainerResolve>();

        private IApplicationContext? GetCallContext(out object? parameters, out IContainerResolve? containerResolve)
        {
            containerResolve = GetResolver();

            if (containerResolve is null)
            {
                parameters = null;
                return null;
            }

            // first priority to the scope one!
            if (_accessor?.HttpContext?.RequestServices is not null)
            {
                var ctx = containerResolve.Resolve<IApplicationContext>();
                parameters = ctx;

                // As this is global for an handler, this can be saved at the level of the class.
                // We do this here only when using an accessor => the IContainerResolve is only available when not in the context of a call
                // when the JwtHttpHandler is built.
                if (null == _settings && !string.IsNullOrWhiteSpace(_settingsName))
                {
                    if (!containerResolve.TryResolve(_settingsName, out _settings))
                        _logger.Technical().System($"No settings for {_settingsName} is found.").Log();
                }

                return ctx;
            }

            parameters = _parameters;

            return _applicationContext;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.Technical().System($"{GetType().Name} delegate handler is called.").Log();

                var applicationContext = GetCallContext(out var parameters, out var containerResolve);

                if (_settings is null || applicationContext is null || containerResolve is null)
                {
                    _logger.Technical().System($"No settings or application context is defined with {GetType().Name}, Check next Delegate Handler").Log();
                    return await base.SendAsync(request, cancellationToken);
                }

                // Or we have an OAuth token and we have to validate if the Authenication Type is 
                // well the same as the ClaimsPrincipal!
                // Or we inject in a header another kind of token and we just inject it (no other check).
                // By pass is provided with the AuthenticationType value = "Inject"

                if (!_settings.Values.TryGetValue(TokenKeys.AuthenticationTypeKey, out var authenticationType))
                {
                    _logger.Technical().System($"No authentication type for {GetType().Name}, Check next Delegate Handler").Log();
                    return await base.SendAsync(request, cancellationToken);
                }

                var inject = authenticationType.Equals("inject", StringComparison.InvariantCultureIgnoreCase);

                // if we don't inject a bearer token, the AuthenticationType defined in the settings must be the same as the authentication type defined in the Identity.
                if (!inject
                    && applicationContext?.Principal?.Identity?.AuthenticationType is not null  
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

                ITokenProvider provider = containerResolve.Resolve<ITokenProvider>(_settings.Values[TokenKeys.ProviderIdKey]);

                _logger.Technical().System("Requesting an authentication token.").Log();
                var tokenInfo = await provider.GetTokenAsync(_settings, parameters);

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

                var scheme = inject ? tokenInfo.TokenType : "Bearer";
                _logger.Technical().System($"Add the {scheme} token to provide authentication evidence.").Log();

                if (new string[] { "Bearer", "Basic" }.Any(s => s.Equals(scheme, StringComparison.InvariantCultureIgnoreCase)))
                    request.Headers.Authorization = new AuthenticationHeaderValue(scheme, tokenInfo.Token);
                else
                    request.Headers.Add(scheme, tokenInfo.Token);

                // Add ActivityId if founded!
                if (applicationContext?.Principal is not null)
                {
                    if (applicationContext!.Principal?.ActivityID is not null)
                    {
                        _logger.Technical().System($"Add the activity id to the request for tracing purpose: {applicationContext.Principal.ActivityID}.").Log();
                        request.Headers.Add("activityid", applicationContext.Principal.ActivityID.ToString());
                    }

                    var culture = applicationContext!.Principal?.Profile?.CurrentCulture?.TwoLetterISOLanguageName;
                    if (culture is not null)
                    {
                        request.Headers.Add("culture", culture);
                        _logger.Technical().System($"Add the current culture to the request: {culture}").Log();

                    }
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
