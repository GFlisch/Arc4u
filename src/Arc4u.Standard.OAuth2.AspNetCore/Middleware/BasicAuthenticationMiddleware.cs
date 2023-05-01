using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Arc4u.Standard.OAuth2.Middleware;

public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BasicAuthenticationContextOption _option;
    private readonly IContainerResolve _container;
    private readonly ILogger<BasicAuthenticationMiddleware> _logger;
    private readonly ITokenCache _tokenCache;
    private readonly ICredentialTokenProvider _provider;
    private readonly bool _hasProvider;
    private readonly ActivitySource _activitySource;

    public BasicAuthenticationMiddleware(RequestDelegate next, IContainerResolve container, BasicAuthenticationContextOption option)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        ArgumentNullException.ThrowIfNull(container);

        ArgumentNullException.ThrowIfNull(option);

        if (null == option.Settings)
        {
            throw new ArgumentNullException(nameof(option.Settings));
        }

        _logger = container.Resolve<ILogger<BasicAuthenticationMiddleware>>();

        if (!string.IsNullOrEmpty(option.DefaultUpn))
        {
            if (!Regex.IsMatch(option.DefaultUpn, @"^@([a-zA-Z0-9]+\.[a-zA-Z0-9]+)"))
            {
                _logger.Technical().Warning($"Bad upn format, we expect a @ and one point.").Log();
                option.DefaultUpn = string.Empty;
            }
            else
            {
                _logger.Technical().Information($"Default upn: {option.DefaultUpn}.").Log();
            }
        }

        if (option.Settings.Values.ContainsKey(TokenKeys.ProviderIdKey))
        {
            _hasProvider = container.TryResolve(option.Settings.Values[TokenKeys.ProviderIdKey], out _provider);
            if (!_hasProvider)
            {
                _logger.Technical().Error($"No token provider was found with resolution name equal to: {option.Settings.Values[TokenKeys.ProviderIdKey]}.").Log();
            }
        }
        else
        {
            _logger.Technical().Error($"No token provider resolution name is defined in your settings!").Log();
        }

        if (!_hasProvider)
        {
            _logger.Technical().Error($"Basic Authentication capability is deactivated!").Log();
        }

        if (!container.TryResolve(out _tokenCache))
        {
            _logger.Technical().Error($"No token ache are defined for Basic Authentication.").Log();
        }

        _option = option;
        _container = container;

        _activitySource = container.Resolve<IActivitySourceFactory>()?.GetArc4u();
    }

    public async Task Invoke(HttpContext context)
    {
        if (!_hasProvider)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        try
        {
            TokenInfo tokenInfo;

            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                if (context.Request.Headers.TryGetValue("Authorization", out var authzValue) && authzValue.Any(value => value.Contains("Basic")))
                {
                    // Add Telemetry.
                    using (var activity = _activitySource?.StartActivity("BasicAuthentication", ActivityKind.Producer))
                    {
                        var cacheKey = BuildKey(authzValue);

                        tokenInfo = _tokenCache?.Get<TokenInfo>(cacheKey);
                        if (null == tokenInfo || tokenInfo.ExpiresOnUtc < DateTime.UtcNow.AddMinutes(1))
                        {
                            var credential = GetCredential(authzValue);

                            if (null != credential && credential.CredentialsEntered)
                            {
                                // Get an Access Token.
                                tokenInfo = await _provider.GetTokenAsync(_option.Settings, credential).ConfigureAwait(false);

                                _tokenCache?.Put(cacheKey, tokenInfo);
                            }
                        }

                        if (null != tokenInfo)
                        {
                            // Replace the Basic Authorization by the access token in the header.
                            var authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token).ToString();
                            context.Request.Headers.Remove("Authorization");
                            context.Request.Headers.Add("Authorization", authorization);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Technical().Exception(ex).Log();
        }

        await _next(context).ConfigureAwait(false);
    }

    private static string BuildKey(string authorizationValue)
    {
        return "Basic_" + authorizationValue.GetHashCode().ToString();
    }

    private CredentialsResult GetCredential(string authorization)
    {
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // We have a Basic authentication.
        var token = authorization.Substring("Basic ".Length).Trim();

        var pair = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var ix = pair.IndexOf(':');
        if (ix == -1)
        {
            _logger.Technical().Warning($"Basic authentication is not well formed.").Log();
            return new CredentialsResult(false);
        }

        var username = pair.Substring(0, ix);
        var pwd = pair.Substring(ix + 1);

        _logger.Technical().System($@"Username receives is: {username}.").Log();

        // is username format ok?
        if (!Regex.IsMatch(username, @"([a-zA-Z0-9]+@[a-zA-Z0-9]+\.[a-zA-Z0-9]+)|([a-zA-Z0-9]+\\[a-zA-Z0-9]+)|([a-zA-Z0-9]+/[a-zA-Z0-9]+)"))
        {
            username = $"{username}{_option.DefaultUpn.Trim()}";
            _logger.Technical().System($@"Username is changed to: {username}.").Log();
        }

        return new CredentialsResult(true, username, pwd);
    }
}
