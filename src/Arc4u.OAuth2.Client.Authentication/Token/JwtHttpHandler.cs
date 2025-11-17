using System.Net.Http.Headers;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.OAuth2.Client.Authentication.Token;

public class JwtHttpHandler<T> : DelegatingHandler
{
    // on the Frontend, we have to retrieve the user context based the singleton instance of the application context
    // service if this was done in the context of a user (via rest api or gRPC service).

    /// <summary>
    /// This is a ctor to use only in a frontend scenario.
    /// No inner handler is defined because this will be done via the AddHttpClient method in a service!
    /// </summary>
    /// <param name="container">The scoped container</param>
    /// <param name="logger">The logger</param>
    /// <param name="resolvingName">The name used to resolve the settings</param>
    public JwtHttpHandler(IServiceProvider container, ILogger<T> logger, string resolvingName)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _container = container ?? throw new ArgumentNullException(nameof(container));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!container.TryGetService(resolvingName, out _settings))
        {
            _logger.Technical().LogNoHttpHandlerSettingsFound(resolvingName);
            throw new ConfigurationException($"No settings found for {resolvingName}.");
        }

        if (container.TryGetService(out _applicationContext))
        {
            return;
        }

        _logger.Technical().LogNoApplicationContextFound();
        throw new ConfigurationException($"No application context found in the DI container.");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="container">The scoped container</param>
    /// <param name="logger">The logger</param>
    /// <param name="settings">A key value collection.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public JwtHttpHandler(IServiceProvider container, ILogger<T> logger, IKeyValueSettings settings)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (container.TryGetService(out _applicationContext))
        {
            return;
        }

        _logger.Technical().LogNoApplicationContextFound();
        throw new ConfigurationException($"No application context found in the DI container.");
    }

    private readonly IKeyValueSettings? _settings;
    private readonly IServiceProvider _container;
    private readonly IApplicationContext? _applicationContext;
    private readonly ILogger<T> _logger;
    private static readonly string[] SourceArray = ["Bearer", "Basic"];

    private IServiceProvider GetResolver() => _container;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.Technical().LogHttpHandlerIsCalled(GetType().Name);

        // Add the authorization header if it is not already present.
        if (null != request.Headers.Authorization)
        {
            _logger.Technical().LogHasAlreadyAnAuthorizationHeader(GetType().Name);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        if (!_settings!.Values.TryGetValue(TokenKeys.ProviderIdKey, out var providerName))
        {
            _logger.Technical().LogNoTokenProviderIsDefinedInSettings();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        _logger.Technical().LogTokenProviderIsResolved(providerName);

        var provider = GetResolver().GetRequiredKeyedService<ITokenProvider>(providerName);

        _logger.Technical().LogRequestingAToken();
        var tokenResultInfo = await provider.GetTokenAsync(_settings, new object()).ConfigureAwait(false);

        if (tokenResultInfo.IsFailed)
        {
            _logger.Technical().LogNoAuthenticationTokenCanBeRetrieve(providerName);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var tokenInfo = tokenResultInfo.Value;

        // check if the token is still valid.
        // This is due to gRPC. It is possible that a gRPC streaming call is not closed and the token in the HttpContext is expired.
        // It is also possible this with OAuth where the token is added to the Identity and used like this => no refresh of the token is possible.
        if (tokenInfo.ExpiresOnUtc < DateTime.UtcNow)
        {
            _logger.Technical().LogTokenIsExpired();
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        var scheme = tokenInfo.TokenType;
        _logger.Technical().LogSchemeInfo(scheme);

        _logger.Technical().LogDebug("Remove any Bearer token attached.");
        request.Headers.Remove("Bearer");

        if (SourceArray.Any(s => s.Equals(scheme, StringComparison.OrdinalIgnoreCase)))
        {
            // Add the token to the Authorization header for Basic or Bearer.
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, tokenInfo.Token);
        }
        else
        {
            // Add the token to the header for a custom scheme.
            request.Headers.Add(scheme, tokenInfo.Token);
        }

        // Add ActivityId if founded!
        if (null == _applicationContext?.Principal)
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        if (! string.IsNullOrWhiteSpace(_applicationContext.ActivityID))
        {
            _logger.Technical().LogPrincipalActivityId(_applicationContext.ActivityID);
            request.Headers.Add("activityid", _applicationContext.ActivityID);
        }

        _logger.Technical().LogCultureRequested(_applicationContext.Principal.Profile.CurrentCulture.TwoLetterISOLanguageName);
        var culture = _applicationContext.Principal.Profile.CurrentCulture.TwoLetterISOLanguageName;
        if (! string.IsNullOrWhiteSpace(culture))
        {
            request.Headers.Add("culture", culture);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
