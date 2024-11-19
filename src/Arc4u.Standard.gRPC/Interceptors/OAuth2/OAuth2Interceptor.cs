using System.Security.Claims;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arc4u.gRPC.Interceptors;

/// <summary>
/// Inject in the Metadata's message the Bearer token of the authenticated user.
/// </summary>
public class OAuth2Interceptor : Interceptor
{
    public OAuth2Interceptor(IScopedServiceProviderAccessor serviceProviderAccessor, ILogger<OAuth2Interceptor> logger, IKeyValueSettings keyValuesSettings)
    {
        _serviceProviderAccessor = serviceProviderAccessor;

        _logger = logger;

        _settings = keyValuesSettings ?? throw new ArgumentNullException(nameof(keyValuesSettings));
    }

    /// <summary>
    /// This is the constructor to use in a Client scenario like a Wpf or a MAUI or a console.
    /// The <see cref="IApplicationContext"/> and the <see cref="ITokenProvider"/> are not scoped to an httpRequest or a job, etc...
    /// </summary>
    /// <param name="containerResolve"><see cref="IContainerResolve"/></param>
    /// <param name="logger"><see cref="ILogger"/></param>
    /// <param name="keyValuesSettings">Property bag for the token povider.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public OAuth2Interceptor(IContainerResolve containerResolve, ILogger<OAuth2Interceptor> logger, IKeyValueSettings keyValuesSettings)
    {
        _containerResolve = containerResolve ?? throw new ArgumentNullException(nameof(containerResolve));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _settings = keyValuesSettings ?? throw new ArgumentNullException(nameof(keyValuesSettings));
    }

    private readonly IKeyValueSettings _settings;
    private readonly ILogger<OAuth2Interceptor> _logger;
    private readonly IScopedServiceProviderAccessor? _serviceProviderAccessor = null;
    private readonly IContainerResolve? _containerResolve = null;

    private IContainerResolve? GetResolver() => _containerResolve ?? _serviceProviderAccessor?.ServiceProvider?.GetService<IContainerResolve>();

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        AddBearerTokenCallerMetadata(ref context);

        return continuation(request, context);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        AddBearerTokenCallerMetadata(ref context);

        return continuation(request, context);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        AddBearerTokenCallerMetadata(ref context);

        return continuation(context);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        AddBearerTokenCallerMetadata(ref context);

        return continuation(request, context);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        AddBearerTokenCallerMetadata(ref context);

        return continuation(context);
    }
    private void AddBearerTokenCallerMetadata<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
                where TRequest : class
                where TResponse : class
    {
        var headers = context.Options.Headers;

        // Call doesn't have a headers collection to add to.
        // Need to create a new context with headers for the call.
        if (headers == null)
        {
            headers = new Metadata();
            var options = context.Options.WithHeaders(headers);
            context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
        }

        // if we have already an "Authorization" defined, we can skip the code here.
        if (null != headers.GetValue("authorization"))
        {
            _logger.Technical().System($"Authorization header found. Skip adding a bearer token for AuthenticationType: {_settings.Values[TokenKeys.AuthenticationTypeKey]}.").Log();
            return;
        }

        var applicationContext = GetCallContext(out var containerResolve);

        if (_settings is null || applicationContext is null || containerResolve is null)
        {
            _logger.Technical().System($"No settings or application context is defined with {GetType().Name}, Check next Delegate Handler").Log();
            return;
        }

        if (!_settings.Values.TryGetValue(TokenKeys.AuthenticationTypeKey, out var authenticationType))
        {
            _logger.Technical().System($"No authentication type for {this.GetType().Name}, Check next Interceptor").Log();
            return;
        }

        var inject = authenticationType.Equals("inject", StringComparison.InvariantCultureIgnoreCase);

        // Skip (BE scenario) if the parameter is an identity and the settings doesn't correspond to the identity's type.
        if (!inject
            &&
            applicationContext.Principal.Identity is ClaimsIdentity claimsIdentity
            &&
            !claimsIdentity.AuthenticationType.Equals(_settings.Values[TokenKeys.AuthenticationTypeKey], StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        claimsIdentity = applicationContext.Principal?.Identity as ClaimsIdentity;

        // But in case we inject we need something in the identity!
        if (claimsIdentity is null && !inject)
        {
            return;
        }

        try
        {
            var provider = containerResolve.Resolve<ITokenProvider>(_settings.Values[TokenKeys.ProviderIdKey]);
            var tokenInfo = provider.GetTokenAsync(_settings, claimsIdentity).Result;

            if (tokenInfo.ExpiresOnUtc < DateTime.UtcNow)
            {
                _logger.Technical().System($"Token is expired! Next Interceptor will be called.").Log();
                return;
            }

            var scheme = inject ? tokenInfo.TokenType : "Bearer";
            _logger.Technical().System($"Add the {scheme} token to provide authentication evidence.").Log();

            if (new string[] { "Bearer", "Basic" }.Any(s => s.Equals(scheme, StringComparison.InvariantCultureIgnoreCase)))
            {
                headers.Add("authorization", $"{scheme} {tokenInfo.Token}");
            }
            else
            {
                headers.Add(scheme, tokenInfo.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.Technical().Exception(ex).Log();
        }

        // Add culture and activityID if exists!
        if (null != applicationContext?.Principal)
        {
            var culture = applicationContext.Principal.Profile?.CurrentCulture?.TwoLetterISOLanguageName;
            if (null != culture && null == headers.GetValue("culture"))
            {
                _logger.Technical().System($"Add the current culture to the request: {applicationContext.Principal.Profile?.CurrentCulture?.TwoLetterISOLanguageName}").Log();
                headers.Add("culture", culture);
            }
        }
    }

    private IApplicationContext? GetCallContext(out IContainerResolve? containerResolve)
    {
        containerResolve = GetResolver();

        return containerResolve?.Resolve<IApplicationContext>();
    }
}
