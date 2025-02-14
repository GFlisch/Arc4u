using System.Net.Http.Headers;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Msal.Token;

public class JwtHttpHandler : DelegatingHandler
{
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
        {
            _logger.Technical().System($"No settings for {resolvingName} is found.").Log();
        }

        container.TryResolve(out _applicationContext);
    }

    public JwtHttpHandler(IContainerResolve container, ILogger<JwtHttpHandler> logger, IKeyValueSettings settings)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        _logger = logger;

        container.TryResolve(out _applicationContext);
    }

    private readonly IKeyValueSettings? _settings;
    private readonly IContainerResolve _container;
    private readonly IApplicationContext? _applicationContext;
    private readonly ILogger<JwtHttpHandler> _logger;

    private IContainerResolve GetResolver() => _container;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.Technical().System($"{GetType().Name} delegate handler is called.").Log();

        if (null == _settings || null == _applicationContext)
        {
            _logger.Technical().System($"{GetType().Name}, Check next Delegate Handler").Log();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // Or we have an OAuth token and we have to validate if the Authenication Type is 
        // well the same as the ClaimsPrincipal!
        // Or we inject in a header another kind of token and we just inject it (no other check).
        // By pass is provided with the AuthenticationType value = "Inject"

        if (!_settings.Values.TryGetValue(TokenKeys.AuthenticationTypeKey, out var authenticationType))
        {
            _logger.Technical().System($"No antuentication type for {GetType().Name}, Check next Delegate Handler").Log();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var inject = authenticationType.Equals("inject", StringComparison.InvariantCultureIgnoreCase);

        // if we don't inject a bearer token, the AuthenticationType defined in the settings must be the same as the authentication type defined in the Identity.
        if (!inject
            && null != _applicationContext?.Principal?.Identity
            && null != _applicationContext?.Principal?.Identity.AuthenticationType
            && !authenticationType.ToLowerInvariant().Contains(_applicationContext.Principal.Identity.AuthenticationType.ToLowerInvariant()))
        {
            _logger.Technical().System($"Authentication type is not the same as the Identity for {GetType().Name}, Check next Delegate Handler").Log();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // if we inject more than one bearer token do it only if no one exist already.
        if (null != request.Headers.Authorization)
        {
            _logger.Technical().System($"An authorization header already exist for handler {GetType().Name}, Check next Delegate Handler").Log();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        _logger.Technical().System($"{GetType().Name} token provider is called.").Log();

        var provider = GetResolver().Resolve<ITokenProvider>(_settings.Values[TokenKeys.ProviderIdKey]);

        if (null == provider)
        {
            _logger.Technical().System($"No token provider is defined for {_settings.Values[TokenKeys.ProviderIdKey]}, Check next Delegate Handler").Log();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        _logger.Technical().System("Requesting an authentication token.").Log();
        var tokenInfo = await provider.GetTokenAsync(_settings, null).ConfigureAwait(false);

        // check if the token is still valid.
        // This is due to gRPC. It is possible that a gRPC streaming call is not closed and the token in the HttpContext is expired.
        // It is also possible this with OAuth where the token is added to the Identity and used like this => no refresh of the token is possible.
        if (null == tokenInfo || tokenInfo.ExpiresOnUtc < DateTime.UtcNow)
        {
            _logger.Technical().System($"Token is expired! Next Hanlder will be called.").Log();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        _logger.Technical().System("Remove any Bearer token attached.").Log();
        request.Headers.Remove("Bearer");

        var scheme = inject ? tokenInfo.TokenType : "Bearer";
        _logger.Technical().System($"Add the {scheme} token to provide authentication evidence.").Log();

        if (new string[] { "Bearer", "Basic" }.Any(s => s.Equals(scheme, StringComparison.InvariantCultureIgnoreCase)))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, tokenInfo.Token);
        }
        else
        {
            request.Headers.Add(scheme, tokenInfo.Token);
        }

        // Add ActivityId if founded!
        if (null != _applicationContext?.Principal)
        {
            if (null != _applicationContext?.ActivityID)
            {
                _logger.Technical().System($"Add the activity id to the request for tracing purpose: {_applicationContext.ActivityID}.").Log();
                request.Headers.Add("activityid", _applicationContext.ActivityID);
            }

            _logger.Technical().System($"Add the current culture to the request: {_applicationContext!.Principal.Profile?.CurrentCulture?.TwoLetterISOLanguageName}").Log();
            var culture = _applicationContext.Principal.Profile?.CurrentCulture?.TwoLetterISOLanguageName;
            if (null != culture)
            {
                request.Headers.Add("culture", culture);
            }
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
