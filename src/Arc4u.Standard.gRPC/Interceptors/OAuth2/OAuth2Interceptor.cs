using System;
using System.Linq;
using System.Security.Claims;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Principal;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.gRPC.Interceptors;

/// <summary>
/// Inject in the Metadata's message the Bearer token of the authenticated user.
/// </summary>
public class OAuth2Interceptor : Interceptor
{
    public OAuth2Interceptor(IHttpContextAccessor accessor, ILogger<OAuth2Interceptor> logger, IKeyValueSettings keyValuesSettings)
    {
#if NETSTANDARD2_0
        if (keyValuesSettings is null)
        {
            throw new ArgumentNullException(nameof(keyValuesSettings));
        }
#else
        ArgumentNullException.ThrowIfNull(keyValuesSettings);
#endif
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

        _settings = keyValuesSettings;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public OAuth2Interceptor(IContainerResolve containerResolve, ILogger<OAuth2Interceptor> logger, IKeyValueSettings keyValuesSettings)
    {
#if NETSTANDARD2_0
        if (keyValuesSettings is null)
        {
            throw new ArgumentNullException(nameof(keyValuesSettings));
        }
#else
        ArgumentNullException.ThrowIfNull(keyValuesSettings);
#endif
        _container = containerResolve;

        _settings = keyValuesSettings;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    private IContainerResolve _container;
    private object _platformParameter;
    private IKeyValueSettings _settings;
    private readonly ILogger<OAuth2Interceptor> _logger;
    private readonly IHttpContextAccessor _accessor;
    private readonly string _settingsName;

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

        if (null != _accessor)
        {
            _container = _accessor.HttpContext.RequestServices.GetService<IContainerResolve>();
        }

        // As this is global for an handler, this can be saved at the level of the class.
        // We do this here only when using an accessor => the IContainerResolve is only available in the context of a call not
        // when the JwtHttpHandler is built.
        if (null == _settings && !String.IsNullOrWhiteSpace(_settingsName))
        {
            if (!_container.TryResolve(_settingsName, out _settings))
            {
                _logger.Technical().Debug($"No settings for {_settingsName} is found.").Log();
            }
        }

        if (_container.TryResolve<IApplicationContext>(out var applicationContext))
        {
            _platformParameter = _platformParameter ?? applicationContext?.Principal?.Identity as ClaimsIdentity;
        }

        if (!_settings.Values.TryGetValue(TokenKeys.AuthenticationTypeKey, out var authenticationType))
        {
            _logger.Technical().System($"No authentication type for {this.GetType().Name}, Check next Interceptor").Log();
            return;
        }

        var inject = authenticationType.Equals("inject", StringComparison.InvariantCultureIgnoreCase);

        // Skip (BE scenario) if the parameter is an identity and the settings doesn't correspond to the identity's type.
        if (_platformParameter is ClaimsIdentity claimsIdentity
            &&
            !inject
            &&
            !claimsIdentity.AuthenticationType.Equals(_settings.Values[TokenKeys.AuthenticationTypeKey], StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }

        // But in case we inject we need something in the platformParameter!
        if (null == _platformParameter && !inject)
        {
            return;
        }

        try
        {
            var provider = _container.Resolve<ITokenProvider>(_settings.Values[TokenKeys.ProviderIdKey]);
            var tokenInfo = provider.GetTokenAsync(_settings, _platformParameter).Result;

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
}
