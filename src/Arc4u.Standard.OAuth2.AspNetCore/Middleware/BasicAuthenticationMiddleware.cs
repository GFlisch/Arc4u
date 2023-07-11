using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Arc4u.Configuration;
using Arc4u.Dependency;
using Arc4u.Diagnostics;
using Arc4u.OAuth2.Extensions;
using Arc4u.OAuth2.Options;
using Arc4u.OAuth2.Security.Principal;
using Arc4u.OAuth2.Token;
using Arc4u.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arc4u.OAuth2.Middleware;

public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly BasicAuthenticationSettingsOptions _options;
    private readonly ILogger<BasicAuthenticationMiddleware> _logger;
    private readonly ActivitySource _activitySource;

    public BasicAuthenticationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));

        ArgumentNullException.ThrowIfNull(serviceProvider);

        _logger = serviceProvider.GetRequiredService<ILogger<BasicAuthenticationMiddleware>>();

        _options = serviceProvider.GetRequiredService<IOptionsMonitor<BasicAuthenticationSettingsOptions>>().CurrentValue;

        if (_options.BasicSettings is null)
        {
            throw new ConfigurationException("Settings collection for basic authentication cannot be null");
        }

        var container = serviceProvider.GetRequiredService<IContainerResolve>();

        if (_options.BasicSettings.Values.ContainsKey(TokenKeys.ProviderIdKey))
        {
            if (!container.TryResolve<ICredentialTokenProvider>(_options.BasicSettings.Values[TokenKeys.ProviderIdKey], out _))
            {
                throw new ConfigurationException($"No token provider ICredentialTokenProvider is defined with ProviderId {_options.BasicSettings.Values[TokenKeys.ProviderIdKey]}!");
            }
        }
        else
        {
            throw new ConfigurationException("No token provider resolution name is defined in your settings!");
        }
        if (!container.TryResolve<ITokenCache>(out _))
        {
            _logger.Technical().Error($"No token cache is defined for Basic Authentication.").Log();
        }

        _activitySource = container.Resolve<IActivitySourceFactory>().GetArc4u();
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var credential = GetBasicCredential(context);

            if (!credential.CredentialsEntered)
            {
                credential = GetSecretCredential(context);
            }

            if (!credential.CredentialsEntered)
            {
                await _next(context).ConfigureAwait(false);
                return;
            }
            var tokenCache = context.RequestServices.GetService<ITokenCache>();

            var cacheKey = BuildKey(credential);
            var tokenInfo = tokenCache?.Get<TokenInfo>(cacheKey);

            if (null == tokenInfo || tokenInfo.ExpiresOnUtc < DateTime.UtcNow.AddMinutes(1))
            {
                tokenInfo = await GetTokenFromCredentialAsync(credential, context.RequestServices).ConfigureAwait(false);

                tokenCache?.Put(cacheKey, tokenInfo);
            }

            if (tokenInfo is not null)
            {
                // Replace the Basic Authorization by the access token in the header.
                var authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token).ToString();
                context.Request.Headers.Remove("Authorization");
                context.Request.Headers.Add("Authorization", authorization);
            }
            else
            {
                _logger.Technical().LogError($"No token has been created for the user {credential.Upn}.");
            }
        }
        catch (Exception ex)
        {
            _logger.Technical().Exception(ex).Log();
        }

        await _next(context).ConfigureAwait(false);
    }

    private async Task<TokenInfo?> GetTokenFromCredentialAsync(CredentialsResult credential, IServiceProvider serviceProvider)
    {
        if (credential.CredentialsEntered)
        {
            var container = serviceProvider.GetRequiredService<IContainerResolve>();

            var provider = container.Resolve<ICredentialTokenProvider>(_options.BasicSettings.Values[TokenKeys.ProviderIdKey]);

            // Get an Access Token.
            return await provider.GetTokenAsync(_options.BasicSettings, credential).ConfigureAwait(false);
        }

        return null;
    }

    private static string BuildKey([DisallowNull] CredentialsResult credentialsResult)
    {
        ArgumentNullException.ThrowIfNull(credentialsResult);

        return "Basic-" + $"{credentialsResult.Upn}-{credentialsResult.Password}".GetHashCode().ToString(CultureInfo.InvariantCulture);
    }

    private CredentialsResult GetSecretCredential([DisallowNull] HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!_options.CertificateHeaderOptions.Any())
        {
            return new CredentialsResult(false);
        }

        foreach (var cert in _options.CertificateHeaderOptions)
        {
            var secret = GetClientSecretIfExist(context, cert.Key);

            // Decrypt the content!
            if (!string.IsNullOrWhiteSpace(secret))
            {
                var pair = cert.Value.Decrypt(secret);

                return new CredentialsResult(false).ExtractCredential(pair, _logger, _options.DefaultUpn);
            }
        }

        return new CredentialsResult(false);
    }

    private static string? GetClientSecretIfExist([DisallowNull] HttpContext context, [DisallowNull] string key)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(key);

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(key);
        }

        var clientSecret = context.Request.Headers.FirstOrDefault(header => header.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value.FirstOrDefault();

        // Check if this is in the url query string.
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            clientSecret = context.Request.Query.FirstOrDefault(p => p.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).Value.FirstOrDefault();
        }

        return clientSecret;
    }

    private CredentialsResult GetBasicCredential([DisallowNull] HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authzValue) && authzValue.Any(value => value.Contains("Basic")))
        {
            var authorization = authzValue.First(basic => basic.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase));

            // We have a Basic authentication.
            var token = authorization["Basic ".Length..].Trim();

            var pair = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            return new CredentialsResult(false).ExtractCredential(pair, _logger, _options.DefaultUpn);
        }

        return new CredentialsResult(false);
    }
}
